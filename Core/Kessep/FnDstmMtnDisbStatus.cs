// Program: FN_DSTM_MTN_DISB_STATUS, ID: 371829844, model: 746.
// Short name: SWEDSTMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DSTM_MTN_DISB_STATUS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDstmMtnDisbStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DSTM_MTN_DISB_STATUS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDstmMtnDisbStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDstmMtnDisbStatus.
  /// </summary>
  public FnDstmMtnDisbStatus(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *** Made various fixes to transaction. A.Kinney 02/27/97 ***
    // ----------------------
    // Made various fixes to the Prad  N.Engoor 02/09/97
    // ----------------------
    // *** Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // **** NEXTRAN / Security Logic ****
    export.Standard.Assign(import.Standard);

    // *** if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // *** Move all IMPORTs to EXPORTs.
    export.DisbursementStatus.Assign(import.DisbursementStatus);
    export.Prompt.Text1 = import.Prompt.Text1;
    export.TypeStatusAudit.Assign(import.TypeStatusAudit);
    export.HiddenDisbursementStatus.Assign(import.Hidden);
    export.HiddenDisplayOk.Flag = import.HiddenDisplayOk.Flag;

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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETDSTL"))
    {
      if (IsEmpty(import.Selected.Code))
      {
        MoveDisbursementStatus(export.HiddenDisbursementStatus,
          export.DisbursementStatus);
      }
      else
      {
        MoveDisbursementStatus(import.Selected, export.DisbursementStatus);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // *** The logic assumes that a record cannot be
    // UPDATEd or DELETEd without first being displayed.
    // Therefore, a key change with either command is invalid.
    if ((Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE")) &&
      !Equal(import.DisbursementStatus.Code, import.Hidden.Code))
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field = GetField(export.DisbursementStatus, "code");

      field.Error = true;

      return;
    }

    // *** If the key field is blank, certain commands are not allowed.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "PREV")) && IsEmpty(export.DisbursementStatus.Code))
    {
      var field = GetField(export.DisbursementStatus, "code");

      field.Error = true;

      export.DisbursementStatus.Name = "";
      export.DisbursementStatus.Description =
        Spaces(DisbursementStatus.Description_MaxLength);
      export.DisbursementStatus.EffectiveDate = null;
      export.DisbursementStatus.DiscontinueDate = null;
      export.Prompt.Text1 = "+";
      ExitState = "KEY_FIELD_IS_BLANK";

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.DisbursementStatus.Name))
      {
        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        var field = GetField(export.DisbursementStatus, "name");

        field.Error = true;

        return;
      }
    }

    // *********************************
    //        Main CASE OF COMMAND.
    // *********************************
    switch(TrimEnd(global.Command))
    {
      case "DSTL":
        // *** Transfer was changed to a link,
        // so RETURN is now used instead.  2/28/97 A.Kinney
        break;
      case "DISPLAY":
        // *** Calls the display module.
        UseFnReadDisbursementStatus();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *** Set the hidden key field to that of the new record.
          export.HiddenDisbursementStatus.Assign(export.DisbursementStatus);

          // *** If discontinue date is equal to maximum date then
          // blank out the field
          var field = GetField(export.DisbursementStatus, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementStatus.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (Lt(export.DisbursementStatus.DiscontinueDate, Now().Date) && !
            Equal(export.DisbursementStatus.DiscontinueDate, null))
          {
            var field3 = GetField(export.DisbursementStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementStatus, "discontinueDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementStatus, "description");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (Equal(export.DisbursementStatus.DiscontinueDate, local.Max.Date))
          {
            export.DisbursementStatus.DiscontinueDate = local.Min.Date;
          }

          export.HiddenDisplayOk.Flag = "Y";
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          var field3 = GetField(export.DisbursementStatus, "code");

          field3.Error = true;

          export.DisbursementStatus.Name = "";
          export.DisbursementStatus.Description =
            Spaces(DisbursementStatus.Description_MaxLength);
          export.DisbursementStatus.EffectiveDate = null;
          export.DisbursementStatus.DiscontinueDate = null;

          var field4 = GetField(export.DisbursementStatus, "code");

          field4.Error = true;

          // *** Set the hidden key field to spaces or zero.
          export.HiddenDisbursementStatus.SystemGeneratedIdentifier = 0;
        }

        export.Prompt.Text1 = "+";

        break;
      case "ADD":
        // ---------------------
        // Validate effective date, is required and cannot be less than current 
        // date.
        // ---------------------
        if (Equal(export.DisbursementStatus.EffectiveDate, null))
        {
          export.DisbursementStatus.EffectiveDate = Now().Date;
        }

        if (Equal(export.DisbursementStatus.DiscontinueDate, null))
        {
          export.DisbursementStatus.DiscontinueDate =
            new DateTime(2099, 12, 31);
        }

        if (Lt(export.DisbursementStatus.EffectiveDate, Now().Date))
        {
          var field = GetField(export.DisbursementStatus, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        // *** Validate the discontinue date, discontinue date cannot be prior 
        // to effective date and cannot be less than current date
        if (Lt(export.DisbursementStatus.DiscontinueDate, Now().Date))
        {
          var field = GetField(export.DisbursementStatus, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

          return;
        }

        if (!Lt(export.DisbursementStatus.EffectiveDate,
          export.DisbursementStatus.DiscontinueDate))
        {
          var field = GetField(export.DisbursementStatus, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          return;
        }

        // *** Calls the create module.
        // -------------------------------------
        // This CAB checks for any date overlaps for the same code value before 
        // creating a new record.
        // -------------------------------------
        UseFnCreateDisbursementStatus();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *** Set the hidden key field to that of the new record.
          export.HiddenDisbursementStatus.Assign(export.DisbursementStatus);

          var field = GetField(export.DisbursementStatus, "code");

          field.Color = "cyan";
          field.Protected = true;

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          if (export.DisbursementStatus.SystemGeneratedIdentifier != 0)
          {
            var field = GetField(export.DisbursementStatus, "code");

            field.Color = "cyan";
            field.Protected = true;
          }

          var field3 = GetField(export.DisbursementStatus, "discontinueDate");

          field3.Error = true;

          var field4 = GetField(export.DisbursementStatus, "effectiveDate");

          field4.Error = true;
        }
        else
        {
        }

        break;
      case "UPDATE":
        if (AsChar(export.HiddenDisplayOk.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ----------------------
        // Validate the effective date and discontinue date, discontinue date 
        // cannot be prior to effective date and effective cannot be prior to
        // current date as well as cannot be greater than or equal to
        // discontinue date.
        // ----------------------
        if (Equal(export.DisbursementStatus.EffectiveDate, null))
        {
          export.DisbursementStatus.EffectiveDate = Now().Date;
        }

        if (Equal(export.DisbursementStatus.DiscontinueDate, null))
        {
          export.DisbursementStatus.DiscontinueDate =
            new DateTime(2099, 12, 31);
        }

        if (Equal(export.DisbursementStatus.DiscontinueDate,
          export.HiddenDisbursementStatus.DiscontinueDate) && Lt
          (export.DisbursementStatus.DiscontinueDate, Now().Date))
        {
          var field3 = GetField(export.DisbursementStatus, "code");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DisbursementStatus, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementStatus, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.DisbursementStatus, "discontinueDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.DisbursementStatus, "description");

          field7.Color = "cyan";
          field7.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.DisbursementStatus.DiscontinueDate,
          export.HiddenDisbursementStatus.DiscontinueDate) || !
          Equal(export.DisbursementStatus.EffectiveDate,
          export.HiddenDisbursementStatus.EffectiveDate))
        {
          var field = GetField(export.DisbursementStatus, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Equal(export.DisbursementStatus.EffectiveDate,
            export.HiddenDisbursementStatus.EffectiveDate) && Lt
            (export.DisbursementStatus.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!Equal(export.DisbursementStatus.EffectiveDate,
            export.HiddenDisbursementStatus.EffectiveDate) && Lt
            (export.DisbursementStatus.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementStatus, "effectiveDate");

            field3.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.DisbursementStatus.DiscontinueDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementStatus, "discontinueDate");

            field3.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.DisbursementStatus.EffectiveDate,
            export.DisbursementStatus.DiscontinueDate))
          {
            var field3 = GetField(export.DisbursementStatus, "discontinueDate");

            field3.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        // *** Calls the update module.
        // -------------------------------------
        // This CAB checks for any date overlaps with existing records having 
        // the same code value before creating a new record.
        // -------------------------------------
        UseFnUpdateDisbursementStatus();

        var field1 = GetField(export.DisbursementStatus, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        if (Lt(export.DisbursementStatus.EffectiveDate, Now().Date))
        {
          var field = GetField(export.DisbursementStatus, "name");

          field.Color = "cyan";
          field.Protected = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field3 = GetField(export.DisbursementStatus, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.DisbursementStatus, "discontinueDate");

          field4.Error = true;
        }
        else
        {
        }

        break;
      case "DELETE":
        if (AsChar(export.HiddenDisplayOk.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        var field2 = GetField(export.DisbursementStatus, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        if (Equal(export.DisbursementStatus.DiscontinueDate, null))
        {
        }
        else if (Lt(export.DisbursementStatus.DiscontinueDate, Now().Date))
        {
          var field3 = GetField(export.DisbursementStatus, "name");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DisbursementStatus, "effectiveDate");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementStatus, "discontinueDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.DisbursementStatus, "description");

          field6.Color = "cyan";
          field6.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        if (Equal(export.DisbursementStatus.EffectiveDate, null))
        {
        }
        else if (Lt(export.DisbursementStatus.EffectiveDate, Now().Date))
        {
          var field3 = GetField(export.DisbursementStatus, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DisbursementStatus, "name");

          field4.Color = "cyan";
          field4.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        // *** Calls the delete module.
        UseFnDeleteDisbursementStatus();

        if (IsExitState("CANNOT_DELETE_EFFECTIVE_RECORD"))
        {
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to spaces or zero.
          export.DisbursementStatus.Assign(local.Blank);
          export.HiddenDisbursementStatus.Assign(local.Blank);

          var field = GetField(export.DisbursementStatus, "code");

          field.Color = "";
          field.Protected = false;

          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else
        {
        }

        break;
      case "LIST":
        if (AsChar(export.Prompt.Text1) == 'S')
        {
          export.Prompt.Text1 = "+";
          ExitState = "ECO_LNK_TO_LST_DISB_STATUSES";
        }
        else
        {
          if (Lt(export.DisbursementStatus.EffectiveDate, Now().Date) && export
            .DisbursementStatus.SystemGeneratedIdentifier != 0)
          {
            var field3 = GetField(export.DisbursementStatus, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementStatus, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementStatus, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (Lt(export.DisbursementStatus.DiscontinueDate, Now().Date) && !
            Equal(export.DisbursementStatus.DiscontinueDate, null) && export
            .DisbursementStatus.SystemGeneratedIdentifier != 0)
          {
            var field3 = GetField(export.DisbursementStatus, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementStatus, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementStatus, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementStatus, "discontinueDate");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.DisbursementStatus, "description");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          var field = GetField(export.Prompt, "text1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        if (export.DisbursementStatus.SystemGeneratedIdentifier != 0)
        {
          var field = GetField(export.DisbursementStatus, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementStatus.DiscontinueDate, Now().Date) && !
            Equal(export.DisbursementStatus.DiscontinueDate, null))
          {
            var field3 = GetField(export.DisbursementStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementStatus, "discontinueDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementStatus, "description");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (Lt(export.DisbursementStatus.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementStatus, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementStatus, "effectiveDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveDisbursementStatus(DisbursementStatus source,
    DisbursementStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
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

  private void UseFnCreateDisbursementStatus()
  {
    var useImport = new FnCreateDisbursementStatus.Import();
    var useExport = new FnCreateDisbursementStatus.Export();

    useImport.DisbursementStatus.Assign(export.DisbursementStatus);

    Call(FnCreateDisbursementStatus.Execute, useImport, useExport);

    export.DisbursementStatus.Assign(useExport.DisbursementStatus);
  }

  private void UseFnDeleteDisbursementStatus()
  {
    var useImport = new FnDeleteDisbursementStatus.Import();
    var useExport = new FnDeleteDisbursementStatus.Export();

    useImport.DisbursementStatus.Assign(import.DisbursementStatus);

    Call(FnDeleteDisbursementStatus.Execute, useImport, useExport);

    export.DisbursementStatus.Assign(useExport.DisbursementStatus);
    MoveTypeStatusAudit(useExport.TypeStatusAudit, export.TypeStatusAudit);
  }

  private void UseFnReadDisbursementStatus()
  {
    var useImport = new FnReadDisbursementStatus.Import();
    var useExport = new FnReadDisbursementStatus.Export();

    useImport.Flag.Flag = import.Flag.Flag;
    useImport.DisbursementStatus.Assign(export.DisbursementStatus);

    Call(FnReadDisbursementStatus.Execute, useImport, useExport);

    export.DisbursementStatus.Assign(useExport.DisbursementStatus);
  }

  private void UseFnUpdateDisbursementStatus()
  {
    var useImport = new FnUpdateDisbursementStatus.Import();
    var useExport = new FnUpdateDisbursementStatus.Export();

    useImport.DisbursementStatus.Assign(export.DisbursementStatus);

    Call(FnUpdateDisbursementStatus.Execute, useImport, useExport);

    export.DisbursementStatus.Assign(useExport.DisbursementStatus);
    export.TypeStatusAudit.Assign(useExport.TypeStatusAudit);
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public DisbursementStatus Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public DisbursementStatus Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of ApPayorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apPayorCsePersonsWorkSet")]
    public CsePersonsWorkSet ApPayorCsePersonsWorkSet
    {
      get => apPayorCsePersonsWorkSet ??= new();
      set => apPayorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApPayorCsePerson.
    /// </summary>
    [JsonPropertyName("apPayorCsePerson")]
    public CsePerson ApPayorCsePerson
    {
      get => apPayorCsePerson ??= new();
      set => apPayorCsePerson = value;
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

    private Common flag;
    private DisbursementStatus selected;
    private Common hiddenDisplayOk;
    private Standard standard;
    private TextWorkArea prompt;
    private DisbursementStatus disbursementStatus;
    private TypeStatusAudit typeStatusAudit;
    private DisbursementStatus hidden;
    private CsePersonsWorkSet apPayorCsePersonsWorkSet;
    private CsePerson apPayorCsePerson;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
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
    /// A value of HiddenDisbursementStatus.
    /// </summary>
    [JsonPropertyName("hiddenDisbursementStatus")]
    public DisbursementStatus HiddenDisbursementStatus
    {
      get => hiddenDisbursementStatus ??= new();
      set => hiddenDisbursementStatus = value;
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

    private Common hiddenDisplayOk;
    private Standard standard;
    private TextWorkArea prompt;
    private DisbursementStatus disbursementStatus;
    private TypeStatusAudit typeStatusAudit;
    private DisbursementStatus hiddenDisbursementStatus;
    private NextTranInfo hiddenNextTranInfo;
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
    public DisbursementStatus Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DateWorkArea max;
    private DateWorkArea min;
    private DisbursementStatus blank;
    private DisbursementStatus disbursementStatus;
  }
#endregion
}

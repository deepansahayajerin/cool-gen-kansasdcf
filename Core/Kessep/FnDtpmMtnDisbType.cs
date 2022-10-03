// Program: FN_DTPM_MTN_DISB_TYPE, ID: 371831697, model: 746.
// Short name: SWEDTPMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DTPM_MTN_DISB_TYPE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDtpmMtnDisbType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DTPM_MTN_DISB_TYPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDtpmMtnDisbType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDtpmMtnDisbType.
  /// </summary>
  public FnDtpmMtnDisbType(IContext context, Import import, Export export):
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
    // AUTHOR    DATE        CHG REQ#     DESCRIPTION
    // R. Marchman 12/18/96               Add data level security
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // **** end   group A ****
    // If command is CLEAR, escape before moving Imports to Exports so that the 
    // screen is blanked out.
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // Move IMPORTs to EXPORTs.
    export.DisbursementType.Assign(import.DisbursementType);
    export.HiddenDisbursementType.Assign(import.HiddenDisbursementType);
    export.Prompt.SelectChar = import.Prompt.SelectChar;

    if (Equal(global.Command, "RETDTYP"))
    {
      if (IsEmpty(import.DisbursementType.Code))
      {
        MoveDisbursementType2(export.HiddenDisbursementType,
          export.DisbursementType);
      }

      global.Command = "DISPLAY";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ****
        UseScCabNextTranGet();
        global.Command = "DISPLAY";
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // ****
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // ****
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        global.Command = "DISPLAY";

        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
      }
    }
    else
    {
      return;
    }

    // ----------------
    // A record cannot be UPDATEd or DELETEd without first being displayed. 
    // Therefore, a key change with either command is invalid.
    // ----------------
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      import.DisbursementType.SystemGeneratedIdentifier != import
      .HiddenDisbursementType.SystemGeneratedIdentifier)
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field = GetField(export.DisbursementType, "code");

      field.Error = true;

      return;
    }

    // Validation common to DISPLAY and DELETE.
    if ((Equal(global.Command, "DISPLAY") || IsEmpty(global.Command) || Equal
      (global.Command, "DELETE")) && IsEmpty(export.DisbursementType.Code))
    {
      export.DisbursementType.Description =
        Spaces(DisbursementType.Description_MaxLength);
      export.DisbursementType.ProgramCode = "";
      export.DisbursementType.CurrentArrearsInd = "";
      export.DisbursementType.RecaptureInd = "";
      export.DisbursementType.Name = "";
      export.DisbursementType.DiscontinueDate = null;
      export.DisbursementType.EffectiveDate = null;
      export.Prompt.SelectChar = "+";
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.DisbursementType, "code");

      field.Error = true;

      return;
    }

    // Validation common to CREATE and UPDATE.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD"))
    {
      // !!!!!  After the ERD is changed to make this a 2 character code update 
      // the code to AF, NA, FC, NF.
      local.Code.CodeName = "PROGRAM";
      local.CodeValue.Cdvalue = import.DisbursementType.ProgramCode;
      UseCabValidateCodeValue();

      if (IsEmpty(import.DisbursementType.Code))
      {
        var field = GetField(export.DisbursementType, "code");

        field.Error = true;

        export.DisbursementType.Description =
          Spaces(DisbursementType.Description_MaxLength);
        export.DisbursementType.ProgramCode = "";
        export.DisbursementType.CurrentArrearsInd = "";
        export.DisbursementType.RecaptureInd = "";
        export.DisbursementType.Name = "";
        export.DisbursementType.DiscontinueDate = null;
        export.DisbursementType.EffectiveDate = null;
        ExitState = "KEY_FIELD_IS_BLANK";

        return;
      }

      if (IsEmpty(import.DisbursementType.Name))
      {
        var field = GetField(export.DisbursementType, "name");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (AsChar(import.DisbursementType.CurrentArrearsInd) != 'C' && AsChar
        (import.DisbursementType.CurrentArrearsInd) != 'A' && AsChar
        (import.DisbursementType.CurrentArrearsInd) != 'I')
      {
        var field = GetField(export.DisbursementType, "currentArrearsInd");

        field.Error = true;

        ExitState = "INVALID_VALUE";

        return;
      }

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        var field = GetField(export.DisbursementType, "programCode");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_ENTER_VALID_PROGRAM";

          return;
        }
      }

      if (AsChar(import.DisbursementType.RecaptureInd) != 'Y' && AsChar
        (import.DisbursementType.RecaptureInd) != 'N')
      {
        var field = GetField(export.DisbursementType, "recaptureInd");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

        return;
      }

      if (Equal(export.DisbursementType.EffectiveDate, null))
      {
        export.DisbursementType.EffectiveDate = Now().Date;
      }

      if (Equal(export.DisbursementType.DiscontinueDate, null))
      {
        export.DisbursementType.DiscontinueDate = new DateTime(2099, 12, 31);
      }

      if (Equal(global.Command, "ADD"))
      {
        if (Lt(export.DisbursementType.EffectiveDate, Now().Date))
        {
          var field = GetField(export.DisbursementType, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (Lt(export.DisbursementType.DiscontinueDate, Now().Date))
        {
          var field = GetField(export.DisbursementType, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

          return;
        }

        if (!Lt(export.DisbursementType.EffectiveDate,
          export.DisbursementType.DiscontinueDate))
        {
          var field = GetField(export.DisbursementType, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          return;
        }
      }
    }

    // **** begin group B ****
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

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    local.MaxDate.DiscontinueDate = UseCabSetMaximumDiscontinueDate();

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.DisbursementType.Code = export.DisbursementType.Code;
        UseFnReadDisbursementType();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // --------------------
          // Set the hidden key field to that of the new record.
          // --------------------
          MoveDisbursementType2(export.DisbursementType,
            export.HiddenDisbursementType);

          var field = GetField(export.DisbursementType, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementType.DiscontinueDate, Now().Date) && !
            Equal(export.DisbursementType.DiscontinueDate, null))
          {
            var field3 = GetField(export.DisbursementType, "discontinueDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementType, "description");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementType, "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementType, "name");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.DisbursementType, "currentArrearsInd");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 = GetField(export.DisbursementType, "recaptureInd");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 = GetField(export.DisbursementType, "programCode");

            field9.Color = "cyan";
            field9.Protected = true;
          }
          else if (Lt(export.DisbursementType.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementType, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementType, "name");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementType, "programCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementType, "currentArrearsInd");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.DisbursementType, "recaptureInd");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (IsExitState("FN0000_DISB_TYP_NF"))
        {
          export.DisbursementType.Code = local.DisbursementType.Code;

          var field = GetField(export.DisbursementType, "code");

          field.Error = true;

          export.DisbursementType.Description =
            Spaces(DisbursementType.Description_MaxLength);
          export.DisbursementType.ProgramCode = "";
          export.DisbursementType.CurrentArrearsInd = "";
          export.DisbursementType.RecaptureInd = "";
          export.DisbursementType.Name = "";
          export.DisbursementType.DiscontinueDate = null;
          export.DisbursementType.EffectiveDate = null;
        }
        else
        {
          // -----------
          // Set the hidden key field to spaces or zero.
          // -----------
          export.HiddenDisbursementType.Code = "";
          export.HiddenDisbursementType.SystemGeneratedIdentifier = 0;
        }

        export.Prompt.SelectChar = "+";

        break;
      case "LIST":
        if (AsChar(export.Prompt.SelectChar) == 'S')
        {
          MoveDisbursementType2(export.DisbursementType,
            export.HiddenDisbursementType);
          export.Prompt.SelectChar = "+";
          ExitState = "ECO_LNK_TO_LST_DISB_TYPES";

          return;
        }
        else
        {
          if (export.DisbursementType.SystemGeneratedIdentifier != 0)
          {
            var field3 = GetField(export.DisbursementType, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            if (Lt(export.DisbursementType.EffectiveDate, Now().Date))
            {
              var field4 = GetField(export.DisbursementType, "name");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.DisbursementType, "effectiveDate");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 =
                GetField(export.DisbursementType, "currentArrearsInd");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.DisbursementType, "programCode");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 = GetField(export.DisbursementType, "recaptureInd");

              field8.Color = "cyan";
              field8.Protected = true;
            }

            if (Lt(export.DisbursementType.DiscontinueDate, Now().Date) && !
              Equal(export.DisbursementType.DiscontinueDate, null))
            {
              var field4 = GetField(export.DisbursementType, "name");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.DisbursementType, "effectiveDate");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.DisbursementType, "discontinueDate");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.DisbursementType, "description");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.DisbursementType, "currentArrearsInd");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 = GetField(export.DisbursementType, "programCode");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 = GetField(export.DisbursementType, "recaptureInd");

              field10.Color = "cyan";
              field10.Protected = true;
            }
          }

          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }

        break;
      case "ADD":
        UseFnCreateDisbursementType();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          MoveDisbursementType2(export.DisbursementType,
            export.HiddenDisbursementType);

          var field = GetField(export.DisbursementType, "code");

          field.Color = "cyan";
          field.Protected = true;

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else if (IsExitState("FN0000_DISB_TYP_AE"))
        {
          var field = GetField(export.DisbursementType, "code");

          field.Error = true;

          return;
        }
        else if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          if (export.DisbursementType.SystemGeneratedIdentifier != 0)
          {
            var field = GetField(export.DisbursementType, "code");

            field.Color = "cyan";
            field.Protected = true;
          }

          var field3 = GetField(export.DisbursementType, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.DisbursementType, "discontinueDate");

          field4.Error = true;

          return;
        }
        else
        {
        }

        return;
      case "UPDATE":
        if (!Equal(export.DisbursementType.Code,
          export.HiddenDisbursementType.Code))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        if (Equal(export.DisbursementType.DiscontinueDate,
          export.HiddenDisbursementType.DiscontinueDate) && Lt
          (export.DisbursementType.DiscontinueDate, Now().Date))
        {
          var field3 = GetField(export.DisbursementType, "code");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DisbursementType, "currentArrearsInd");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementType, "recaptureInd");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.DisbursementType, "name");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.DisbursementType, "programCode");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.DisbursementType, "effectiveDate");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.DisbursementType, "discontinueDate");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.DisbursementType, "description");

          field10.Color = "cyan";
          field10.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.DisbursementType.DiscontinueDate,
          export.HiddenDisbursementType.DiscontinueDate) || !
          Equal(export.DisbursementType.EffectiveDate,
          export.HiddenDisbursementType.EffectiveDate))
        {
          var field = GetField(export.DisbursementType, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Equal(export.DisbursementType.EffectiveDate,
            export.HiddenDisbursementType.EffectiveDate) && Lt
            (export.DisbursementType.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementType, "currentArrearsInd");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementType, "recaptureInd");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementType, "name");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementType, "programCode");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.DisbursementType, "effectiveDate");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          if (!Equal(export.DisbursementType.EffectiveDate,
            export.HiddenDisbursementType.EffectiveDate) && Lt
            (export.DisbursementType.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementType, "effectiveDate");

            field3.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.DisbursementType.DiscontinueDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementType, "discontinueDate");

            field3.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.DisbursementType.EffectiveDate,
            export.DisbursementType.DiscontinueDate))
          {
            var field3 = GetField(export.DisbursementType, "discontinueDate");

            field3.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        UseFnUpdateDisbursementType();

        var field1 = GetField(export.DisbursementType, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        if (Lt(export.DisbursementType.EffectiveDate, Now().Date))
        {
          var field3 = GetField(export.DisbursementType, "currentArrearsInd");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DisbursementType, "recaptureInd");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementType, "name");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.DisbursementType, "programCode");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.DisbursementType, "effectiveDate");

          field7.Color = "cyan";
          field7.Protected = true;
        }

        if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field3 = GetField(export.DisbursementType, "effectiveDate");

          field3.Error = true;

          var field4 = GetField(export.DisbursementType, "discontinueDate");

          field4.Error = true;

          return;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveDisbursementType2(export.DisbursementType,
            export.HiddenDisbursementType);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          return;
        }
        else if (IsExitState("FN0000_DISB_TYP_NF"))
        {
          var field = GetField(export.DisbursementType, "code");

          field.Error = true;
        }
        else if (IsExitState("FN0000_DISB_TYP_NU"))
        {
          var field = GetField(export.DisbursementType, "code");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "DELETE":
        if (!Equal(export.DisbursementType.Code,
          export.HiddenDisbursementType.Code))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        var field2 = GetField(export.DisbursementType, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        if (Equal(export.DisbursementType.DiscontinueDate, null))
        {
        }
        else if (Lt(export.DisbursementType.DiscontinueDate, Now().Date))
        {
          var field3 = GetField(export.DisbursementType, "name");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DisbursementType, "description");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementType, "effectiveDate");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.DisbursementType, "discontinueDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.DisbursementType, "programCode");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.DisbursementType, "recaptureInd");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.DisbursementType, "currentArrearsInd");

          field9.Color = "cyan";
          field9.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        if (Equal(export.DisbursementType.EffectiveDate, null))
        {
        }
        else if (Lt(export.DisbursementType.EffectiveDate, Now().Date))
        {
          var field3 = GetField(export.DisbursementType, "name");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DisbursementType, "effectiveDate");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.DisbursementType, "programCode");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.DisbursementType, "recaptureInd");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.DisbursementType, "currentArrearsInd");

          field7.Color = "cyan";
          field7.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        UseFnDeleteDisbursementType();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenDisbursementType.Code = "";
          export.HiddenDisbursementType.SystemGeneratedIdentifier = 0;
          export.DisbursementType.SystemGeneratedIdentifier = 0;
          export.DisbursementType.Code = "";
          export.DisbursementType.Name = "";
          export.DisbursementType.Description =
            Spaces(DisbursementType.Description_MaxLength);
          export.DisbursementType.CurrentArrearsInd = "";
          export.DisbursementType.ProgramCode = "";
          export.DisbursementType.RecaptureInd = "";
          export.DisbursementType.EffectiveDate = null;
          export.DisbursementType.DiscontinueDate = null;

          var field = GetField(export.DisbursementType, "code");

          field.Color = "";
          field.Protected = false;

          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else if (IsExitState("FN0000_DISB_TYP_NF"))
        {
          var field = GetField(export.DisbursementType, "code");

          field.Error = true;
        }
        else if (IsExitState("CANNOT_DELETE_EFFECTIVE_DATE"))
        {
          var field = GetField(export.DisbursementType, "effectiveDate");

          field.Error = true;
        }
        else
        {
        }

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        if (export.DisbursementType.SystemGeneratedIdentifier != 0)
        {
          var field = GetField(export.DisbursementType, "code");

          field.Color = "cyan";
          field.Protected = true;

          if (Lt(export.DisbursementType.EffectiveDate, Now().Date))
          {
            var field3 = GetField(export.DisbursementType, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementType, "currentArrearsInd");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementType, "programCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementType, "recaptureInd");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.DisbursementType, "effectiveDate");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          if (Lt(export.DisbursementType.DiscontinueDate, Now().Date) && !
            Equal(export.DisbursementType.DiscontinueDate, null))
          {
            var field3 = GetField(export.DisbursementType, "name");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DisbursementType, "currentArrearsInd");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DisbursementType, "programCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DisbursementType, "recaptureInd");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.DisbursementType, "effectiveDate");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 = GetField(export.DisbursementType, "discontinueDate");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 = GetField(export.DisbursementType, "description");

            field9.Color = "cyan";
            field9.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (Equal(export.DisbursementType.DiscontinueDate,
        local.MaxDate.DiscontinueDate))
      {
        export.DisbursementType.DiscontinueDate = null;
      }
    }
  }

  private static void MoveDisbursementType1(DisbursementType source,
    DisbursementType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.CurrentArrearsInd = source.CurrentArrearsInd;
    target.ProgramCode = source.ProgramCode;
    target.RecaptureInd = source.RecaptureInd;
    target.CashNonCashInd = source.CashNonCashInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.Description = source.Description;
  }

  private static void MoveDisbursementType2(DisbursementType source,
    DisbursementType target)
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseFnCreateDisbursementType()
  {
    var useImport = new FnCreateDisbursementType.Import();
    var useExport = new FnCreateDisbursementType.Export();

    useImport.DisbursementType.Assign(export.DisbursementType);

    Call(FnCreateDisbursementType.Execute, useImport, useExport);
  }

  private void UseFnDeleteDisbursementType()
  {
    var useImport = new FnDeleteDisbursementType.Import();
    var useExport = new FnDeleteDisbursementType.Export();

    MoveDisbursementType2(export.DisbursementType, useImport.DisbursementType);

    Call(FnDeleteDisbursementType.Execute, useImport, useExport);
  }

  private void UseFnReadDisbursementType()
  {
    var useImport = new FnReadDisbursementType.Import();
    var useExport = new FnReadDisbursementType.Export();

    useImport.Flag.Flag = import.Flag.Flag;
    MoveDisbursementType2(export.DisbursementType, useImport.DisbursementType);

    Call(FnReadDisbursementType.Execute, useImport, useExport);

    MoveDisbursementType1(useExport.DisbursementType, export.DisbursementType);
  }

  private void UseFnUpdateDisbursementType()
  {
    var useImport = new FnUpdateDisbursementType.Import();
    var useExport = new FnUpdateDisbursementType.Export();

    useImport.DisbursementType.Assign(export.DisbursementType);

    Call(FnUpdateDisbursementType.Execute, useImport, useExport);
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Common Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of HiddenDisbursementType.
    /// </summary>
    [JsonPropertyName("hiddenDisbursementType")]
    public DisbursementType HiddenDisbursementType
    {
      get => hiddenDisbursementType ??= new();
      set => hiddenDisbursementType = value;
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

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private Common flag;
    private DisbursementType disbursementType;
    private Common last;
    private DisbursementType hiddenDisbursementType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common prompt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Common Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of HiddenDisbursementType.
    /// </summary>
    [JsonPropertyName("hiddenDisbursementType")]
    public DisbursementType HiddenDisbursementType
    {
      get => hiddenDisbursementType ??= new();
      set => hiddenDisbursementType = value;
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

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private DisbursementType disbursementType;
    private Common last;
    private DisbursementType hiddenDisbursementType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common prompt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DisbursementType MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private DisbursementType disbursementType;
    private DisbursementType maxDate;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private DateWorkArea initialized;
    private Common temp;
  }
#endregion
}

// Program: FN_REFI_MTN_COST_RCVRY_FEE_INFO, ID: 371810093, model: 746.
// Short name: SWEREFIP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REFI_MTN_COST_RCVRY_FEE_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRefiMtnCostRcvryFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REFI_MTN_COST_RCVRY_FEE_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRefiMtnCostRcvryFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRefiMtnCostRcvryFeeInfo.
  /// </summary>
  public FnRefiMtnCostRcvryFeeInfo(IContext context, Import import,
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
    // ----------------------------------------
    // Date	 Name		  Request # 	Description
    // 09/05/97 E. Parker-DIR    H00027164     Removed edits on cap since it can
    // be zero.
    // ----------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.Tribunal.Assign(import.Tribunal);
    local.Tribunal.Identifier = import.Tribunal.Identifier;
    export.Fips.CountyDescription = import.Fips.CountyDescription;
    export.TribunalFeeInformation.Assign(import.TribunalFeeInformation);
    export.HiddenTribunalFeeInformation.Assign(
      import.HiddenTribunalFeeInformation);
    export.HiddenTribunal.Identifier = import.HiddenTribunal.Identifier;
    export.HiddenTribunalFeeInformation.Assign(
      import.HiddenTribunalFeeInformation);
    export.PromptTextWorkArea.Text1 = import.PromptTextWorkArea.Text1;

    if (Equal(global.Command, "RETLTRB"))
    {
      if (import.Tribunal.Identifier == 0)
      {
        local.Tribunal.Identifier = import.HiddenTribunal.Identifier;
        export.Tribunal.Identifier = import.HiddenTribunal.Identifier;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETRECF"))
    {
      if (import.TribunalFeeInformation.SystemGeneratedIdentifier == 0)
      {
        export.TribunalFeeInformation.SystemGeneratedIdentifier =
          export.HiddenTribunalFeeInformation.SystemGeneratedIdentifier;
      }

      global.Command = "DISPLAY";
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // *** If the next tran info is not equal to spaces, this implies the user 
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

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "RECF"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // **** end   group C ****
    // ----------------------------
    // The logic assumes that a record cannot be UPDATEd or DELETEd without 
    // first being displayed. Therefore, a key change with either command is
    // invalid.
    // ----------------------------
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      import.Tribunal.Identifier != import.HiddenTribunal.Identifier)
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      var field = GetField(export.Tribunal, "identifier");

      field.Error = true;

      return;
    }

    // If the key field is blank, certain commands are not allowed.
    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "RECF")) && export.Tribunal.Identifier == 0)
    {
      export.Fips.CountyDescription = "";
      export.Tribunal.Name = "";
      export.Tribunal.Identifier = 0;
      export.Tribunal.JudicialDistrict = "";
      export.Tribunal.JudicialDivision = "";
      export.TribunalFeeInformation.Cap = 0;
      export.TribunalFeeInformation.Rate = 0;
      export.TribunalFeeInformation.Description =
        Spaces(TribunalFeeInformation.Description_MaxLength);
      export.TribunalFeeInformation.DiscontinueDate = null;
      export.TribunalFeeInformation.EffectiveDate = null;
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.Tribunal, "identifier");

      field.Error = true;

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (import.TribunalFeeInformation.Rate.GetValueOrDefault() == 0)
      {
        var field = GetField(export.TribunalFeeInformation, "rate");

        field.Error = true;

        ExitState = "FN0000_RATE_REQUIRED";

        return;
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        UseFnReadCostRecoveryFeeInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *** Set the hidden key field to that of the new record.
          export.HiddenTribunal.Identifier = export.Tribunal.Identifier;
          export.HiddenTribunalFeeInformation.Assign(
            export.TribunalFeeInformation);

          if (Lt(export.TribunalFeeInformation.EffectiveDate, Now().Date))
          {
            if (Lt(export.TribunalFeeInformation.DiscontinueDate, Now().Date) &&
              !Equal(export.TribunalFeeInformation.DiscontinueDate, null))
            {
              var field16 =
                GetField(export.TribunalFeeInformation, "description");

              field16.Color = "cyan";
              field16.Protected = true;

              var field17 =
                GetField(export.TribunalFeeInformation, "discontinueDate");

              field17.Color = "cyan";
              field17.Protected = true;
            }

            var field9 = GetField(export.Fips, "countyDescription");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.Tribunal, "judicialDistrict");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.Tribunal, "judicialDivision");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 = GetField(export.Tribunal, "identifier");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.TribunalFeeInformation, "cap");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 = GetField(export.TribunalFeeInformation, "rate");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 =
              GetField(export.TribunalFeeInformation, "effectiveDate");

            field15.Color = "cyan";
            field15.Protected = true;
          }

          if (Equal(export.TribunalFeeInformation.DiscontinueDate,
            local.Max.Date))
          {
            export.TribunalFeeInformation.DiscontinueDate = local.Init1.Date;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (IsExitState("TRIBUNAL_NF"))
        {
          var field = GetField(export.Tribunal, "identifier");

          field.Error = true;
        }
        else if (IsExitState("TRIBUNAL_FEE_INFORMATION_NF"))
        {
        }
        else
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenTribunal.Identifier = 0;
        }

        export.PromptTextWorkArea.Text1 = "+";

        break;
      case "ADD":
        if (Equal(export.TribunalFeeInformation.EffectiveDate, null))
        {
          export.TribunalFeeInformation.EffectiveDate = Now().Date;
        }

        if (Equal(export.TribunalFeeInformation.DiscontinueDate, null))
        {
          export.TribunalFeeInformation.DiscontinueDate =
            new DateTime(2099, 12, 31);
        }

        var field1 = GetField(export.Fips, "countyDescription");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Tribunal, "judicialDistrict");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Tribunal, "judicialDivision");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Tribunal, "identifier");

        field4.Color = "cyan";
        field4.Protected = true;

        if (Lt(export.TribunalFeeInformation.EffectiveDate, Now().Date))
        {
          var field = GetField(export.TribunalFeeInformation, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (Lt(export.TribunalFeeInformation.DiscontinueDate, Now().Date))
        {
          var field =
            GetField(export.TribunalFeeInformation, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

          return;
        }

        if (!Lt(export.TribunalFeeInformation.EffectiveDate,
          export.TribunalFeeInformation.DiscontinueDate))
        {
          var field =
            GetField(export.TribunalFeeInformation, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          return;
        }

        UseCreateCostRecoveryFeeInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *** Set the hidden key field to that of the new record.
          export.HiddenTribunal.Identifier = export.Tribunal.Identifier;
          export.HiddenTribunalFeeInformation.Assign(
            export.TribunalFeeInformation);
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          var field9 = GetField(export.TribunalFeeInformation, "effectiveDate");

          field9.Error = true;

          var field10 =
            GetField(export.TribunalFeeInformation, "discontinueDate");

          field10.Error = true;
        }
        else
        {
        }

        break;
      case "UPDATE":
        if (Equal(export.TribunalFeeInformation.EffectiveDate, null))
        {
          export.TribunalFeeInformation.EffectiveDate = Now().Date;
        }

        if (Equal(export.TribunalFeeInformation.DiscontinueDate, null))
        {
          export.TribunalFeeInformation.DiscontinueDate =
            new DateTime(2099, 12, 31);
        }

        var field5 = GetField(export.Fips, "countyDescription");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Tribunal, "judicialDistrict");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Tribunal, "judicialDivision");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.Tribunal, "identifier");

        field8.Color = "cyan";
        field8.Protected = true;

        if (Equal(export.TribunalFeeInformation.DiscontinueDate,
          export.HiddenTribunalFeeInformation.DiscontinueDate) && Lt
          (export.TribunalFeeInformation.DiscontinueDate, Now().Date))
        {
          var field9 = GetField(export.TribunalFeeInformation, "description");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.TribunalFeeInformation, "discontinueDate");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.TribunalFeeInformation, "cap");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.TribunalFeeInformation, "rate");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 =
            GetField(export.TribunalFeeInformation, "effectiveDate");

          field13.Color = "cyan";
          field13.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.TribunalFeeInformation.DiscontinueDate,
          export.HiddenTribunalFeeInformation.DiscontinueDate) || !
          Equal(export.TribunalFeeInformation.EffectiveDate,
          export.HiddenTribunalFeeInformation.EffectiveDate))
        {
          if (Equal(export.TribunalFeeInformation.EffectiveDate,
            export.HiddenTribunalFeeInformation.EffectiveDate) && Lt
            (export.TribunalFeeInformation.EffectiveDate, Now().Date))
          {
            var field9 = GetField(export.TribunalFeeInformation, "cap");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.TribunalFeeInformation, "rate");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.TribunalFeeInformation, "effectiveDate");

            field11.Color = "cyan";
            field11.Protected = true;
          }

          if (!Equal(export.TribunalFeeInformation.EffectiveDate,
            export.HiddenTribunalFeeInformation.EffectiveDate) && Lt
            (export.TribunalFeeInformation.EffectiveDate, Now().Date))
          {
            var field =
              GetField(export.TribunalFeeInformation, "effectiveDate");

            field.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.TribunalFeeInformation.DiscontinueDate, Now().Date))
          {
            var field =
              GetField(export.TribunalFeeInformation, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.TribunalFeeInformation.EffectiveDate,
            export.TribunalFeeInformation.DiscontinueDate))
          {
            var field =
              GetField(export.TribunalFeeInformation, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        UseUpdateCostRecoveryFeeInfo();

        if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
        {
          if (Equal(export.TribunalFeeInformation.EffectiveDate,
            export.HiddenTribunalFeeInformation.EffectiveDate) && Lt
            (export.TribunalFeeInformation.EffectiveDate, Now().Date))
          {
            var field11 = GetField(export.TribunalFeeInformation, "cap");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 = GetField(export.TribunalFeeInformation, "rate");

            field12.Color = "cyan";
            field12.Protected = true;
          }

          var field9 = GetField(export.TribunalFeeInformation, "effectiveDate");

          field9.Error = true;

          var field10 =
            GetField(export.TribunalFeeInformation, "discontinueDate");

          field10.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (Lt(export.TribunalFeeInformation.EffectiveDate, Now().Date))
          {
            var field9 = GetField(export.TribunalFeeInformation, "cap");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.TribunalFeeInformation, "rate");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.TribunalFeeInformation, "effectiveDate");

            field11.Color = "cyan";
            field11.Protected = true;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
        }

        break;
      case "DELETE":
        if (Lt(export.TribunalFeeInformation.EffectiveDate, Now().Date))
        {
          if (Lt(export.TribunalFeeInformation.DiscontinueDate, Now().Date) && !
            Equal(export.TribunalFeeInformation.DiscontinueDate, null))
          {
            var field16 =
              GetField(export.TribunalFeeInformation, "description");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 =
              GetField(export.TribunalFeeInformation, "discontinueDate");

            field17.Color = "cyan";
            field17.Protected = true;
          }

          var field9 = GetField(export.Fips, "countyDescription");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.Tribunal, "judicialDistrict");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.Tribunal, "judicialDivision");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Tribunal, "identifier");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.TribunalFeeInformation, "cap");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.TribunalFeeInformation, "rate");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 =
            GetField(export.TribunalFeeInformation, "effectiveDate");

          field15.Color = "cyan";
          field15.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        UseDeleteCostRecoveryFeeInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenTribunal.Identifier = 0;
          export.HiddenTribunalFeeInformation.SystemGeneratedIdentifier = 0;
          export.TribunalFeeInformation.Cap = 0;
          export.TribunalFeeInformation.Rate = 0;
          export.TribunalFeeInformation.Description =
            Spaces(TribunalFeeInformation.Description_MaxLength);
          export.TribunalFeeInformation.EffectiveDate = null;
          export.TribunalFeeInformation.DiscontinueDate = null;
          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else if (IsExitState("TRIBUNAL_FEE_INFORMATION_NF"))
        {
        }
        else if (IsExitState("TRIBUNAL_NF"))
        {
          var field = GetField(export.Tribunal, "identifier");

          field.Error = true;

          export.Tribunal.Name = "";
        }
        else
        {
        }

        break;
      case "LIST":
        if (AsChar(import.PromptTextWorkArea.Text1) == 'S')
        {
          export.PromptTextWorkArea.Text1 = "+";
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
        }
        else
        {
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
      case "RECF":
        ExitState = "ECO_LNK_TO_COST_RECOVERY_FEE_INF";

        break;
      default:
        if (export.Tribunal.Identifier != 0)
        {
          if (Lt(export.TribunalFeeInformation.DiscontinueDate, Now().Date) && !
            Equal(export.TribunalFeeInformation.DiscontinueDate, null))
          {
            var field9 = GetField(export.Fips, "countyDescription");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.Tribunal, "judicialDistrict");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.Tribunal, "judicialDivision");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 = GetField(export.Tribunal, "identifier");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 =
              GetField(export.TribunalFeeInformation, "description");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.TribunalFeeInformation, "discontinueDate");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.TribunalFeeInformation, "cap");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 = GetField(export.TribunalFeeInformation, "rate");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 =
              GetField(export.TribunalFeeInformation, "effectiveDate");

            field17.Color = "cyan";
            field17.Protected = true;
          }

          if (Lt(export.TribunalFeeInformation.EffectiveDate, Now().Date))
          {
            var field9 =
              GetField(export.TribunalFeeInformation, "effectiveDate");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.TribunalFeeInformation, "cap");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.TribunalFeeInformation, "rate");

            field11.Color = "cyan";
            field11.Protected = true;
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

  private static void MoveTribunalFeeInformation1(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Rate = source.Rate;
    target.Cap = source.Cap;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.Description = source.Description;
  }

  private static void MoveTribunalFeeInformation2(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCreateCostRecoveryFeeInfo()
  {
    var useImport = new CreateCostRecoveryFeeInfo.Import();
    var useExport = new CreateCostRecoveryFeeInfo.Export();

    useImport.Fips.CountyDescription = import.Fips.CountyDescription;
    useImport.Tribunal.Assign(import.Tribunal);
    useImport.TribunalFeeInformation.Assign(import.TribunalFeeInformation);

    Call(CreateCostRecoveryFeeInfo.Execute, useImport, useExport);

    export.Fips.CountyDescription = useExport.Fips.CountyDescription;
    export.Tribunal.Assign(useExport.Tribunal);
    export.TribunalFeeInformation.Assign(useExport.TribunalFeeInformation);
  }

  private void UseDeleteCostRecoveryFeeInfo()
  {
    var useImport = new DeleteCostRecoveryFeeInfo.Import();
    var useExport = new DeleteCostRecoveryFeeInfo.Export();

    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    useImport.TribunalFeeInformation.SystemGeneratedIdentifier =
      export.TribunalFeeInformation.SystemGeneratedIdentifier;

    Call(DeleteCostRecoveryFeeInfo.Execute, useImport, useExport);
  }

  private void UseFnReadCostRecoveryFeeInfo()
  {
    var useImport = new FnReadCostRecoveryFeeInfo.Import();
    var useExport = new FnReadCostRecoveryFeeInfo.Export();

    useImport.Recf.Flag = import.Recf.Flag;
    useImport.Tribunal.Identifier = local.Tribunal.Identifier;
    MoveTribunalFeeInformation2(export.TribunalFeeInformation,
      useImport.TribunalFeeInformation);

    Call(FnReadCostRecoveryFeeInfo.Execute, useImport, useExport);

    export.Tribunal.Assign(useExport.Tribunal);
    export.Fips.CountyDescription = useExport.Fips.CountyDescription;
    export.TribunalFeeInformation.Assign(useExport.TribunalFeeInformation);
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

  private void UseUpdateCostRecoveryFeeInfo()
  {
    var useImport = new UpdateCostRecoveryFeeInfo.Import();
    var useExport = new UpdateCostRecoveryFeeInfo.Export();

    MoveTribunalFeeInformation1(import.TribunalFeeInformation,
      useImport.TribunalFeeInformation);
    useImport.Tribunal.Assign(import.Tribunal);

    Call(UpdateCostRecoveryFeeInfo.Execute, useImport, useExport);
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
    /// A value of Recf.
    /// </summary>
    [JsonPropertyName("recf")]
    public Common Recf
    {
      get => recf ??= new();
      set => recf = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
    }

    /// <summary>
    /// A value of HiddenTribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("hiddenTribunalFeeInformation")]
    public TribunalFeeInformation HiddenTribunalFeeInformation
    {
      get => hiddenTribunalFeeInformation ??= new();
      set => hiddenTribunalFeeInformation = value;
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
    /// A value of PromptTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptTextWorkArea")]
    public TextWorkArea PromptTextWorkArea
    {
      get => promptTextWorkArea ??= new();
      set => promptTextWorkArea = value;
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

    private Common recf;
    private Tribunal tribunal;
    private Fips fips;
    private TribunalFeeInformation tribunalFeeInformation;
    private Tribunal hiddenTribunal;
    private TribunalFeeInformation hiddenTribunalFeeInformation;
    private Common promptCommon;
    private TextWorkArea promptTextWorkArea;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
    }

    /// <summary>
    /// A value of HiddenTribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("hiddenTribunalFeeInformation")]
    public TribunalFeeInformation HiddenTribunalFeeInformation
    {
      get => hiddenTribunalFeeInformation ??= new();
      set => hiddenTribunalFeeInformation = value;
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
    /// A value of PromptTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptTextWorkArea")]
    public TextWorkArea PromptTextWorkArea
    {
      get => promptTextWorkArea ??= new();
      set => promptTextWorkArea = value;
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

    private Tribunal tribunal;
    private Fips fips;
    private TribunalFeeInformation tribunalFeeInformation;
    private Tribunal hiddenTribunal;
    private TribunalFeeInformation hiddenTribunalFeeInformation;
    private Common promptCommon;
    private TextWorkArea promptTextWorkArea;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public DateWorkArea Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public TribunalFeeInformation Initialized
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

    private Tribunal tribunal;
    private DateWorkArea max;
    private DateWorkArea init1;
    private TribunalFeeInformation initialized;
    private Common temp;
  }
#endregion
}

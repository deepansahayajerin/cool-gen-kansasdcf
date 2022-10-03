// Program: SP_DRLM_ALERT_DISTR_LIST_MAINT, ID: 371747431, model: 746.
// Short name: SWEDRLMP
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
/// A program: SP_DRLM_ALERT_DISTR_LIST_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDrlmAlertDistrListMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DRLM_ALERT_DISTR_LIST_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDrlmAlertDistrListMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDrlmAlertDistrListMaint.
  /// </summary>
  public SpDrlmAlertDistrListMaint(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date	 Developer 	Request #    Description
    // 11/07/96 Alan Samuels                Initial Development
    // 2/24/97  Siraj Konkader              Added logic to validate
    // Business Object Code against Event Business Object Code,
    // Added logic to validate Bus Obj Code against Role Codes
    // Added logic to validate Bus Obj Code against Case Unit Func
    // 03/02/11  GVandy	CQ355
    // For INR events allow alert distributions to CAS, CAU, and INR
    // business objects (previously only supported CAS and CAU distributions).
    // 01/22/13  GVandy	CQ33617
    // Support alert distribution to a single user selected service provider.
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    // Clear scrolling group if command=clear.
    // ---------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    export.Event1.Assign(import.Event1);
    MoveEventDetail(import.EventDetail1, export.EventDetail1);
    export.HiddenEvent.ControlNumber = import.HiddenEvent.ControlNumber;
    export.HiddenEventDetail.SystemGeneratedIdentifier =
      import.HiddenEventDetail.SystemGeneratedIdentifier;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.EventDetail2.SelectChar = import.EventDetail2.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    local.Current.Date = Now().Date;

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.HiddenNextTranInfo);

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          UseScCabNextTranPut();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;

          break;
        }

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ---------------------------------------------
        // Populate export views from local next_tran_info view read from the 
        // data base
        // Set command to initial command required or ESCAPE
        // ---------------------------------------------
        export.HiddenNextTranInfo.Assign(local.NextTranInfo);

        return;
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "ADD":
        // Add logic is located at bottom of PrAD.
        break;
      case "UPDATE":
        // Update logic is located at bottom of PrAD.
        break;
      case "DELETE":
        // Delete logic is located at bottom of PrAD.
        break;
      case "LIST":
        // List logic is located at bottom of PrAD.
        break;
      case "RETALLS":
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Alert2.ControlNumber =
            import.Group.Item.Alert2.ControlNumber;
          export.Group.Update.HiddenAlert.ControlNumber =
            import.Group.Item.HiddenAlert.ControlNumber;
          export.Group.Update.AlertDistributionRule.Assign(
            import.Group.Item.AlertDistributionRule);
          export.Group.Update.HiddenAlertDistributionRule.Assign(
            import.Group.Item.HiddenAlertDistributionRule);

          if (Lt(export.Group.Item.HiddenAlertDistributionRule.EffectiveDate,
            local.Current.Date) && Lt
            (local.Zero.Date,
            export.Group.Item.HiddenAlertDistributionRule.EffectiveDate))
          {
            var field1 =
              GetField(export.Group.Item.AlertDistributionRule, "laPersonCode");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.AlertDistributionRule,
              "csePersonAcctCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.AlertDistributionRule, "caseRoleCode");
              

            field3.Color = "cyan";
            field3.Protected = true;
          }

          export.Group.Update.Office.SystemGeneratedId =
            import.Group.Item.Office.SystemGeneratedId;
          MoveOfficeServiceProvider(import.Group.Item.OfficeServiceProvider,
            export.Group.Update.OfficeServiceProvider);
          MoveServiceProvider(import.Group.Item.ServiceProvider1,
            export.Group.Update.ServiceProvider1);
          MoveServiceProvider(import.Group.Item.HiddenServiceProvider,
            export.Group.Update.HiddenServiceProvider);
          export.Group.Update.HiddenOffice.SystemGeneratedId =
            import.Group.Item.HiddenOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.Group.Item.
            HiddenOfficeServiceProvider,
            export.Group.Update.HiddenOfficeServiceProvider);
          MoveCommon(import.Group.Item.ServiceProvider2,
            export.Group.Update.ServiceProvider2);
          export.Group.Update.HiddenExportGrpServicePrvdr.PersonName =
            import.Group.Item.HiddenImportGrpServicePrvdr.PersonName;
          export.Group.Update.BusinessObject.SelectChar =
            import.Group.Item.BusinessObject.SelectChar;
          export.Group.Update.CuFunction.SelectChar =
            import.Group.Item.CuFunction.SelectChar;
          export.Group.Update.ReasonCode.SelectChar =
            import.Group.Item.ReasonCode.SelectChar;
          export.Group.Update.Alert1.SelectChar =
            import.Group.Item.Alert1.SelectChar;

          if (AsChar(export.Group.Item.Alert1.SelectChar) == 'S')
          {
            if (import.FromLinkAlert.ControlNumber != 0)
            {
              export.Group.Update.Alert2.ControlNumber =
                import.FromLinkAlert.ControlNumber;
            }

            export.Group.Update.Alert1.SelectChar = "";

            var field = GetField(export.Group.Item.Alert1, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }

          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Next();
        }

        return;
      case "RETCDVL":
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Alert2.ControlNumber =
            import.Group.Item.Alert2.ControlNumber;
          export.Group.Update.HiddenAlert.ControlNumber =
            import.Group.Item.HiddenAlert.ControlNumber;
          export.Group.Update.AlertDistributionRule.Assign(
            import.Group.Item.AlertDistributionRule);
          export.Group.Update.HiddenAlertDistributionRule.Assign(
            import.Group.Item.HiddenAlertDistributionRule);

          if (Lt(export.Group.Item.HiddenAlertDistributionRule.EffectiveDate,
            local.Current.Date) && Lt
            (local.Zero.Date,
            export.Group.Item.HiddenAlertDistributionRule.EffectiveDate))
          {
            var field1 =
              GetField(export.Group.Item.AlertDistributionRule, "laPersonCode");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.AlertDistributionRule,
              "csePersonAcctCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.AlertDistributionRule, "caseRoleCode");
              

            field3.Color = "cyan";
            field3.Protected = true;
          }

          export.Group.Update.Office.SystemGeneratedId =
            import.Group.Item.Office.SystemGeneratedId;
          MoveOfficeServiceProvider(import.Group.Item.OfficeServiceProvider,
            export.Group.Update.OfficeServiceProvider);
          MoveServiceProvider(import.Group.Item.ServiceProvider1,
            export.Group.Update.ServiceProvider1);
          MoveServiceProvider(import.Group.Item.HiddenServiceProvider,
            export.Group.Update.HiddenServiceProvider);
          export.Group.Update.HiddenOffice.SystemGeneratedId =
            import.Group.Item.HiddenOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.Group.Item.
            HiddenOfficeServiceProvider,
            export.Group.Update.HiddenOfficeServiceProvider);
          MoveCommon(import.Group.Item.ServiceProvider2,
            export.Group.Update.ServiceProvider2);
          export.Group.Update.HiddenExportGrpServicePrvdr.PersonName =
            import.Group.Item.HiddenImportGrpServicePrvdr.PersonName;
          export.Group.Update.BusinessObject.SelectChar =
            import.Group.Item.BusinessObject.SelectChar;
          export.Group.Update.CuFunction.SelectChar =
            import.Group.Item.CuFunction.SelectChar;
          export.Group.Update.ReasonCode.SelectChar =
            import.Group.Item.ReasonCode.SelectChar;
          export.Group.Update.Alert1.SelectChar =
            import.Group.Item.Alert1.SelectChar;
          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;

          if (AsChar(export.Group.Item.BusinessObject.SelectChar) == 'S')
          {
            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.BusinessObject.SelectChar = "";
              export.Group.Update.AlertDistributionRule.BusinessObjectCode =
                import.FromLinkCodeValue.Cdvalue;
            }

            var field =
              GetField(export.Group.Item.BusinessObject, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
          else if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
          {
            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.CuFunction.SelectChar = "";
              export.Group.Update.AlertDistributionRule.CaseUnitFunction =
                import.FromLinkCodeValue.Cdvalue;
            }

            var field = GetField(export.Group.Item.CuFunction, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
          else if (AsChar(export.Group.Item.ReasonCode.SelectChar) == 'S')
          {
            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.ReasonCode.SelectChar = "";
              export.Group.Update.AlertDistributionRule.ReasonCode =
                import.FromLinkCodeValue.Cdvalue;
            }

            var field = GetField(export.Group.Item.ReasonCode, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }

          export.Group.Update.BusinessObject.SelectChar = "";
          export.Group.Update.CuFunction.SelectChar = "";
          export.Group.Update.ReasonCode.SelectChar = "";
          export.Group.Next();
        }

        return;
      case "RETSVPO":
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Alert2.ControlNumber =
            import.Group.Item.Alert2.ControlNumber;
          export.Group.Update.HiddenAlert.ControlNumber =
            import.Group.Item.HiddenAlert.ControlNumber;
          export.Group.Update.AlertDistributionRule.Assign(
            import.Group.Item.AlertDistributionRule);
          export.Group.Update.HiddenAlertDistributionRule.Assign(
            import.Group.Item.HiddenAlertDistributionRule);

          if (Lt(export.Group.Item.HiddenAlertDistributionRule.EffectiveDate,
            local.Current.Date) && Lt
            (local.Zero.Date,
            export.Group.Item.HiddenAlertDistributionRule.EffectiveDate))
          {
            var field1 =
              GetField(export.Group.Item.AlertDistributionRule, "laPersonCode");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.AlertDistributionRule,
              "csePersonAcctCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.AlertDistributionRule, "caseRoleCode");
              

            field3.Color = "cyan";
            field3.Protected = true;
          }

          export.Group.Update.Office.SystemGeneratedId =
            import.Group.Item.Office.SystemGeneratedId;
          MoveOfficeServiceProvider(import.Group.Item.OfficeServiceProvider,
            export.Group.Update.OfficeServiceProvider);
          MoveServiceProvider(import.Group.Item.ServiceProvider1,
            export.Group.Update.ServiceProvider1);
          MoveServiceProvider(import.Group.Item.HiddenServiceProvider,
            export.Group.Update.HiddenServiceProvider);
          export.Group.Update.HiddenOffice.SystemGeneratedId =
            import.Group.Item.HiddenOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.Group.Item.
            HiddenOfficeServiceProvider,
            export.Group.Update.HiddenOfficeServiceProvider);
          MoveCommon(import.Group.Item.ServiceProvider2,
            export.Group.Update.ServiceProvider2);
          export.Group.Update.HiddenExportGrpServicePrvdr.PersonName =
            import.Group.Item.HiddenImportGrpServicePrvdr.PersonName;
          export.Group.Update.BusinessObject.SelectChar =
            import.Group.Item.BusinessObject.SelectChar;
          export.Group.Update.CuFunction.SelectChar =
            import.Group.Item.CuFunction.SelectChar;
          export.Group.Update.ReasonCode.SelectChar =
            import.Group.Item.ReasonCode.SelectChar;
          export.Group.Update.Alert1.SelectChar =
            import.Group.Item.Alert1.SelectChar;
          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;

          if (AsChar(export.Group.Item.ServiceProvider2.SelectChar) == 'S')
          {
            if (!IsEmpty(import.FromSvpoServiceProvider.UserId))
            {
              export.Group.Update.Office.SystemGeneratedId =
                import.FromSvpoOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(import.FromSvpoOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              MoveServiceProvider(import.FromSvpoServiceProvider,
                export.Group.Update.ServiceProvider1);
              export.Group.Update.ServiceProvider2.PersonName =
                TrimEnd(import.FromSvpoServiceProvider.LastName) + ", " + import
                .FromSvpoServiceProvider.FirstName;
            }

            var field =
              GetField(export.Group.Item.ServiceProvider2, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;

            export.Group.Update.ServiceProvider2.SelectChar = "";
          }

          export.Group.Next();
        }

        return;
      case "RETEDLM":
        if (AsChar(import.EventDetail2.SelectChar) == 'S')
        {
          export.EventDetail2.SelectChar = "";

          if (import.FromLinkEvent.ControlNumber > 0)
          {
            export.Event1.ControlNumber = import.FromLinkEvent.ControlNumber;
            export.EventDetail1.SystemGeneratedIdentifier =
              import.FromLinkEventDetail.SystemGeneratedIdentifier;
          }

          var field = GetField(export.Event1, "controlNumber");

          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          global.Command = "DISPLAY";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "XXFMMENU":
        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // ---------------------------------------------
    // Move group views if command <> display.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      local.Prompt.Count = 0;
      local.Select.Count = 0;

      if (AsChar(export.EventDetail2.SelectChar) == 'S')
      {
        var field = GetField(export.EventDetail2, "selectChar");

        field.Error = true;

        ++local.Prompt.Count;
      }

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.Alert2.ControlNumber =
          import.Group.Item.Alert2.ControlNumber;
        export.Group.Update.HiddenAlert.ControlNumber =
          import.Group.Item.HiddenAlert.ControlNumber;
        export.Group.Update.AlertDistributionRule.Assign(
          import.Group.Item.AlertDistributionRule);

        if (Lt(export.Group.Item.HiddenAlertDistributionRule.EffectiveDate,
          local.Current.Date) && Lt
          (local.Zero.Date,
          export.Group.Item.HiddenAlertDistributionRule.EffectiveDate))
        {
          var field1 =
            GetField(export.Group.Item.AlertDistributionRule, "laPersonCode");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Group.Item.AlertDistributionRule,
            "csePersonAcctCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Group.Item.AlertDistributionRule, "caseRoleCode");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        export.Group.Update.HiddenAlertDistributionRule.Assign(
          import.Group.Item.HiddenAlertDistributionRule);
        export.Group.Update.Office.SystemGeneratedId =
          import.Group.Item.Office.SystemGeneratedId;
        MoveOfficeServiceProvider(import.Group.Item.OfficeServiceProvider,
          export.Group.Update.OfficeServiceProvider);
        MoveServiceProvider(import.Group.Item.ServiceProvider1,
          export.Group.Update.ServiceProvider1);
        MoveServiceProvider(import.Group.Item.HiddenServiceProvider,
          export.Group.Update.HiddenServiceProvider);
        export.Group.Update.HiddenOffice.SystemGeneratedId =
          import.Group.Item.HiddenOffice.SystemGeneratedId;
        MoveOfficeServiceProvider(import.Group.Item.HiddenOfficeServiceProvider,
          export.Group.Update.HiddenOfficeServiceProvider);
        export.Group.Update.HiddenExportGrpServicePrvdr.PersonName =
          import.Group.Item.HiddenImportGrpServicePrvdr.PersonName;
        export.Group.Update.BusinessObject.SelectChar =
          import.Group.Item.BusinessObject.SelectChar;

        if (AsChar(import.Group.Item.BusinessObject.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.BusinessObject, "selectChar");

          field.Error = true;

          ++local.Prompt.Count;
        }

        export.Group.Update.CuFunction.SelectChar =
          import.Group.Item.CuFunction.SelectChar;

        if (AsChar(import.Group.Item.CuFunction.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.CuFunction, "selectChar");

          field.Error = true;

          ++local.Prompt.Count;
        }

        export.Group.Update.ReasonCode.SelectChar =
          import.Group.Item.ReasonCode.SelectChar;

        if (AsChar(export.Group.Item.ReasonCode.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.ReasonCode, "selectChar");

          field.Error = true;

          ++local.Prompt.Count;
        }

        export.Group.Update.Alert1.SelectChar =
          import.Group.Item.Alert1.SelectChar;

        if (AsChar(export.Group.Item.Alert1.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.Alert1, "selectChar");

          field.Error = true;

          ++local.Prompt.Count;
        }

        MoveCommon(import.Group.Item.ServiceProvider2,
          export.Group.Update.ServiceProvider2);

        if (AsChar(export.Group.Item.ServiceProvider2.SelectChar) == 'S')
        {
          var field =
            GetField(export.Group.Item.ServiceProvider2, "selectChar");

          field.Error = true;

          ++local.Prompt.Count;
        }

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ++local.Select.Count;
        }

        export.Group.Next();
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
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

    // ---------------------------------------------
    // Must display before maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (import.Event1.ControlNumber == import.HiddenEvent.ControlNumber && import
        .EventDetail1.SystemGeneratedIdentifier == import
        .HiddenEventDetail.SystemGeneratedIdentifier)
      {
      }
      else
      {
        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        return;
      }

      switch(local.Select.Count)
      {
        case 0:
          ExitState = "SP0000_REQUEST_REQUIRES_SEL";

          return;
        case 1:
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "ADD":
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "SP0000_REQUEST_REQUIRES_SEL";

            return;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
        }

        local.Errors.Count = 0;

        if (export.Event1.ControlNumber == 0)
        {
          ++local.Errors.Count;

          var field = GetField(export.Event1, "controlNumber");

          field.Error = true;

          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }

        if (export.EventDetail1.SystemGeneratedIdentifier == 0)
        {
          ++local.Errors.Count;

          var field =
            GetField(export.EventDetail1, "systemGeneratedIdentifier");

          field.Error = true;

          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }

        local.Zero.Date = null;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;

            // ---------------------------------------------
            // An add must be on a previously blank row.
            // ---------------------------------------------
            if (export.Group.Item.AlertDistributionRule.
              SystemGeneratedIdentifier > 0)
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

              return;
            }

            local.PassToAlertDistributionRule.Assign(
              export.Group.Item.AlertDistributionRule);

            // ---------------------------------------------
            // Perform data validation for add request.
            // ---------------------------------------------
            // ---------------------------------------------
            // Prompt is not valid on add.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.ServiceProvider2.SelectChar))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ServiceProvider2, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            MoveServiceProvider(export.Group.Item.ServiceProvider1,
              local.ServiceProvider2);

            if (IsEmpty(export.Group.Item.ServiceProvider1.UserId))
            {
              export.Group.Update.Office.SystemGeneratedId =
                local.NullOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(local.NullOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              MoveServiceProvider(local.NullServiceProvider,
                export.Group.Update.ServiceProvider1);
              export.Group.Update.ServiceProvider2.PersonName = "";
              local.Office.SystemGeneratedId =
                local.NullOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(local.NullOfficeServiceProvider,
                local.OfficeServiceProvider);
              local.ServiceProvider2.Assign(local.NullServiceProvider);
            }
            else
            {
              // @@@
              if (ReadServiceProvider())
              {
                local.ServiceProvider2.Assign(entities.ServiceProvider);

                if (export.Group.Item.Office.SystemGeneratedId == 0)
                {
                  // -- The user entered the service provider ID instead of 
                  // prompting to SVPO.
                  // -- If there is only one office assignment for this service 
                  // provider then use the
                  //    assignment in that office.
                  local.ServiceProvider1.Count = 0;

                  foreach(var item in ReadOfficeServiceProviderOffice())
                  {
                    local.Office.SystemGeneratedId =
                      entities.Office.SystemGeneratedId;
                    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                      local.OfficeServiceProvider);
                    ++local.ServiceProvider1.Count;
                  }

                  switch(local.ServiceProvider1.Count)
                  {
                    case 0:
                      ++local.Errors.Count;

                      var field1 =
                        GetField(export.Group.Item.ServiceProvider1, "userId");

                      field1.Error = true;

                      ExitState = "SP0000_OFFICE_ASSIGNMENT_NF";

                      break;
                    case 1:
                      break;
                    default:
                      ++local.Errors.Count;

                      var field2 =
                        GetField(export.Group.Item.ServiceProvider1, "userId");

                      field2.Error = true;

                      ExitState = "SP0000_MULTIPLE_OFFICE_ASSGNMENT";

                      break;
                  }
                }
                else if (ReadOfficeOfficeServiceProvider())
                {
                  local.Office.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    local.OfficeServiceProvider);
                }
                else
                {
                  ++local.Errors.Count;

                  var field1 =
                    GetField(export.Group.Item.ServiceProvider1, "userId");

                  field1.Error = true;

                  ExitState = "OFFICE_SERVICE_PROVIDER_NF";
                }
              }
              else
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ServiceProvider1, "userId");

                field1.Error = true;

                ExitState = "SERVICE_PROVIDER_NF";
              }
            }

            // ---------------------------------------------
            // Prompt is not valid on add.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.ReasonCode.SelectChar))
            {
              ++local.Errors.Count;

              var field1 = GetField(export.Group.Item.ReasonCode, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // Reason Code is required and must be valid.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.AlertDistributionRule.ReasonCode))
            {
              if (!IsEmpty(export.Group.Item.ServiceProvider1.UserId))
              {
                // --  Reason Code is required only if service provider is not 
                // selected.
                goto Test1;
              }

              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule, "reasonCode");
                

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              local.PassToCode.CodeName = "ASSIGNMENT REASON CODE";
              local.PassToCodeValue.Cdvalue =
                export.Group.Item.AlertDistributionRule.ReasonCode ?? Spaces
                (10);
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnValidation.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule, "reasonCode");
                  

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
            }

Test1:

            // ---------------------------------------------
            // Prompt is not valid on add.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.BusinessObject.SelectChar))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.BusinessObject, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // Validate business object code. Field is
            // required.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.AlertDistributionRule.
              BusinessObjectCode))
            {
              local.PassToCode.CodeName = "BUSINESS OBJECT CODE";
              local.PassToCodeValue.Cdvalue =
                export.Group.Item.AlertDistributionRule.BusinessObjectCode ?? Spaces
                (10);
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnValidation.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "businessObjectCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
              else
              {
                // ------------------------------------------------------------
                // Now validate Event Business Object Code and Alert 
                // Distribution Business Object Code
                // ------------------------------------------------------------
                switch(TrimEnd(export.Event1.BusinessObjectCode))
                {
                  case "ADA":
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "ADA":
                        break;
                      case "OAA":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_SELECT_CAS_CAU_ADA_OAA";

                        break;
                    }

                    break;
                  case "CAS":
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_ENTER_CAS_CAU_LRF";

                        break;
                    }

                    break;
                  case "CAU":
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_ENTER_CAS_CAU_LRF";

                        break;
                    }

                    break;
                  case "LEA":
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LEA":
                        break;
                      case "OBL":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_ENTER_CAS_CAU_LEA_OBL";

                        break;
                    }

                    break;
                  case "LRF":
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "LRF":
                        break;
                      case "CAU":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_ENTER_CAS_CAU_LRF";

                        break;
                    }

                    break;
                  case "OAA":
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "LEA":
                        break;
                      case "CAU":
                        break;
                      case "OBL":
                        break;
                      case "ADA":
                        break;
                      case "OAA":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_USE_CAS_CAU_LEA_OBL_AD_OA";

                        break;
                    }

                    break;
                  case "OBL":
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "LEA":
                        break;
                      case "CAU":
                        break;
                      case "OBL":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_ENTER_CAS_CAU_LEA_OBL";

                        break;
                    }

                    break;
                  case "INR":
                    // 03/02/11  GVandy  CQ355  For INR events allow alert 
                    // distributions to CAS, CAU, and
                    // INR business objects (previously only supported CAS and 
                    // CAU distributions).
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "INR":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_ENTER_CAS_CAU_INR";

                        break;
                    }

                    break;
                  default:
                    // ------------------------------------------------------------
                    // For all other Event Business Objects, the only 
                    // distribution
                    // must be on Case or Case Unit.
                    // ------------------------------------------------------------
                    switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                      BusinessObjectCode ?? ""))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      default:
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.AlertDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        ExitState = "SP0000_ENTER_CAS_CAU";

                        break;
                    }

                    break;
                }
              }
            }
            else
            {
              // @@@
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "businessObjectCode");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }

            // ---------------------------------------------
            // LE Person Role, LE Case Role, CSENet Role,
            // CSE Person Acc, and Case Role are
            // mutually exclusive, if entered.
            // ---------------------------------------------
            local.Common.Count = 0;

            switch(TrimEnd(export.Group.Item.AlertDistributionRule.
              LaPersonCode ?? ""))
            {
              case "":
                break;
              case "P":
                ++local.Common.Count;

                break;
              case "R":
                ++local.Common.Count;

                break;
              case "C":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "laPersonCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.AlertDistributionRule.
              LaCaseRoleCode ?? ""))
            {
              case "":
                break;
              case "AR":
                ++local.Common.Count;

                break;
              case "AP":
                ++local.Common.Count;

                break;
              case "CH":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "laCaseRoleCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.AlertDistributionRule.
              CsenetRoleCode ?? ""))
            {
              case "":
                break;
              case "AR":
                ++local.Common.Count;

                break;
              case "AP":
                ++local.Common.Count;

                break;
              case "CH":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "csenetRoleCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.AlertDistributionRule.
              CsePersonAcctCode ?? ""))
            {
              case "":
                break;
              case "E":
                ++local.Common.Count;

                break;
              case "R":
                ++local.Common.Count;

                break;
              case "S":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "csePersonAcctCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.AlertDistributionRule.
              CaseRoleCode ?? ""))
            {
              case "":
                break;
              case "AR":
                ++local.Common.Count;

                break;
              case "AP":
                ++local.Common.Count;

                break;
              case "CH":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "caseRoleCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            if (local.Common.Count > 1)
            {
              var field1 =
                GetField(export.Group.Item.AlertDistributionRule, "laPersonCode");
                

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.AlertDistributionRule,
                "csePersonAcctCode");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.AlertDistributionRule, "caseRoleCode");
                

              field3.Error = true;

              ExitState = "SP0000_ONLY_ONE_CODE_ALLOWED";
              ++local.Errors.Count;
            }
            else if (local.Common.Count == 1)
            {
              // ---------------------------------------------------
              // Check value of Role against business object entered
              // ---------------------------------------------------
              switch(TrimEnd(export.Group.Item.AlertDistributionRule.
                BusinessObjectCode ?? ""))
              {
                case "CAS":
                  if (IsEmpty(export.Group.Item.AlertDistributionRule.
                    CaseRoleCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "caseRoleCode");

                    field3.Error = true;
                  }

                  break;
                case "CAU":
                  if (IsEmpty(export.Group.Item.AlertDistributionRule.
                    CaseRoleCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "caseRoleCode");

                    field3.Error = true;
                  }

                  break;
                case "LRF":
                  if (IsEmpty(export.Group.Item.AlertDistributionRule.
                    CaseRoleCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "caseRoleCode");

                    field3.Error = true;
                  }

                  break;
                case "LEA":
                  if (IsEmpty(export.Group.Item.AlertDistributionRule.
                    LaPersonCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "laPersonCode");

                    field3.Error = true;
                  }

                  break;
                case "ADA":
                  if (IsEmpty(export.Group.Item.AlertDistributionRule.
                    CsePersonAcctCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "csePersonAcctCode");

                    field3.Error = true;
                  }

                  break;
                case "OBL":
                  if (IsEmpty(export.Group.Item.AlertDistributionRule.
                    CsePersonAcctCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "csePersonAcctCode");

                    field3.Error = true;
                  }

                  break;
                case "OAA":
                  if (IsEmpty(export.Group.Item.AlertDistributionRule.
                    CsePersonAcctCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.AlertDistributionRule,
                      "csePersonAcctCode");

                    field3.Error = true;
                  }

                  break;
                default:
                  ++local.Errors.Count;
                  ExitState = "SP0000_ONLY_ASSIGNABLE_BUS_OBJ";

                  var field1 =
                    GetField(export.Group.Item.AlertDistributionRule,
                    "businessObjectCode");

                  field1.Error = true;

                  break;
              }
            }

            // ---------------------------------------------
            // Effective date default is current date.
            // ---------------------------------------------
            if (Equal(export.Group.Item.AlertDistributionRule.EffectiveDate,
              local.Zero.Date))
            {
              local.PassToAlertDistributionRule.EffectiveDate =
                local.Current.Date;
              export.Group.Update.AlertDistributionRule.EffectiveDate =
                local.Current.Date;
            }
            else if (!Lt(export.Group.Item.AlertDistributionRule.EffectiveDate,
              local.Current.Date))
            {
            }
            else
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "effectiveDate");

              field1.Error = true;

              ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";
            }

            // ---------------------------------------------
            // Discontinue date must be > effective date.
            // Default value = 12/31/2099.
            // ---------------------------------------------
            if (Equal(export.Group.Item.AlertDistributionRule.DiscontinueDate,
              local.Zero.Date))
            {
              local.PassToAlertDistributionRule.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }
            else if (!Lt(export.Group.Item.AlertDistributionRule.
              DiscontinueDate,
              export.Group.Item.AlertDistributionRule.EffectiveDate))
            {
            }
            else
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "discontinueDate");

              field1.Error = true;

              ExitState = "SP0000_INVALID_DISC_DATE";
            }

            // ---------------------------------------------
            // Optimization ind must be 'Y' or 'N'.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.AlertDistributionRule.OptimizationInd))
              
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "optimizationInd");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else if (AsChar(export.Group.Item.AlertDistributionRule.
              OptimizationInd) == 'Y' || AsChar
              (export.Group.Item.AlertDistributionRule.OptimizationInd) == 'N')
            {
            }
            else
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "optimizationInd");

              field1.Error = true;

              ExitState = "SP0000_INVALID_VALUE_ENTERED";
            }

            // ---------------------------------------------
            // Prioritization code is required.  If Opt Ind
            // = 'Y', prioritization code must be 1 - 8.
            // ---------------------------------------------
            if (export.Group.Item.AlertDistributionRule.PrioritizationCode == 0)
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "prioritizationCode");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else if (AsChar(export.Group.Item.AlertDistributionRule.
              OptimizationInd) == 'Y')
            {
              if (export.Group.Item.HiddenAlertDistributionRule.
                PrioritizationCode > 8)
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "prioritizationCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
            }

            // ---------------------------------------------
            // Prompt is not valid on add.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.CuFunction.SelectChar))
            {
              ++local.Errors.Count;

              var field1 = GetField(export.Group.Item.CuFunction, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // Validate case unit function.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.AlertDistributionRule.
              CaseUnitFunction))
            {
              local.PassToCode.CodeName = "CASE UNIT FUNCTION";
              local.PassToCodeValue.Cdvalue =
                export.Group.Item.AlertDistributionRule.CaseUnitFunction;
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnValidation.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "caseUnitFunction");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
              else if (!Equal(export.Group.Item.AlertDistributionRule.
                BusinessObjectCode, "CAU"))
              {
                ++local.Errors.Count;
                ExitState = "SP0000_BUS_OBJ_MUST_BE_CAU";

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "businessObjectCode");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "caseUnitFunction");

                field2.Error = true;
              }
            }
            else if (Equal(export.Group.Item.AlertDistributionRule.
              BusinessObjectCode, "CAU"))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "caseUnitFunction");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }

            // ---------------------------------------------
            // Validate alert control number. Field is
            // required.
            // ---------------------------------------------
            if (export.Group.Item.Alert2.ControlNumber == 0)
            {
              ++local.Errors.Count;

              var field1 = GetField(export.Group.Item.Alert2, "controlNumber");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else if (ReadAlert())
            {
              // Continue
            }
            else
            {
              ++local.Errors.Count;

              var field1 = GetField(export.Group.Item.Alert2, "controlNumber");

              field1.Error = true;

              ExitState = "SP0000_INVALID_VALUE_ENTERED";
            }
          }
          else
          {
            continue;
          }

          if (local.Errors.Count > 1)
          {
            ExitState = "SP0000_MULTIPLE_ERRORS";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Create alert
          // distribution rule.
          // ---------------------------------------------
          local.PassToAlert.ControlNumber =
            export.Group.Item.Alert2.ControlNumber;
          local.PassToEvent.ControlNumber = import.Event1.ControlNumber;
          local.PassToEventDetail.SystemGeneratedIdentifier =
            import.EventDetail1.SystemGeneratedIdentifier;
          UseSpCabCreateAlertDistrRule();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_ADD_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
          }
          else
          {
            return;
          }

          export.Group.Update.AlertDistributionRule.Assign(local.ReturnFrom);
          local.PassToDateWorkArea.Date =
            export.Group.Item.AlertDistributionRule.DiscontinueDate;
          export.Group.Update.AlertDistributionRule.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
          export.Group.Update.HiddenAlertDistributionRule.Assign(
            local.ReturnFrom);
          export.Group.Update.HiddenAlert.ControlNumber =
            export.Group.Item.Alert2.ControlNumber;
          export.Group.Update.Office.SystemGeneratedId =
            local.Office.SystemGeneratedId;
          MoveOfficeServiceProvider(local.OfficeServiceProvider,
            export.Group.Update.OfficeServiceProvider);
          MoveServiceProvider(local.ServiceProvider2,
            export.Group.Update.ServiceProvider1);
          export.Group.Update.ServiceProvider2.PersonName =
            TrimEnd(entities.ServiceProvider.LastName) + ", " + entities
            .ServiceProvider.FirstName;
          MoveServiceProvider(export.Group.Item.ServiceProvider1,
            export.Group.Update.HiddenServiceProvider);
          export.Group.Update.HiddenOffice.SystemGeneratedId =
            export.Group.Item.Office.SystemGeneratedId;
          MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
            export.Group.Update.HiddenOfficeServiceProvider);
          export.Group.Update.HiddenExportGrpServicePrvdr.PersonName =
            export.Group.Item.ServiceProvider2.PersonName;

          break;
        }

        break;
      case "UPDATE":
        local.Zero.Date = null;
        local.Errors.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;

            // ---------------------------------------------
            // An update must be performed on a populated
            // row.
            // ---------------------------------------------
            if (export.Group.Item.AlertDistributionRule.
              SystemGeneratedIdentifier == 0)
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

              return;
            }

            // ---------------------------------------------
            // Prompt is not valid on update.
            // ---------------------------------------------
            if (!IsEmpty(import.EventDetail2.SelectChar))
            {
              var field1 = GetField(export.EventDetail2, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";

              return;
            }

            local.PassToAlertDistributionRule.Assign(
              export.Group.Item.AlertDistributionRule);

            // ---------------------------------------------
            // Perform data validation for update request.
            // ---------------------------------------------
            // ---------------------------------------------
            // Prompt is not valid on update.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.ServiceProvider2.SelectChar))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ServiceProvider2, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            if (!Equal(export.Group.Item.ServiceProvider1.UserId,
              export.Group.Item.HiddenServiceProvider.UserId))
            {
              ++local.Errors.Count;
              export.Group.Update.Office.SystemGeneratedId =
                export.Group.Item.HiddenOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(export.Group.Item.
                HiddenOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              MoveServiceProvider(export.Group.Item.HiddenServiceProvider,
                export.Group.Update.ServiceProvider1);
              export.Group.Update.ServiceProvider2.PersonName =
                export.Group.Item.HiddenExportGrpServicePrvdr.PersonName;

              var field1 =
                GetField(export.Group.Item.ServiceProvider1, "userId");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            // ---------------------------------------------
            // Prompt is not valid on update.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.ReasonCode.SelectChar))
            {
              ++local.Errors.Count;

              var field1 = GetField(export.Group.Item.ReasonCode, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // Reason Code is required and must be valid.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.AlertDistributionRule.ReasonCode))
            {
              if (!IsEmpty(export.Group.Item.ServiceProvider1.UserId))
              {
                // --  Reason Code is required only if service provider is not 
                // selected.
                goto Test2;
              }

              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule, "reasonCode");
                

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              // -- Reason code not allowed if service provider info is entered.
              if (IsEmpty(export.Group.Item.HiddenAlertDistributionRule.
                ReasonCode) && !
                IsEmpty(export.Group.Item.ServiceProvider1.UserId))
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule, "reasonCode");
                  

                field1.Error = true;

                ExitState = "SP0000_NOT_ALLOWED_WITH_USER_ID";

                goto Test2;
              }

              local.PassToCode.CodeName = "ASSIGNMENT REASON CODE";
              local.PassToCodeValue.Cdvalue =
                export.Group.Item.AlertDistributionRule.ReasonCode ?? Spaces
                (10);
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnValidation.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule, "reasonCode");
                  

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
            }

Test2:

            // ---------------------------------------------
            // None of the 'roles' can be changed.  These are
            // the fields on the second row of data.
            // ---------------------------------------------
            if (Equal(export.Group.Item.AlertDistributionRule.LaPersonCode,
              export.Group.Item.HiddenAlertDistributionRule.LaPersonCode))
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.AlertDistributionRule.LaPersonCode =
                export.Group.Item.HiddenAlertDistributionRule.LaPersonCode ?? ""
                ;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule, "laPersonCode");
                

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (Equal(export.Group.Item.AlertDistributionRule.LaCaseRoleCode,
              export.Group.Item.HiddenAlertDistributionRule.LaCaseRoleCode))
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.AlertDistributionRule.LaCaseRoleCode =
                export.Group.Item.HiddenAlertDistributionRule.
                  LaCaseRoleCode ?? "";

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "laCaseRoleCode");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (Equal(export.Group.Item.AlertDistributionRule.CsenetRoleCode,
              export.Group.Item.HiddenAlertDistributionRule.CsenetRoleCode))
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.AlertDistributionRule.CsenetRoleCode =
                export.Group.Item.HiddenAlertDistributionRule.
                  CsenetRoleCode ?? "";

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "csenetRoleCode");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (Equal(export.Group.Item.AlertDistributionRule.CsePersonAcctCode,
              export.Group.Item.HiddenAlertDistributionRule.CsePersonAcctCode))
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.AlertDistributionRule.CsePersonAcctCode =
                export.Group.Item.HiddenAlertDistributionRule.
                  CsePersonAcctCode ?? "";

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "csePersonAcctCode");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (Equal(export.Group.Item.AlertDistributionRule.CaseRoleCode,
              export.Group.Item.HiddenAlertDistributionRule.CaseRoleCode))
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.AlertDistributionRule.CaseRoleCode =
                export.Group.Item.HiddenAlertDistributionRule.CaseRoleCode ?? ""
                ;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule, "caseRoleCode");
                

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            // ---------------------------------------------
            // Effective date default is current date.
            // ---------------------------------------------
            if (!Equal(export.Group.Item.AlertDistributionRule.EffectiveDate,
              export.Group.Item.HiddenAlertDistributionRule.EffectiveDate))
            {
              if (!Lt(export.Group.Item.HiddenAlertDistributionRule.
                EffectiveDate, local.Current.Date))
              {
                if (Equal(export.Group.Item.AlertDistributionRule.EffectiveDate,
                  local.Zero.Date))
                {
                  local.PassToAlertDistributionRule.EffectiveDate =
                    local.Current.Date;
                  export.Group.Update.AlertDistributionRule.EffectiveDate =
                    local.Current.Date;
                }
                else if (!Lt(export.Group.Item.AlertDistributionRule.
                  EffectiveDate, local.Current.Date))
                {
                }
                else
                {
                  ++local.Errors.Count;

                  var field1 =
                    GetField(export.Group.Item.AlertDistributionRule,
                    "effectiveDate");

                  field1.Error = true;

                  export.Group.Update.AlertDistributionRule.EffectiveDate =
                    export.Group.Item.HiddenAlertDistributionRule.EffectiveDate;
                    
                  ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";
                }
              }
              else
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.AlertDistributionRule,
                  "effectiveDate");

                field1.Error = true;

                export.Group.Update.AlertDistributionRule.EffectiveDate =
                  export.Group.Item.HiddenAlertDistributionRule.EffectiveDate;
                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }
            }

            // ---------------------------------------------
            // Discontinue date must be >= effective date.
            // Default value = 12/31/2099.
            // ---------------------------------------------
            if (Equal(export.Group.Item.AlertDistributionRule.DiscontinueDate,
              local.Zero.Date))
            {
              local.PassToAlertDistributionRule.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }
            else if (Lt(export.Group.Item.AlertDistributionRule.DiscontinueDate,
              export.Group.Item.AlertDistributionRule.EffectiveDate))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "discontinueDate");

              field1.Error = true;

              ExitState = "SP0000_INVALID_DISC_DATE";
            }

            // ---------------------------------------------
            // Prt Cd and Opt Ind are mandatory.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.AlertDistributionRule.OptimizationInd))
              
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "optimizationInd");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else if (AsChar(export.Group.Item.AlertDistributionRule.
              OptimizationInd) == 'Y' || AsChar
              (export.Group.Item.AlertDistributionRule.OptimizationInd) == 'N')
            {
            }
            else
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "optimizationInd");

              field1.Error = true;

              ExitState = "SP0000_INVALID_VALUE_ENTERED";
            }

            // ---------------------------------------------
            // Prioritization code is required.
            // ---------------------------------------------
            if (export.Group.Item.AlertDistributionRule.PrioritizationCode == 0)
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "prioritizationCode");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }

            // ---------------------------------------------
            // Prompt is not valid on update.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.CuFunction.SelectChar))
            {
              ++local.Errors.Count;

              var field1 = GetField(export.Group.Item.CuFunction, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // CU Function cannot be changed.
            // ---------------------------------------------
            if (Equal(export.Group.Item.AlertDistributionRule.CaseUnitFunction,
              export.Group.Item.HiddenAlertDistributionRule.CaseUnitFunction))
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.AlertDistributionRule.CaseUnitFunction =
                export.Group.Item.HiddenAlertDistributionRule.CaseUnitFunction;

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "caseUnitFunction");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            // ---------------------------------------------
            // Prompt is not valid on add.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.BusinessObject.SelectChar))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.BusinessObject, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // Business Obj Code cannot be changed.
            // ---------------------------------------------
            if (Equal(export.Group.Item.AlertDistributionRule.
              BusinessObjectCode,
              export.Group.Item.HiddenAlertDistributionRule.
                BusinessObjectCode))
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.AlertDistributionRule.BusinessObjectCode =
                export.Group.Item.HiddenAlertDistributionRule.
                  BusinessObjectCode ?? "";

              var field1 =
                GetField(export.Group.Item.AlertDistributionRule,
                "businessObjectCode");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            // ---------------------------------------------
            // Prompt is not valid on add.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.Alert1.SelectChar))
            {
              ++local.Errors.Count;

              var field1 = GetField(export.Group.Item.Alert1, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // Alert cannot be changed.
            // ---------------------------------------------
            if (export.Group.Item.Alert2.ControlNumber == export
              .Group.Item.HiddenAlert.ControlNumber)
            {
            }
            else
            {
              ++local.Errors.Count;
              export.Group.Update.Alert2.ControlNumber =
                export.Group.Item.HiddenAlert.ControlNumber;

              var field1 = GetField(export.Group.Item.Alert2, "controlNumber");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (local.Errors.Count == 0)
            {
              // ---------------------------------------------
              // Prt Cd, Opt Ind, Eff Date, Disc Date or reason
              // must have changed.
              // ---------------------------------------------
              if (Equal(export.Group.Item.AlertDistributionRule.DiscontinueDate,
                export.Group.Item.HiddenAlertDistributionRule.
                  DiscontinueDate) && Equal
                (export.Group.Item.AlertDistributionRule.EffectiveDate,
                export.Group.Item.HiddenAlertDistributionRule.EffectiveDate) &&
                AsChar
                (export.Group.Item.AlertDistributionRule.OptimizationInd) == AsChar
                (export.Group.Item.HiddenAlertDistributionRule.OptimizationInd) &&
                export.Group.Item.AlertDistributionRule.PrioritizationCode == export
                .Group.Item.HiddenAlertDistributionRule.PrioritizationCode && Equal
                (export.Group.Item.AlertDistributionRule.ReasonCode,
                export.Group.Item.HiddenAlertDistributionRule.ReasonCode))
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "SP0000_DATA_NOT_CHANGED";

                return;
              }
            }
          }
          else
          {
            continue;
          }

          if (local.Errors.Count > 1)
          {
            ExitState = "SP0000_MULTIPLE_ERRORS";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Update Activity
          // Start Stop.
          // ---------------------------------------------
          local.PassToEvent.ControlNumber = import.Event1.ControlNumber;
          local.PassToEventDetail.SystemGeneratedIdentifier =
            import.EventDetail1.SystemGeneratedIdentifier;
          UseSpCabUpdateAlertDistrRules();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
          }
          else
          {
            return;
          }

          export.Group.Update.AlertDistributionRule.Assign(local.ReturnFrom);
          local.PassToDateWorkArea.Date =
            export.Group.Item.AlertDistributionRule.DiscontinueDate;
          export.Group.Update.AlertDistributionRule.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
          export.Group.Update.HiddenAlertDistributionRule.Assign(
            local.ReturnFrom);

          break;
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            // ---------------------------------------------
            // A delete must be performed on a populated
            // row.
            // ---------------------------------------------
            if (export.Group.Item.AlertDistributionRule.
              SystemGeneratedIdentifier == 0)
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

              return;
            }

            local.PassToAlertDistributionRule.Assign(
              export.Group.Item.AlertDistributionRule);
            local.PassToEvent.ControlNumber = import.Event1.ControlNumber;
            local.PassToEventDetail.SystemGeneratedIdentifier =
              import.EventDetail1.SystemGeneratedIdentifier;
            UseSpCabDeleteAlertDistrRules();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
              MoveAlertDistributionRule2(local.Initialize,
                export.Group.Update.AlertDistributionRule);
              export.Group.Update.HiddenAlertDistributionRule.Assign(
                local.Initialize);
              export.Group.Update.Alert2.ControlNumber = 0;
              export.Group.Update.HiddenAlert.ControlNumber = 0;
              export.Group.Update.Common.SelectChar = "";
              export.Group.Update.Office.SystemGeneratedId =
                local.NullOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(local.NullOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              MoveServiceProvider(local.NullServiceProvider,
                export.Group.Update.ServiceProvider1);
              MoveServiceProvider(local.NullServiceProvider,
                export.Group.Update.HiddenServiceProvider);
              export.Group.Update.HiddenOffice.SystemGeneratedId =
                local.NullOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(local.NullOfficeServiceProvider,
                export.Group.Update.HiddenOfficeServiceProvider);
              export.Group.Update.ServiceProvider2.PersonName = "";
              export.Group.Update.HiddenExportGrpServicePrvdr.PersonName = "";
            }
            else
            {
              return;
            }

            break;
          }
        }

        break;
      case "LIST":
        switch(local.Prompt.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        if (AsChar(import.EventDetail2.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_EDLM";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Alert1.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_ALLS";

            return;
          }

          if (AsChar(export.Group.Item.BusinessObject.SelectChar) == 'S')
          {
            export.ToTran.CodeName = "BUSINESS OBJECT CODE";
            ExitState = "ECO_LNK_TO_CDVL";

            return;
          }

          if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
          {
            export.ToTran.CodeName = "CASE UNIT FUNCTION";
            ExitState = "ECO_LNK_TO_CDVL";

            return;
          }

          if (AsChar(export.Group.Item.ReasonCode.SelectChar) == 'S')
          {
            export.ToTran.CodeName = "ASSIGNMENT REASON CODE";
            ExitState = "ECO_LNK_TO_CDVL";

            return;
          }

          if (AsChar(export.Group.Item.ServiceProvider2.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_SVPO";

            return;
          }
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadEventEventDetail())
      {
        export.Event1.Assign(entities.Event1);
        export.HiddenEvent.ControlNumber = entities.Event1.ControlNumber;
        MoveEventDetail(entities.EventDetail, export.EventDetail1);
        export.HiddenEventDetail.SystemGeneratedIdentifier =
          entities.EventDetail.SystemGeneratedIdentifier;

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadAlertDistributionRuleAlert())
        {
          export.Group.Update.Alert2.ControlNumber =
            entities.Alert.ControlNumber;
          export.Group.Update.HiddenAlert.ControlNumber =
            entities.Alert.ControlNumber;
          export.Group.Update.AlertDistributionRule.Assign(
            entities.AlertDistributionRule);
          MoveAlertDistributionRule2(entities.AlertDistributionRule,
            export.Group.Update.HiddenAlertDistributionRule);
          local.PassToDateWorkArea.Date =
            entities.AlertDistributionRule.DiscontinueDate;
          export.Group.Update.AlertDistributionRule.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
          export.Group.Update.HiddenAlertDistributionRule.DiscontinueDate =
            export.Group.Item.AlertDistributionRule.DiscontinueDate;

          if (Lt(export.Group.Item.HiddenAlertDistributionRule.EffectiveDate,
            local.Current.Date))
          {
            var field1 =
              GetField(export.Group.Item.AlertDistributionRule, "laPersonCode");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.AlertDistributionRule,
              "csePersonAcctCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.AlertDistributionRule, "caseRoleCode");
              

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (ReadOfficeServiceProviderOfficeServiceProvider())
          {
            export.Group.Update.Office.SystemGeneratedId =
              entities.Office.SystemGeneratedId;
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.Group.Update.OfficeServiceProvider);
            MoveServiceProvider(entities.ServiceProvider,
              export.Group.Update.ServiceProvider1);
            export.Group.Update.ServiceProvider2.PersonName =
              TrimEnd(entities.ServiceProvider.LastName) + ", " + entities
              .ServiceProvider.FirstName;
            MoveServiceProvider(entities.ServiceProvider,
              export.Group.Update.HiddenServiceProvider);
            export.Group.Update.HiddenOffice.SystemGeneratedId =
              entities.Office.SystemGeneratedId;
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.Group.Update.HiddenOfficeServiceProvider);
            export.Group.Update.HiddenExportGrpServicePrvdr.PersonName =
              export.Group.Item.ServiceProvider2.PersonName;
          }

          export.Group.Next();
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
      }
      else if (export.Event1.ControlNumber > 0)
      {
        var field1 = GetField(export.Event1, "controlNumber");

        field1.Error = true;

        var field2 = GetField(export.EventDetail1, "systemGeneratedIdentifier");

        field2.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
      }
      else
      {
        var field = GetField(export.Event1, "controlNumber");

        field.Protected = false;
        field.Focused = true;

        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
      }
    }
  }

  private static void MoveAlertDistributionRule1(AlertDistributionRule source,
    AlertDistributionRule target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveAlertDistributionRule2(AlertDistributionRule source,
    AlertDistributionRule target)
  {
    target.BusinessObjectCode = source.BusinessObjectCode;
    target.CaseUnitFunction = source.CaseUnitFunction;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.ReasonCode = source.ReasonCode;
    target.CaseRoleCode = source.CaseRoleCode;
    target.CsePersonAcctCode = source.CsePersonAcctCode;
    target.CsenetRoleCode = source.CsenetRoleCode;
    target.LaCaseRoleCode = source.LaCaseRoleCode;
    target.LaPersonCode = source.LaPersonCode;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.PersonName = source.PersonName;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveEventDetail(EventDetail source, EventDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DetailName = source.DetailName;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.PassToDateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.PassToCode.CodeName;
    useImport.CodeValue.Cdvalue = local.PassToCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnValidation.Flag = useExport.ValidCode.Flag;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

  private void UseSpCabCreateAlertDistrRule()
  {
    var useImport = new SpCabCreateAlertDistrRule.Import();
    var useExport = new SpCabCreateAlertDistrRule.Export();

    useImport.Event1.ControlNumber = local.PassToEvent.ControlNumber;
    useImport.EventDetail.SystemGeneratedIdentifier =
      local.PassToEventDetail.SystemGeneratedIdentifier;
    useImport.Alert.ControlNumber = local.PassToAlert.ControlNumber;
    useImport.AlertDistributionRule.Assign(local.PassToAlertDistributionRule);
    useImport.ServiceProvider.SystemGeneratedId =
      local.ServiceProvider2.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = local.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(local.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabCreateAlertDistrRule.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.AlertDistributionRule);
  }

  private void UseSpCabDeleteAlertDistrRules()
  {
    var useImport = new SpCabDeleteAlertDistrRules.Import();
    var useExport = new SpCabDeleteAlertDistrRules.Export();

    useImport.Event1.ControlNumber = local.PassToEvent.ControlNumber;
    useImport.EventDetail.SystemGeneratedIdentifier =
      local.PassToEventDetail.SystemGeneratedIdentifier;
    MoveAlertDistributionRule1(local.PassToAlertDistributionRule,
      useImport.AlertDistributionRule);

    Call(SpCabDeleteAlertDistrRules.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateAlertDistrRules()
  {
    var useImport = new SpCabUpdateAlertDistrRules.Import();
    var useExport = new SpCabUpdateAlertDistrRules.Export();

    useImport.Event1.ControlNumber = local.PassToEvent.ControlNumber;
    useImport.EventDetail.SystemGeneratedIdentifier =
      local.PassToEventDetail.SystemGeneratedIdentifier;
    useImport.AlertDistributionRule.Assign(local.PassToAlertDistributionRule);

    Call(SpCabUpdateAlertDistrRules.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.AlertDistributionRule);
  }

  private bool ReadAlert()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "controlNumber", export.Group.Item.Alert2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Populated = true;
      });
  }

  private IEnumerable<bool> ReadAlertDistributionRuleAlert()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);

    return ReadEach("ReadAlertDistributionRuleAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.AlertDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AlertDistributionRule.BusinessObjectCode =
          db.GetNullableString(reader, 1);
        entities.AlertDistributionRule.CaseUnitFunction =
          db.GetString(reader, 2);
        entities.AlertDistributionRule.PrioritizationCode =
          db.GetInt32(reader, 3);
        entities.AlertDistributionRule.OptimizationInd =
          db.GetString(reader, 4);
        entities.AlertDistributionRule.ReasonCode =
          db.GetNullableString(reader, 5);
        entities.AlertDistributionRule.CaseRoleCode =
          db.GetNullableString(reader, 6);
        entities.AlertDistributionRule.CsePersonAcctCode =
          db.GetNullableString(reader, 7);
        entities.AlertDistributionRule.CsenetRoleCode =
          db.GetNullableString(reader, 8);
        entities.AlertDistributionRule.LaCaseRoleCode =
          db.GetNullableString(reader, 9);
        entities.AlertDistributionRule.LaPersonCode =
          db.GetNullableString(reader, 10);
        entities.AlertDistributionRule.EffectiveDate = db.GetDate(reader, 11);
        entities.AlertDistributionRule.DiscontinueDate = db.GetDate(reader, 12);
        entities.AlertDistributionRule.EveNo = db.GetInt32(reader, 13);
        entities.AlertDistributionRule.EvdId = db.GetInt32(reader, 14);
        entities.AlertDistributionRule.AleNo = db.GetNullableInt32(reader, 15);
        entities.Alert.ControlNumber = db.GetInt32(reader, 15);
        entities.AlertDistributionRule.OspGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.AlertDistributionRule.OffGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.AlertDistributionRule.OspRoleCode =
          db.GetNullableString(reader, 18);
        entities.AlertDistributionRule.OspEffectiveDt =
          db.GetNullableDate(reader, 19);
        entities.Alert.Populated = true;
        entities.AlertDistributionRule.Populated = true;

        return true;
      });
  }

  private bool ReadEventEventDetail()
  {
    entities.EventDetail.Populated = false;
    entities.Event1.Populated = false;

    return Read("ReadEventEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Event1.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          export.EventDetail1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.Event1.Name = db.GetString(reader, 1);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 2);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.EventDetail.DetailName = db.GetString(reader, 4);
        entities.EventDetail.Populated = true;
        entities.Event1.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.OfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          export.Group.Item.OfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "officeId", export.Group.Item.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOffice()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOffice",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.AlertDistributionRule.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.AlertDistributionRule.OspGeneratedId.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.AlertDistributionRule.OffGeneratedId.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.AlertDistributionRule.OspRoleCode ?? ""
          );
        db.SetDate(
          command, "effectiveDate",
          entities.AlertDistributionRule.OspEffectiveDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.ServiceProvider.LastName = db.GetString(reader, 7);
        entities.ServiceProvider.FirstName = db.GetString(reader, 8);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "userId", export.Group.Item.ServiceProvider1.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ReasonCode.
      /// </summary>
      [JsonPropertyName("reasonCode")]
      public Common ReasonCode
      {
        get => reasonCode ??= new();
        set => reasonCode = value;
      }

      /// <summary>
      /// A value of HiddenAlert.
      /// </summary>
      [JsonPropertyName("hiddenAlert")]
      public Alert HiddenAlert
      {
        get => hiddenAlert ??= new();
        set => hiddenAlert = value;
      }

      /// <summary>
      /// A value of Alert1.
      /// </summary>
      [JsonPropertyName("alert1")]
      public Common Alert1
      {
        get => alert1 ??= new();
        set => alert1 = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CuFunction.
      /// </summary>
      [JsonPropertyName("cuFunction")]
      public Common CuFunction
      {
        get => cuFunction ??= new();
        set => cuFunction = value;
      }

      /// <summary>
      /// A value of BusinessObject.
      /// </summary>
      [JsonPropertyName("businessObject")]
      public Common BusinessObject
      {
        get => businessObject ??= new();
        set => businessObject = value;
      }

      /// <summary>
      /// A value of HiddenAlertDistributionRule.
      /// </summary>
      [JsonPropertyName("hiddenAlertDistributionRule")]
      public AlertDistributionRule HiddenAlertDistributionRule
      {
        get => hiddenAlertDistributionRule ??= new();
        set => hiddenAlertDistributionRule = value;
      }

      /// <summary>
      /// A value of Alert2.
      /// </summary>
      [JsonPropertyName("alert2")]
      public Alert Alert2
      {
        get => alert2 ??= new();
        set => alert2 = value;
      }

      /// <summary>
      /// A value of AlertDistributionRule.
      /// </summary>
      [JsonPropertyName("alertDistributionRule")]
      public AlertDistributionRule AlertDistributionRule
      {
        get => alertDistributionRule ??= new();
        set => alertDistributionRule = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of ServiceProvider1.
      /// </summary>
      [JsonPropertyName("serviceProvider1")]
      public ServiceProvider ServiceProvider1
      {
        get => serviceProvider1 ??= new();
        set => serviceProvider1 = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of ServiceProvider2.
      /// </summary>
      [JsonPropertyName("serviceProvider2")]
      public Common ServiceProvider2
      {
        get => serviceProvider2 ??= new();
        set => serviceProvider2 = value;
      }

      /// <summary>
      /// A value of HiddenOffice.
      /// </summary>
      [JsonPropertyName("hiddenOffice")]
      public Office HiddenOffice
      {
        get => hiddenOffice ??= new();
        set => hiddenOffice = value;
      }

      /// <summary>
      /// A value of HiddenServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenServiceProvider")]
      public ServiceProvider HiddenServiceProvider
      {
        get => hiddenServiceProvider ??= new();
        set => hiddenServiceProvider = value;
      }

      /// <summary>
      /// A value of HiddenOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvider")]
      public OfficeServiceProvider HiddenOfficeServiceProvider
      {
        get => hiddenOfficeServiceProvider ??= new();
        set => hiddenOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of HiddenImportGrpServicePrvdr.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpServicePrvdr")]
      public Common HiddenImportGrpServicePrvdr
      {
        get => hiddenImportGrpServicePrvdr ??= new();
        set => hiddenImportGrpServicePrvdr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 54;

      private Common reasonCode;
      private Alert hiddenAlert;
      private Common alert1;
      private Common common;
      private Common cuFunction;
      private Common businessObject;
      private AlertDistributionRule hiddenAlertDistributionRule;
      private Alert alert2;
      private AlertDistributionRule alertDistributionRule;
      private Office office;
      private ServiceProvider serviceProvider1;
      private OfficeServiceProvider officeServiceProvider;
      private Common serviceProvider2;
      private Office hiddenOffice;
      private ServiceProvider hiddenServiceProvider;
      private OfficeServiceProvider hiddenOfficeServiceProvider;
      private Common hiddenImportGrpServicePrvdr;
    }

    /// <summary>
    /// A value of FromLinkCodeValue.
    /// </summary>
    [JsonPropertyName("fromLinkCodeValue")]
    public CodeValue FromLinkCodeValue
    {
      get => fromLinkCodeValue ??= new();
      set => fromLinkCodeValue = value;
    }

    /// <summary>
    /// A value of FromLinkAlert.
    /// </summary>
    [JsonPropertyName("fromLinkAlert")]
    public Alert FromLinkAlert
    {
      get => fromLinkAlert ??= new();
      set => fromLinkAlert = value;
    }

    /// <summary>
    /// A value of FromLinkEventDetail.
    /// </summary>
    [JsonPropertyName("fromLinkEventDetail")]
    public EventDetail FromLinkEventDetail
    {
      get => fromLinkEventDetail ??= new();
      set => fromLinkEventDetail = value;
    }

    /// <summary>
    /// A value of FromLinkEvent.
    /// </summary>
    [JsonPropertyName("fromLinkEvent")]
    public Event1 FromLinkEvent
    {
      get => fromLinkEvent ??= new();
      set => fromLinkEvent = value;
    }

    /// <summary>
    /// A value of HiddenEventDetail.
    /// </summary>
    [JsonPropertyName("hiddenEventDetail")]
    public EventDetail HiddenEventDetail
    {
      get => hiddenEventDetail ??= new();
      set => hiddenEventDetail = value;
    }

    /// <summary>
    /// A value of EventDetail1.
    /// </summary>
    [JsonPropertyName("eventDetail1")]
    public EventDetail EventDetail1
    {
      get => eventDetail1 ??= new();
      set => eventDetail1 = value;
    }

    /// <summary>
    /// A value of HiddenEvent.
    /// </summary>
    [JsonPropertyName("hiddenEvent")]
    public Event1 HiddenEvent
    {
      get => hiddenEvent ??= new();
      set => hiddenEvent = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail2.
    /// </summary>
    [JsonPropertyName("eventDetail2")]
    public Common EventDetail2
    {
      get => eventDetail2 ??= new();
      set => eventDetail2 = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of FromSvpoOffice.
    /// </summary>
    [JsonPropertyName("fromSvpoOffice")]
    public Office FromSvpoOffice
    {
      get => fromSvpoOffice ??= new();
      set => fromSvpoOffice = value;
    }

    /// <summary>
    /// A value of FromSvpoServiceProvider.
    /// </summary>
    [JsonPropertyName("fromSvpoServiceProvider")]
    public ServiceProvider FromSvpoServiceProvider
    {
      get => fromSvpoServiceProvider ??= new();
      set => fromSvpoServiceProvider = value;
    }

    /// <summary>
    /// A value of FromSvpoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("fromSvpoOfficeServiceProvider")]
    public OfficeServiceProvider FromSvpoOfficeServiceProvider
    {
      get => fromSvpoOfficeServiceProvider ??= new();
      set => fromSvpoOfficeServiceProvider = value;
    }

    private CodeValue fromLinkCodeValue;
    private Alert fromLinkAlert;
    private EventDetail fromLinkEventDetail;
    private Event1 fromLinkEvent;
    private EventDetail hiddenEventDetail;
    private EventDetail eventDetail1;
    private Event1 hiddenEvent;
    private Event1 event1;
    private Common eventDetail2;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Office fromSvpoOffice;
    private ServiceProvider fromSvpoServiceProvider;
    private OfficeServiceProvider fromSvpoOfficeServiceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ReasonCode.
      /// </summary>
      [JsonPropertyName("reasonCode")]
      public Common ReasonCode
      {
        get => reasonCode ??= new();
        set => reasonCode = value;
      }

      /// <summary>
      /// A value of HiddenAlert.
      /// </summary>
      [JsonPropertyName("hiddenAlert")]
      public Alert HiddenAlert
      {
        get => hiddenAlert ??= new();
        set => hiddenAlert = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Alert1.
      /// </summary>
      [JsonPropertyName("alert1")]
      public Common Alert1
      {
        get => alert1 ??= new();
        set => alert1 = value;
      }

      /// <summary>
      /// A value of CuFunction.
      /// </summary>
      [JsonPropertyName("cuFunction")]
      public Common CuFunction
      {
        get => cuFunction ??= new();
        set => cuFunction = value;
      }

      /// <summary>
      /// A value of BusinessObject.
      /// </summary>
      [JsonPropertyName("businessObject")]
      public Common BusinessObject
      {
        get => businessObject ??= new();
        set => businessObject = value;
      }

      /// <summary>
      /// A value of HiddenAlertDistributionRule.
      /// </summary>
      [JsonPropertyName("hiddenAlertDistributionRule")]
      public AlertDistributionRule HiddenAlertDistributionRule
      {
        get => hiddenAlertDistributionRule ??= new();
        set => hiddenAlertDistributionRule = value;
      }

      /// <summary>
      /// A value of Alert2.
      /// </summary>
      [JsonPropertyName("alert2")]
      public Alert Alert2
      {
        get => alert2 ??= new();
        set => alert2 = value;
      }

      /// <summary>
      /// A value of AlertDistributionRule.
      /// </summary>
      [JsonPropertyName("alertDistributionRule")]
      public AlertDistributionRule AlertDistributionRule
      {
        get => alertDistributionRule ??= new();
        set => alertDistributionRule = value;
      }

      /// <summary>
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>
      /// A value of ServiceProvider1.
      /// </summary>
      [JsonPropertyName("serviceProvider1")]
      public ServiceProvider ServiceProvider1
      {
        get => serviceProvider1 ??= new();
        set => serviceProvider1 = value;
      }

      /// <summary>
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of ServiceProvider2.
      /// </summary>
      [JsonPropertyName("serviceProvider2")]
      public Common ServiceProvider2
      {
        get => serviceProvider2 ??= new();
        set => serviceProvider2 = value;
      }

      /// <summary>
      /// A value of HiddenOffice.
      /// </summary>
      [JsonPropertyName("hiddenOffice")]
      public Office HiddenOffice
      {
        get => hiddenOffice ??= new();
        set => hiddenOffice = value;
      }

      /// <summary>
      /// A value of HiddenServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenServiceProvider")]
      public ServiceProvider HiddenServiceProvider
      {
        get => hiddenServiceProvider ??= new();
        set => hiddenServiceProvider = value;
      }

      /// <summary>
      /// A value of HiddenOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hiddenOfficeServiceProvider")]
      public OfficeServiceProvider HiddenOfficeServiceProvider
      {
        get => hiddenOfficeServiceProvider ??= new();
        set => hiddenOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of HiddenExportGrpServicePrvdr.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpServicePrvdr")]
      public Common HiddenExportGrpServicePrvdr
      {
        get => hiddenExportGrpServicePrvdr ??= new();
        set => hiddenExportGrpServicePrvdr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 54;

      private Common reasonCode;
      private Alert hiddenAlert;
      private Common common;
      private Common alert1;
      private Common cuFunction;
      private Common businessObject;
      private AlertDistributionRule hiddenAlertDistributionRule;
      private Alert alert2;
      private AlertDistributionRule alertDistributionRule;
      private Office office;
      private ServiceProvider serviceProvider1;
      private OfficeServiceProvider officeServiceProvider;
      private Common serviceProvider2;
      private Office hiddenOffice;
      private ServiceProvider hiddenServiceProvider;
      private OfficeServiceProvider hiddenOfficeServiceProvider;
      private Common hiddenExportGrpServicePrvdr;
    }

    /// <summary>
    /// A value of ToTran.
    /// </summary>
    [JsonPropertyName("toTran")]
    public Code ToTran
    {
      get => toTran ??= new();
      set => toTran = value;
    }

    /// <summary>
    /// A value of HiddenEventDetail.
    /// </summary>
    [JsonPropertyName("hiddenEventDetail")]
    public EventDetail HiddenEventDetail
    {
      get => hiddenEventDetail ??= new();
      set => hiddenEventDetail = value;
    }

    /// <summary>
    /// A value of EventDetail1.
    /// </summary>
    [JsonPropertyName("eventDetail1")]
    public EventDetail EventDetail1
    {
      get => eventDetail1 ??= new();
      set => eventDetail1 = value;
    }

    /// <summary>
    /// A value of HiddenEvent.
    /// </summary>
    [JsonPropertyName("hiddenEvent")]
    public Event1 HiddenEvent
    {
      get => hiddenEvent ??= new();
      set => hiddenEvent = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail2.
    /// </summary>
    [JsonPropertyName("eventDetail2")]
    public Common EventDetail2
    {
      get => eventDetail2 ??= new();
      set => eventDetail2 = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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

    private Code toTran;
    private EventDetail hiddenEventDetail;
    private EventDetail eventDetail1;
    private Event1 hiddenEvent;
    private Event1 event1;
    private Common eventDetail2;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ServiceProvider1.
    /// </summary>
    [JsonPropertyName("serviceProvider1")]
    public Common ServiceProvider1
    {
      get => serviceProvider1 ??= new();
      set => serviceProvider1 = value;
    }

    /// <summary>
    /// A value of ServiceProvider2.
    /// </summary>
    [JsonPropertyName("serviceProvider2")]
    public ServiceProvider ServiceProvider2
    {
      get => serviceProvider2 ??= new();
      set => serviceProvider2 = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of NullServiceProvider.
    /// </summary>
    [JsonPropertyName("nullServiceProvider")]
    public ServiceProvider NullServiceProvider
    {
      get => nullServiceProvider ??= new();
      set => nullServiceProvider = value;
    }

    /// <summary>
    /// A value of NullOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("nullOfficeServiceProvider")]
    public OfficeServiceProvider NullOfficeServiceProvider
    {
      get => nullOfficeServiceProvider ??= new();
      set => nullOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NullOffice.
    /// </summary>
    [JsonPropertyName("nullOffice")]
    public Office NullOffice
    {
      get => nullOffice ??= new();
      set => nullOffice = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Errors.
    /// </summary>
    [JsonPropertyName("errors")]
    public Common Errors
    {
      get => errors ??= new();
      set => errors = value;
    }

    /// <summary>
    /// A value of PassToDateWorkArea.
    /// </summary>
    [JsonPropertyName("passToDateWorkArea")]
    public DateWorkArea PassToDateWorkArea
    {
      get => passToDateWorkArea ??= new();
      set => passToDateWorkArea = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public AlertDistributionRule Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of PassToEvent.
    /// </summary>
    [JsonPropertyName("passToEvent")]
    public Event1 PassToEvent
    {
      get => passToEvent ??= new();
      set => passToEvent = value;
    }

    /// <summary>
    /// A value of PassToEventDetail.
    /// </summary>
    [JsonPropertyName("passToEventDetail")]
    public EventDetail PassToEventDetail
    {
      get => passToEventDetail ??= new();
      set => passToEventDetail = value;
    }

    /// <summary>
    /// A value of PassToAlert.
    /// </summary>
    [JsonPropertyName("passToAlert")]
    public Alert PassToAlert
    {
      get => passToAlert ??= new();
      set => passToAlert = value;
    }

    /// <summary>
    /// A value of ReturnValidation.
    /// </summary>
    [JsonPropertyName("returnValidation")]
    public Common ReturnValidation
    {
      get => returnValidation ??= new();
      set => returnValidation = value;
    }

    /// <summary>
    /// A value of PassToCode.
    /// </summary>
    [JsonPropertyName("passToCode")]
    public Code PassToCode
    {
      get => passToCode ??= new();
      set => passToCode = value;
    }

    /// <summary>
    /// A value of PassToCodeValue.
    /// </summary>
    [JsonPropertyName("passToCodeValue")]
    public CodeValue PassToCodeValue
    {
      get => passToCodeValue ??= new();
      set => passToCodeValue = value;
    }

    /// <summary>
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public AlertDistributionRule ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    /// <summary>
    /// A value of PassToAlertDistributionRule.
    /// </summary>
    [JsonPropertyName("passToAlertDistributionRule")]
    public AlertDistributionRule PassToAlertDistributionRule
    {
      get => passToAlertDistributionRule ??= new();
      set => passToAlertDistributionRule = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    private Common serviceProvider1;
    private ServiceProvider serviceProvider2;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider nullServiceProvider;
    private OfficeServiceProvider nullOfficeServiceProvider;
    private Office nullOffice;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Common errors;
    private DateWorkArea passToDateWorkArea;
    private AlertDistributionRule initialize;
    private Event1 passToEvent;
    private EventDetail passToEventDetail;
    private Alert passToAlert;
    private Common returnValidation;
    private Code passToCode;
    private CodeValue passToCodeValue;
    private AlertDistributionRule returnFrom;
    private AlertDistributionRule passToAlertDistributionRule;
    private Common common;
    private NextTranInfo nextTranInfo;
    private Common prompt;
    private Common select;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    /// <summary>
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private Alert alert;
    private AlertDistributionRule alertDistributionRule;
    private EventDetail eventDetail;
    private Event1 event1;
  }
#endregion
}

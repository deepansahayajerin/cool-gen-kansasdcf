// Program: SP_TRLM_ACT_DISTR_RULE_L_MAINT, ID: 371748607, model: 746.
// Short name: SWETRLMP
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
/// A program: SP_TRLM_ACT_DISTR_RULE_L_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpTrlmActDistrRuleLMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_TRLM_ACT_DISTR_RULE_L_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpTrlmActDistrRuleLMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpTrlmActDistrRuleLMaint.
  /// </summary>
  public SpTrlmActDistrRuleLMaint(IContext context, Import import, Export export)
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
    // Date	 Developer 	Request #    Description
    // 11/15/96 Alan Samuels                Initial Development
    // 2/24/97  Siraj Konkader              Added logic to validate
    // Business Object Code against Event Business Object Code,
    // Added logic to validate Bus Obj Code against Role Codes
    // Added logic to validate Bus Obj Code against Case Unit Func
    // Added logic to restrict addition of more than one distrib-
    // ution rule of type RSP.
    // 03/02/11  GVandy	CQ355
    // For INR events allow activity distributions to CAS, CAU, and INR
    // business objects (previously only supported CAS and CAU distributions).
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Zero.Date = null;
    local.Max.Date = UseCabSetMaximumDiscontinueDate1();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Activity.Assign(import.Activity);
    export.HiddenActivity.ControlNumber = import.HiddenActivity.ControlNumber;
    export.ActivityDetail2.SelectChar = import.ActivityDetail2.SelectChar;
    export.ActivityDetail1.Assign(import.ActivityDetail1);

    // ---------------------------------------------
    // Clear group view if command=clear.
    // ---------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

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
        if (Equal(global.Command, "XXNEXTXX"))
        {
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
        }

        break;
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
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
      case "RETADLM":
        if (AsChar(import.ActivityDetail2.SelectChar) == 'S')
        {
          export.ActivityDetail2.SelectChar = "";

          if (import.FromLinkActivity.ControlNumber > 0 && import
            .FromLinkActivityDetail.SystemGeneratedIdentifier > 0)
          {
            export.Activity.ControlNumber =
              import.FromLinkActivity.ControlNumber;
            export.ActivityDetail1.SystemGeneratedIdentifier =
              import.FromLinkActivityDetail.SystemGeneratedIdentifier;
          }

          var field = GetField(export.Activity, "controlNumber");

          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          global.Command = "DISPLAY";
        }

        break;
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

          export.Group.Update.ActivityDistributionRule.Assign(
            import.Group.Item.ActivityDistributionRule);
          export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);
          export.Group.Update.AssignCode.SelectChar =
            import.Group.Item.AssignCode.SelectChar;
          export.Group.Update.ReasonCode.SelectChar =
            import.Group.Item.ReasonCode.SelectChar;
          export.Group.Update.BusObject.SelectChar =
            import.Group.Item.BusObject.SelectChar;
          export.Group.Update.CuFunction.SelectChar =
            import.Group.Item.CuFunction.SelectChar;
          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;

          if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
          {
            export.Group.Update.BusObject.SelectChar = "";

            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.ActivityDistributionRule.BusinessObjectCode =
                import.FromLinkCodeValue.Cdvalue;
            }

            var field = GetField(export.Group.Item.BusObject, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
          else if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
          {
            export.Group.Update.CuFunction.SelectChar = "";

            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.ActivityDistributionRule.CaseUnitFunction =
                import.FromLinkCodeValue.Cdvalue;
            }

            var field = GetField(export.Group.Item.CuFunction, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
          else if (AsChar(export.Group.Item.ReasonCode.SelectChar) == 'S')
          {
            export.Group.Update.ReasonCode.SelectChar = "";

            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.ActivityDistributionRule.ReasonCode =
                import.FromLinkCodeValue.Cdvalue;
            }

            var field = GetField(export.Group.Item.ReasonCode, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }
          else if (AsChar(export.Group.Item.AssignCode.SelectChar) == 'S')
          {
            export.Group.Update.AssignCode.SelectChar = "";

            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.ActivityDistributionRule.ResponsibilityCode =
                import.FromLinkCodeValue.Cdvalue;
            }

            var field = GetField(export.Group.Item.AssignCode, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
          }

          export.Group.Update.Protect.Flag = import.Group.Item.Protect.Flag;

          if (AsChar(export.Group.Item.Protect.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "businessObjectCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.BusObject, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "caseRoleCode");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "csePersonAcctCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "laPersonCode");

            field6.Color = "cyan";
            field6.Protected = true;

            if (Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              Now().Date) && !
              Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              local.Zero.Date))
            {
              var field7 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "discontinueDate");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseUnitFunction");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "responsibilityCode");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "reasonCode");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Group.Item.AssignCode, "selectChar");

              field11.Intensity = Intensity.Dark;
              field11.Protected = true;

              var field12 = GetField(export.Group.Item.BusObject, "selectChar");

              field12.Intensity = Intensity.Dark;
              field12.Protected = true;

              var field13 =
                GetField(export.Group.Item.CuFunction, "selectChar");

              field13.Intensity = Intensity.Dark;
              field13.Protected = true;

              var field14 =
                GetField(export.Group.Item.ReasonCode, "selectChar");

              field14.Intensity = Intensity.Dark;
              field14.Protected = true;
            }
          }

          export.Group.Next();
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

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

      if (AsChar(export.ActivityDetail2.SelectChar) == 'S')
      {
        ++local.Prompt.Count;

        var field = GetField(export.ActivityDetail2, "selectChar");

        field.Error = true;
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

        export.Group.Update.ActivityDistributionRule.Assign(
          import.Group.Item.ActivityDistributionRule);
        export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);
        export.Group.Update.AssignCode.SelectChar =
          import.Group.Item.AssignCode.SelectChar;

        if (AsChar(export.Group.Item.AssignCode.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.AssignCode, "selectChar");

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

        export.Group.Update.BusObject.SelectChar =
          import.Group.Item.BusObject.SelectChar;

        if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.BusObject, "selectChar");

          field.Error = true;

          ++local.Prompt.Count;
        }

        export.Group.Update.CuFunction.SelectChar =
          import.Group.Item.CuFunction.SelectChar;

        if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.CuFunction, "selectChar");

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

        export.Group.Update.Protect.Flag = import.Group.Item.Protect.Flag;

        if (AsChar(export.Group.Item.Protect.Flag) == 'Y')
        {
          var field1 =
            GetField(export.Group.Item.ActivityDistributionRule,
            "businessObjectCode");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Group.Item.BusObject, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Group.Item.ActivityDistributionRule, "effectiveDate");
            

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 =
            GetField(export.Group.Item.ActivityDistributionRule, "caseRoleCode");
            

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.Group.Item.ActivityDistributionRule,
            "csePersonAcctCode");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.Group.Item.ActivityDistributionRule, "laPersonCode");
            

          field6.Color = "cyan";
          field6.Protected = true;

          if (Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
            Now().Date) && !
            Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
            local.Zero.Date))
          {
            var field7 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "discontinueDate");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "caseUnitFunction");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "responsibilityCode");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Group.Item.ActivityDistributionRule, "reasonCode");
              

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.Group.Item.AssignCode, "selectChar");

            field11.Intensity = Intensity.Dark;
            field11.Protected = true;

            var field12 = GetField(export.Group.Item.BusObject, "selectChar");

            field12.Intensity = Intensity.Dark;
            field12.Protected = true;

            var field13 = GetField(export.Group.Item.CuFunction, "selectChar");

            field13.Intensity = Intensity.Dark;
            field13.Protected = true;

            var field14 = GetField(export.Group.Item.ReasonCode, "selectChar");

            field14.Intensity = Intensity.Dark;
            field14.Protected = true;
          }
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
      if (import.Activity.ControlNumber == import.HiddenActivity.ControlNumber)
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

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (Equal(export.Group.Item.ActivityDistributionRule.
            ResponsibilityCode, "RSP"))
          {
            ++local.Rsp.Count;
          }

          if (local.Rsp.Count > 1)
          {
            var field =
              GetField(export.Group.Item.ActivityDistributionRule,
              "responsibilityCode");

            field.Error = true;

            ExitState = "SP0000_ONLY_ONE_RSP_ALLOWED";

            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;

            // ---------------------------------------------
            // An add must be on a previously blank row.
            // ---------------------------------------------
            if (export.Group.Item.Hidden.SystemGeneratedIdentifier > 0)
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

              return;
            }

            // ---------------------------------------------
            // Perform data validation for add request.
            // ---------------------------------------------
            local.PassToActivityDistributionRule.Assign(
              export.Group.Item.ActivityDistributionRule);

            // ---------------------------------------------
            // If entered, only one role code is allowed.
            // ---------------------------------------------
            local.Common.Count = 0;

            switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
              LaPersonCode ?? ""))
            {
              case "":
                break;
              case "C":
                ++local.Common.Count;

                break;
              case "P":
                ++local.Common.Count;

                break;
              case "R":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "laPersonCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
              LaCaseRoleCode ?? ""))
            {
              case "":
                break;
              case "AP":
                ++local.Common.Count;

                break;
              case "AR":
                ++local.Common.Count;

                break;
              case "CH":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "laCaseRoleCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
              CsenetRoleCode ?? ""))
            {
              case "":
                break;
              case "AP":
                ++local.Common.Count;

                break;
              case "AR":
                ++local.Common.Count;

                break;
              case "CH":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "csenetRoleCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
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
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "csePersonAcctCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
              CaseRoleCode ?? ""))
            {
              case "":
                break;
              case "AP":
                ++local.Common.Count;

                break;
              case "AR":
                ++local.Common.Count;

                break;
              case "CH":
                ++local.Common.Count;

                break;
              default:
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "caseRoleCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";

                break;
            }

            if (local.Common.Count > 1)
            {
              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "laPersonCode");

              field1.Error = true;

              var field2 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "csePersonAcctCode");

              field2.Error = true;

              var field3 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseRoleCode");

              field3.Error = true;

              ++local.Errors.Count;
              ExitState = "SP0000_ONLY_ONE_CODE_ALLOWED";
            }
            else if (local.Common.Count == 1)
            {
              switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                BusinessObjectCode))
              {
                case "CAS":
                  if (IsEmpty(export.Group.Item.ActivityDistributionRule.
                    CaseRoleCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "caseRoleCode");

                    field3.Error = true;
                  }

                  break;
                case "CAU":
                  if (IsEmpty(export.Group.Item.ActivityDistributionRule.
                    CaseRoleCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "caseRoleCode");

                    field3.Error = true;
                  }

                  break;
                case "LRF":
                  if (IsEmpty(export.Group.Item.ActivityDistributionRule.
                    CaseRoleCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "caseRoleCode");

                    field3.Error = true;
                  }

                  break;
                case "ADA":
                  if (IsEmpty(export.Group.Item.ActivityDistributionRule.
                    CsePersonAcctCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "csePersonAcctCode");

                    field3.Error = true;
                  }

                  break;
                case "OAA":
                  if (IsEmpty(export.Group.Item.ActivityDistributionRule.
                    CsePersonAcctCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "csePersonAcctCode");

                    field3.Error = true;
                  }

                  break;
                case "LEA":
                  if (IsEmpty(export.Group.Item.ActivityDistributionRule.
                    LaPersonCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "laPersonCode");

                    field3.Error = true;
                  }

                  break;
                case "OBL":
                  if (IsEmpty(export.Group.Item.ActivityDistributionRule.
                    CsePersonAcctCode))
                  {
                    ++local.Errors.Count;
                    ExitState = "SP0000_INVALID_ROLE_FOR_BUS_OBJ";

                    var field2 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "businessObjectCode");

                    field2.Error = true;

                    var field3 =
                      GetField(export.Group.Item.ActivityDistributionRule,
                      "csePersonAcctCode");

                    field3.Error = true;
                  }

                  break;
                default:
                  ExitState = "SP0000_ONLY_ASSIGNABLE_BUS_OBJ";
                  ++local.Errors.Count;

                  var field1 =
                    GetField(export.Group.Item.ActivityDistributionRule,
                    "businessObjectCode");

                  field1.Error = true;

                  break;
              }
            }

            // ---------------------------------------------
            // Effective date default is current date.
            // ---------------------------------------------
            if (Equal(export.Group.Item.ActivityDistributionRule.EffectiveDate,
              local.Zero.Date))
            {
              local.PassToActivityDistributionRule.EffectiveDate =
                local.Current.Date;
              export.Group.Update.ActivityDistributionRule.EffectiveDate =
                local.Current.Date;
            }
            else if (!Lt(export.Group.Item.ActivityDistributionRule.
              EffectiveDate, local.Current.Date))
            {
            }
            else
            {
              ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "effectiveDate");

              field1.Error = true;

              ++local.Errors.Count;
            }

            // ---------------------------------------------
            // Discontinue date must be > effective date.
            // Default value = 12/31/2099.
            // ---------------------------------------------
            if (Equal(export.Group.Item.ActivityDistributionRule.
              DiscontinueDate, local.Zero.Date))
            {
              local.PassToActivityDistributionRule.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate2();
            }
            else if (!Lt(export.Group.Item.ActivityDistributionRule.
              DiscontinueDate,
              export.Group.Item.ActivityDistributionRule.EffectiveDate) && !
              Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              local.Current.Date))
            {
            }
            else
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "discontinueDate");

              field1.Error = true;

              ExitState = "SP0000_INVALID_DISC_DATE";
            }

            // ---------------------------------------------
            // Responsibility Code is required and must be valid.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.ActivityDistributionRule.
              ResponsibilityCode))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "responsibilityCode");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              local.PassToValidateCode.CodeName = "MONA ASSIGNMENT REASON CODE";
              local.PassToValidateCodeValue.Cdvalue =
                export.Group.Item.ActivityDistributionRule.ResponsibilityCode;
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnFromValidate.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "responsibilityCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
            }

            // ---------------------------------------------
            // Reason Code is required and must be valid.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.ActivityDistributionRule.ReasonCode))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "reasonCode");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              if (Equal(export.Group.Item.ActivityDistributionRule.
                BusinessObjectCode, "LEA"))
              {
                local.PassToValidateCode.CodeName =
                  "LEGAL ASSIGNMENT REASON CODE";
              }
              else
              {
                local.PassToValidateCode.CodeName = "ASSIGNMENT REASON CODE";
              }

              local.PassToValidateCodeValue.Cdvalue =
                export.Group.Item.ActivityDistributionRule.ReasonCode;
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnFromValidate.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "reasonCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
            }

            // ---------------------------------------------
            // CU function must be a valid value.  It is
            // required if business code is for a case unit.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.ActivityDistributionRule.
              CaseUnitFunction))
            {
              local.PassToValidateCode.CodeName = "CASE UNIT FUNCTION";
              local.PassToValidateCodeValue.Cdvalue =
                export.Group.Item.ActivityDistributionRule.CaseUnitFunction ?? Spaces
                (10);
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnFromValidate.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "caseUnitFunction");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
              else if (!Equal(export.Group.Item.ActivityDistributionRule.
                BusinessObjectCode, "CAU"))
              {
                ++local.Errors.Count;
                ExitState = "SP0000_BUS_OBJ_MUST_BE_CAU";

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "caseUnitFunction");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "businessObjectCode");

                field2.Error = true;
              }
            }
            else if (Equal(export.Group.Item.ActivityDistributionRule.
              BusinessObjectCode, "CAU"))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseUnitFunction");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }

            // ---------------------------------------------
            // Business Object must be a valid value.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.ActivityDistributionRule.
              BusinessObjectCode))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "businessObjectCode");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              local.PassToValidateCode.CodeName = "BUSINESS OBJECT CODE";
              local.PassToValidateCodeValue.Cdvalue =
                export.Group.Item.ActivityDistributionRule.BusinessObjectCode;
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnFromValidate.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "businessObjectCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
              else
              {
                switch(TrimEnd(export.ActivityDetail1.BusinessObjectCode ?? ""))
                {
                  case "CAS":
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ExitState = "SP0000_ENTER_CAS_CAU_LRF";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  case "CAU":
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ExitState = "SP0000_ENTER_CAS_CAU_LRF";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  case "LRF":
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ExitState = "SP0000_ENTER_CAS_CAU_LRF";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  case "ADA":
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
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
                        ExitState = "SP0000_SELECT_CAS_CAU_ADA_OAA";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  case "OBL":
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
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
                        ExitState = "SP0000_ENTER_CAS_CAU_LEA_OBL";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  case "OAA":
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LEA":
                        break;
                      case "OBL":
                        break;
                      case "ADA":
                        break;
                      case "OAA":
                        break;
                      default:
                        ExitState = "SP0000_USE_CAS_CAU_LEA_OBL_AD_OA";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  case "LEA":
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
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
                        ExitState = "SP0000_ENTER_CAS_CAU_LEA_OBL";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  case "INR":
                    // 03/02/11  GVandy  CQ355  For INR events allow alert 
                    // distributions to CAS, CAU, and INR
                    // business objects (previously only supported CAS and CAU 
                    // distributions).
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "INR":
                        break;
                      default:
                        ExitState = "SP0000_ENTER_CAS_CAU_INR";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                  default:
                    // ------------------------------------------------------------
                    // For all other Event Business Objects, the only 
                    // distribution
                    // must be on Case or Case Unit.
                    // ------------------------------------------------------------
                    switch(TrimEnd(export.Group.Item.ActivityDistributionRule.
                      BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      default:
                        ExitState = "SP0000_ENTER_CAS_CAU";
                        ++local.Errors.Count;

                        var field1 =
                          GetField(export.Group.Item.ActivityDistributionRule,
                          "businessObjectCode");

                        field1.Error = true;

                        break;
                    }

                    break;
                }
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

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            break;
          }

          // ---------------------------------------------
          // Data has passed validation. Create activity
          // detail.
          // ---------------------------------------------
          UseSpCabCreateActivityDistrRul();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_ADD_SUCCESSFUL"))
          {
            if (Equal(local.ReturnFrom.DiscontinueDate, local.Max.Date))
            {
              local.ReturnFrom.DiscontinueDate = local.Zero.Date;
            }

            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.ActivityDistributionRule.
              Assign(local.ReturnFrom);
            export.Group.Update.Hidden.Assign(local.ReturnFrom);
          }
          else
          {
            break;
          }
        }

        // *** Set the Protection ***
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Protect.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "businessObjectCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.BusObject, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "caseRoleCode");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "csePersonAcctCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "laPersonCode");

            field6.Color = "cyan";
            field6.Protected = true;

            if (Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              Now().Date) && !
              Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              local.Zero.Date))
            {
              var field7 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "discontinueDate");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseUnitFunction");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "responsibilityCode");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "reasonCode");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Group.Item.AssignCode, "selectChar");

              field11.Intensity = Intensity.Dark;
              field11.Protected = true;

              var field12 = GetField(export.Group.Item.BusObject, "selectChar");

              field12.Intensity = Intensity.Dark;
              field12.Protected = true;

              var field13 =
                GetField(export.Group.Item.CuFunction, "selectChar");

              field13.Intensity = Intensity.Dark;
              field13.Protected = true;

              var field14 =
                GetField(export.Group.Item.ReasonCode, "selectChar");

              field14.Intensity = Intensity.Dark;
              field14.Protected = true;
            }
          }
        }

        break;
      case "UPDATE":
        local.Zero.Date = null;
        local.Errors.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (Equal(export.Group.Item.ActivityDistributionRule.
            ResponsibilityCode, "RSP"))
          {
            ++local.Rsp.Count;
          }

          if (local.Rsp.Count > 1)
          {
            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "responsibilityCode");

            field1.Error = true;

            ExitState = "SP0000_ONLY_ONE_RSP_ALLOWED";

            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) != 'S')
          {
            continue;
          }

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Protected = false;

          // ---------------------------------------------
          // An update must be performed on a populated
          // row.
          // ---------------------------------------------
          if (export.Group.Item.Hidden.SystemGeneratedIdentifier == 0)
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

            return;
          }

          // ---------------------------------------------
          // Perform data validation for update request.
          // ---------------------------------------------
          // ---------------------------------------------
          // Case Unit Function, Reason Code, Responsibility
          // Code or Discontinue Date must have changed.
          // ---------------------------------------------
          if (Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
            export.Group.Item.Hidden.DiscontinueDate) && Equal
            (export.Group.Item.ActivityDistributionRule.EffectiveDate,
            export.Group.Item.Hidden.EffectiveDate) && Equal
            (export.Group.Item.ActivityDistributionRule.ReasonCode,
            export.Group.Item.Hidden.ReasonCode) && Equal
            (export.Group.Item.ActivityDistributionRule.ResponsibilityCode,
            export.Group.Item.Hidden.ResponsibilityCode) && Equal
            (export.Group.Item.ActivityDistributionRule.CaseUnitFunction,
            export.Group.Item.Hidden.CaseUnitFunction))
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_DATA_NOT_CHANGED";

            return;
          }

          local.PassToActivityDistributionRule.Assign(
            export.Group.Item.ActivityDistributionRule);

          // ---------------------------------------------
          // None of the Role Codes can be changed.
          // ---------------------------------------------
          if (Equal(export.Group.Item.ActivityDistributionRule.LaPersonCode,
            export.Group.Item.Hidden.LaPersonCode))
          {
          }
          else
          {
            ++local.Errors.Count;
            export.Group.Update.ActivityDistributionRule.LaPersonCode =
              export.Group.Item.Hidden.LaPersonCode ?? "";

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "laPersonCode");

            field1.Error = true;

            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
          }

          if (Equal(export.Group.Item.ActivityDistributionRule.LaCaseRoleCode,
            export.Group.Item.Hidden.LaCaseRoleCode))
          {
          }
          else
          {
            ++local.Errors.Count;
            export.Group.Update.ActivityDistributionRule.LaCaseRoleCode =
              export.Group.Item.Hidden.LaCaseRoleCode ?? "";

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "laCaseRoleCode");

            field1.Error = true;

            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
          }

          if (Equal(export.Group.Item.ActivityDistributionRule.CsenetRoleCode,
            export.Group.Item.Hidden.CsenetRoleCode))
          {
          }
          else
          {
            ++local.Errors.Count;
            export.Group.Update.ActivityDistributionRule.CsenetRoleCode =
              export.Group.Item.Hidden.CsenetRoleCode ?? "";

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "csenetRoleCode");

            field1.Error = true;

            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
          }

          if (Equal(export.Group.Item.ActivityDistributionRule.
            CsePersonAcctCode, export.Group.Item.Hidden.CsePersonAcctCode))
          {
          }
          else
          {
            ++local.Errors.Count;
            export.Group.Update.ActivityDistributionRule.CsePersonAcctCode =
              export.Group.Item.Hidden.CsePersonAcctCode ?? "";

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "csePersonAcctCode");

            field1.Error = true;

            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
          }

          if (Equal(export.Group.Item.ActivityDistributionRule.CaseRoleCode,
            export.Group.Item.Hidden.CaseRoleCode))
          {
          }
          else
          {
            ++local.Errors.Count;
            export.Group.Update.ActivityDistributionRule.CaseRoleCode =
              export.Group.Item.Hidden.CaseRoleCode ?? "";

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "caseRoleCode");

            field1.Error = true;

            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
          }

          if (!Equal(export.Group.Item.ActivityDistributionRule.EffectiveDate,
            export.Group.Item.Hidden.EffectiveDate))
          {
            if (Equal(export.Group.Item.Hidden.EffectiveDate, local.Current.Date))
              
            {
              local.PassToActivityDistributionRule.EffectiveDate =
                local.Current.Date;
              export.Group.Update.ActivityDistributionRule.EffectiveDate =
                local.Current.Date;
            }
            else if (!Lt(export.Group.Item.ActivityDistributionRule.
              EffectiveDate, local.Current.Date))
            {
            }
            else
            {
              ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "effectiveDate");

              field1.Error = true;

              ++local.Errors.Count;
            }
          }

          // ---------------------------------------------
          // Discontinue date must be > effective date.
          // Default value = 12/31/2099.
          // ---------------------------------------------
          if (Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
            local.Zero.Date))
          {
            local.PassToActivityDistributionRule.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate2();
          }
          else if (!Lt(export.Group.Item.ActivityDistributionRule.
            DiscontinueDate,
            export.Group.Item.ActivityDistributionRule.EffectiveDate) && !
            Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
            local.Current.Date))
          {
          }
          else
          {
            ++local.Errors.Count;

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "discontinueDate");

            field1.Error = true;

            ExitState = "SP0000_INVALID_DISC_DATE";
          }

          // ---------------------------------------------
          // Prompt is not valid on update.
          // ---------------------------------------------
          if (AsChar(export.Group.Item.AssignCode.SelectChar) == 'S')
          {
            ++local.Errors.Count;

            var field1 = GetField(export.Group.Item.AssignCode, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_PROMPT_NOT_ALLOWED";
          }

          // ---------------------------------------------
          // Responsibility Code is required and must be valid.
          // ---------------------------------------------
          if (IsEmpty(export.Group.Item.ActivityDistributionRule.
            ResponsibilityCode))
          {
            ++local.Errors.Count;

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "responsibilityCode");

            field1.Error = true;

            ExitState = "SP0000_REQUIRED_FIELD_MISSING";
          }
          else
          {
            local.PassToValidateCode.CodeName = "MONA ASSIGNMENT REASON CODE";
            local.PassToValidateCodeValue.Cdvalue =
              export.Group.Item.ActivityDistributionRule.ResponsibilityCode;
            UseCabValidateCodeValue();

            if (AsChar(local.ReturnFromValidate.Flag) == 'N')
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "responsibilityCode");

              field1.Error = true;

              ExitState = "SP0000_INVALID_VALUE_ENTERED";
            }
          }

          // ---------------------------------------------
          // Prompt is not valid on update.
          // ---------------------------------------------
          if (AsChar(export.Group.Item.ReasonCode.SelectChar) == 'S')
          {
            ++local.Errors.Count;

            var field1 = GetField(export.Group.Item.ReasonCode, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_PROMPT_NOT_ALLOWED";
          }

          if (Equal(export.Group.Item.ActivityDistributionRule.ReasonCode,
            export.Group.Item.Hidden.ReasonCode) && !
            IsEmpty(export.Group.Item.ActivityDistributionRule.ReasonCode))
          {
          }
          else
          {
            // ---------------------------------------------
            // Reason Code is required and must be valid.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.ActivityDistributionRule.ReasonCode))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "reasonCode");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              if (Equal(export.Group.Item.ActivityDistributionRule.
                BusinessObjectCode, "LEA"))
              {
                local.PassToValidateCode.CodeName =
                  "LEGAL ASSIGNMENT REASON CODE";
              }
              else
              {
                local.PassToValidateCode.CodeName = "ASSIGNMENT REASON CODE";
              }

              local.PassToValidateCodeValue.Cdvalue =
                export.Group.Item.ActivityDistributionRule.ReasonCode;
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnFromValidate.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "reasonCode");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
            }
          }

          // ---------------------------------------------
          // Prompt is not valid on update.
          // ---------------------------------------------
          if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
          {
            ++local.Errors.Count;

            var field1 = GetField(export.Group.Item.CuFunction, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_PROMPT_NOT_ALLOWED";
          }

          if (Equal(export.Group.Item.ActivityDistributionRule.CaseUnitFunction,
            export.Group.Item.Hidden.CaseUnitFunction))
          {
          }
          else
          {
            // ---------------------------------------------
            // CU function must be a valid value.  It is
            // required if business code is for a case unit.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.ActivityDistributionRule.
              CaseUnitFunction))
            {
              local.PassToValidateCode.CodeName = "CASE UNIT FUNCTION";
              local.PassToValidateCodeValue.Cdvalue =
                export.Group.Item.ActivityDistributionRule.CaseUnitFunction ?? Spaces
                (10);
              UseCabValidateCodeValue();

              if (AsChar(local.ReturnFromValidate.Flag) == 'N')
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "caseUnitFunction");

                field1.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }
              else if (!Equal(export.Group.Item.ActivityDistributionRule.
                BusinessObjectCode, "CAU"))
              {
                ++local.Errors.Count;

                var field1 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "caseUnitFunction");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ActivityDistributionRule,
                  "businessObjectCode");

                field2.Error = true;

                ExitState = "SP0000_BUS_OBJ_MUST_BE_CAU";
              }
            }
            else if (Equal(export.Group.Item.Hidden.BusinessObjectCode, "CAU"))
            {
              ++local.Errors.Count;

              var field1 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseUnitFunction");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
          }

          // ---------------------------------------------
          // Prompt is not valid on update.
          // ---------------------------------------------
          if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
          {
            ++local.Errors.Count;

            var field1 = GetField(export.Group.Item.BusObject, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_PROMPT_NOT_ALLOWED";
          }

          // ---------------------------------------------
          // Business object code may not be changed..
          // ---------------------------------------------
          if (Equal(export.Group.Item.ActivityDistributionRule.
            BusinessObjectCode, export.Group.Item.Hidden.BusinessObjectCode))
          {
          }
          else
          {
            ++local.Errors.Count;
            export.Group.Update.ActivityDistributionRule.BusinessObjectCode =
              export.Group.Item.Hidden.BusinessObjectCode;

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "businessObjectCode");

            field1.Error = true;

            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
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
          // Data has passed validation. Update Activity
          // Distr rule.
          // ---------------------------------------------
          local.PassToActivityDistributionRule.Assign(
            export.Group.Item.ActivityDistributionRule);

          if (Equal(local.PassToActivityDistributionRule.DiscontinueDate,
            local.Zero.Date))
          {
            local.PassToDateWorkArea.Date =
              local.PassToActivityDistributionRule.DiscontinueDate;
            local.PassToActivityDistributionRule.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate2();
          }

          UseSpCabUpdateActivityDistrRul();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
            if (Equal(local.ReturnFrom.DiscontinueDate, local.Max.Date))
            {
              local.ReturnFrom.DiscontinueDate = local.Zero.Date;
            }

            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.ActivityDistributionRule.
              Assign(local.ReturnFrom);
            export.Group.Update.Hidden.Assign(local.ReturnFrom);
          }
          else
          {
            return;
          }
        }

        // *** Set the Protection ***
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Protect.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "businessObjectCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.BusObject, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "caseRoleCode");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "csePersonAcctCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "laPersonCode");

            field6.Color = "cyan";
            field6.Protected = true;

            if (Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              Now().Date) && !
              Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              local.Zero.Date))
            {
              var field7 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "discontinueDate");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseUnitFunction");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "responsibilityCode");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "reasonCode");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Group.Item.AssignCode, "selectChar");

              field11.Intensity = Intensity.Dark;
              field11.Protected = true;

              var field12 = GetField(export.Group.Item.BusObject, "selectChar");

              field12.Intensity = Intensity.Dark;
              field12.Protected = true;

              var field13 =
                GetField(export.Group.Item.CuFunction, "selectChar");

              field13.Intensity = Intensity.Dark;
              field13.Protected = true;

              var field14 =
                GetField(export.Group.Item.ReasonCode, "selectChar");

              field14.Intensity = Intensity.Dark;
              field14.Protected = true;
            }
          }
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          // *** Set the Protection ***
          if (AsChar(export.Group.Item.Protect.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "businessObjectCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.BusObject, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "caseRoleCode");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "csePersonAcctCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "laPersonCode");

            field6.Color = "cyan";
            field6.Protected = true;

            if (Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              Now().Date) && !
              Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              local.Zero.Date))
            {
              var field7 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "discontinueDate");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseUnitFunction");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "responsibilityCode");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "reasonCode");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Group.Item.AssignCode, "selectChar");

              field11.Intensity = Intensity.Dark;
              field11.Protected = true;

              var field12 = GetField(export.Group.Item.BusObject, "selectChar");

              field12.Intensity = Intensity.Dark;
              field12.Protected = true;

              var field13 =
                GetField(export.Group.Item.CuFunction, "selectChar");

              field13.Intensity = Intensity.Dark;
              field13.Protected = true;

              var field14 =
                GetField(export.Group.Item.ReasonCode, "selectChar");

              field14.Intensity = Intensity.Dark;
              field14.Protected = true;
            }
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            // ---------------------------------------------
            // A delete must be performed on a populated
            // row.
            // ---------------------------------------------
            if (export.Group.Item.Hidden.SystemGeneratedIdentifier == 0)
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

              return;
            }

            // ---------------------------------------------
            // Prompt is not valid on delete.
            // ---------------------------------------------
            if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.CuFunction, "selectChar");

              field.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.CuFunction, "selectChar");

              field.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_DELETE_SUCCESSFUL"))
            {
            }
            else
            {
              return;
            }

            local.PassToActivityDistributionRule.SystemGeneratedIdentifier =
              export.Group.Item.ActivityDistributionRule.
                SystemGeneratedIdentifier;
            UseSpCabDeleteActivityDistrRul();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_DELETE_SUCCESSFUL"))
            {
              ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
              export.Group.Update.Common.SelectChar = "";

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Protected = false;

              export.Group.Update.ActivityDistributionRule.Assign(
                local.Initialize);
              export.Group.Update.Hidden.Assign(local.Initialize);
            }
            else
            {
              return;
            }
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

        if (AsChar(import.ActivityDetail2.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_ADLM";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
            export.ToList.CodeName = "BUSINESS OBJECT CODE";

            return;
          }

          if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
            export.ToList.CodeName = "CASE UNIT FUNCTION";

            return;
          }

          if (AsChar(export.Group.Item.AssignCode.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
            export.ToList.CodeName = "MONA ASSIGNMENT REASON CODE";

            return;
          }

          if (AsChar(export.Group.Item.ReasonCode.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            if (Equal(export.Group.Item.ActivityDistributionRule.
              BusinessObjectCode, "LEA"))
            {
              export.ToList.CodeName = "LEGAL ASSIGNMENT REASON CODE";
            }
            else
            {
              export.ToList.CodeName = "ASSIGNMENT REASON CODE";
            }

            return;
          }
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.ActivityDetail2.SelectChar = "";

      if (ReadActivity())
      {
        MoveActivity(entities.Activity, export.Activity);
        export.HiddenActivity.ControlNumber = entities.Activity.ControlNumber;

        if (ReadActivityDetail())
        {
          export.ActivityDetail1.Assign(entities.ActivityDetail);

          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadActivityDistributionRule())
          {
            export.Group.Update.ActivityDistributionRule.Assign(
              entities.ActivityDistributionRule);
            export.Group.Update.Hidden.
              Assign(entities.ActivityDistributionRule);
            local.PassToDateWorkArea.Date =
              entities.ActivityDistributionRule.DiscontinueDate;
            export.Group.Update.ActivityDistributionRule.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate2();
            export.Group.Update.Hidden.DiscontinueDate =
              export.Group.Item.ActivityDistributionRule.DiscontinueDate;

            var field1 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "businessObjectCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.BusObject, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "caseRoleCode");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "csePersonAcctCode");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Group.Item.ActivityDistributionRule,
              "laPersonCode");

            field6.Color = "cyan";
            field6.Protected = true;

            export.Group.Update.Protect.Flag = "Y";

            if (Lt(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              Now().Date) && !
              Equal(export.Group.Item.ActivityDistributionRule.DiscontinueDate,
              local.Zero.Date))
            {
              var field7 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "discontinueDate");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "caseUnitFunction");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "responsibilityCode");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Group.Item.ActivityDistributionRule,
                "reasonCode");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Group.Item.AssignCode, "selectChar");

              field11.Intensity = Intensity.Dark;
              field11.Protected = true;

              var field12 = GetField(export.Group.Item.BusObject, "selectChar");

              field12.Intensity = Intensity.Dark;
              field12.Protected = true;

              var field13 =
                GetField(export.Group.Item.CuFunction, "selectChar");

              field13.Intensity = Intensity.Dark;
              field13.Protected = true;

              var field14 =
                GetField(export.Group.Item.ReasonCode, "selectChar");

              field14.Intensity = Intensity.Dark;
              field14.Protected = true;
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
        else
        {
          export.ActivityDetail1.BusinessObjectCode = "";
          export.ActivityDetail1.RegulationSourceDescription = "";
          export.ActivityDetail1.SystemGeneratedIdentifier = 0;

          if (export.ActivityDetail1.SystemGeneratedIdentifier > 0)
          {
            ExitState = "SP0000_ACTIVITY_DETAIL_NF";

            var field =
              GetField(export.ActivityDetail1, "systemGeneratedIdentifier");

            field.Error = true;
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            var field =
              GetField(export.ActivityDetail1, "systemGeneratedIdentifier");

            field.Error = true;
          }
        }
      }
      else
      {
        export.ActivityDetail1.BusinessObjectCode = "";
        export.ActivityDetail1.RegulationSourceDescription = "";
        export.ActivityDetail1.SystemGeneratedIdentifier = 0;

        if (export.Activity.ControlNumber > 0)
        {
          ExitState = "SP0000_ACTIVITY_NF";

          var field = GetField(export.Activity, "controlNumber");

          field.Error = true;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

          var field = GetField(export.Activity, "controlNumber");

          field.Protected = false;
          field.Focused = true;
        }
      }
    }
  }

  private static void MoveActivity(Activity source, Activity target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Name = source.Name;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
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

    useImport.CodeValue.Cdvalue = local.PassToValidateCodeValue.Cdvalue;
    useImport.Code.CodeName = local.PassToValidateCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnFromValidate.Flag = useExport.ValidCode.Flag;
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

  private void UseSpCabCreateActivityDistrRul()
  {
    var useImport = new SpCabCreateActivityDistrRul.Import();
    var useExport = new SpCabCreateActivityDistrRul.Export();

    useImport.ActivityDetail.SystemGeneratedIdentifier =
      import.ActivityDetail1.SystemGeneratedIdentifier;
    useImport.Activity.ControlNumber = import.Activity.ControlNumber;
    useImport.ActivityDistributionRule.Assign(
      local.PassToActivityDistributionRule);

    Call(SpCabCreateActivityDistrRul.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.ActivityDistributionRule);
  }

  private void UseSpCabDeleteActivityDistrRul()
  {
    var useImport = new SpCabDeleteActivityDistrRul.Import();
    var useExport = new SpCabDeleteActivityDistrRul.Export();

    useImport.ActivityDistributionRule.SystemGeneratedIdentifier =
      local.PassToActivityDistributionRule.SystemGeneratedIdentifier;
    useImport.ActivityDetail.SystemGeneratedIdentifier =
      export.ActivityDetail1.SystemGeneratedIdentifier;
    useImport.Activity.ControlNumber = export.Activity.ControlNumber;

    Call(SpCabDeleteActivityDistrRul.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateActivityDistrRul()
  {
    var useImport = new SpCabUpdateActivityDistrRul.Import();
    var useExport = new SpCabUpdateActivityDistrRul.Export();

    useImport.ActivityDistributionRule.Assign(
      local.PassToActivityDistributionRule);
    useImport.ActivityDetail.SystemGeneratedIdentifier =
      export.ActivityDetail1.SystemGeneratedIdentifier;
    useImport.Activity.ControlNumber = export.Activity.ControlNumber;

    Call(SpCabUpdateActivityDistrRul.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.ActivityDistributionRule);
  }

  private bool ReadActivity()
  {
    entities.Activity.Populated = false;

    return Read("ReadActivity",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.Activity.Populated = true;
      });
  }

  private bool ReadActivityDetail()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", entities.Activity.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          export.ActivityDetail1.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate", local.Zero.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.BusinessObjectCode =
          db.GetNullableString(reader, 1);
        entities.ActivityDetail.CaseUnitFunction =
          db.GetNullableString(reader, 2);
        entities.ActivityDetail.RegulationSourceDescription =
          db.GetNullableString(reader, 3);
        entities.ActivityDetail.EffectiveDate = db.GetDate(reader, 4);
        entities.ActivityDetail.DiscontinueDate = db.GetDate(reader, 5);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 6);
        entities.ActivityDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadActivityDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);

    return ReadEach("ReadActivityDistributionRule",
      (db, command) =>
      {
        db.SetInt32(command, "actControlNo", entities.ActivityDetail.ActNo);
        db.SetInt32(
          command, "acdId", entities.ActivityDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ActivityDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDistributionRule.BusinessObjectCode =
          db.GetString(reader, 1);
        entities.ActivityDistributionRule.CaseUnitFunction =
          db.GetNullableString(reader, 2);
        entities.ActivityDistributionRule.ReasonCode = db.GetString(reader, 3);
        entities.ActivityDistributionRule.ResponsibilityCode =
          db.GetString(reader, 4);
        entities.ActivityDistributionRule.CaseRoleCode =
          db.GetNullableString(reader, 5);
        entities.ActivityDistributionRule.CsePersonAcctCode =
          db.GetNullableString(reader, 6);
        entities.ActivityDistributionRule.CsenetRoleCode =
          db.GetNullableString(reader, 7);
        entities.ActivityDistributionRule.LaCaseRoleCode =
          db.GetNullableString(reader, 8);
        entities.ActivityDistributionRule.LaPersonCode =
          db.GetNullableString(reader, 9);
        entities.ActivityDistributionRule.EffectiveDate =
          db.GetDate(reader, 10);
        entities.ActivityDistributionRule.DiscontinueDate =
          db.GetDate(reader, 11);
        entities.ActivityDistributionRule.ActControlNo =
          db.GetInt32(reader, 12);
        entities.ActivityDistributionRule.AcdId = db.GetInt32(reader, 13);
        entities.ActivityDistributionRule.Populated = true;

        return true;
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
      /// A value of Protect.
      /// </summary>
      [JsonPropertyName("protect")]
      public Common Protect
      {
        get => protect ??= new();
        set => protect = value;
      }

      /// <summary>
      /// A value of AssignCode.
      /// </summary>
      [JsonPropertyName("assignCode")]
      public Common AssignCode
      {
        get => assignCode ??= new();
        set => assignCode = value;
      }

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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ActivityDistributionRule Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of ActivityDistributionRule.
      /// </summary>
      [JsonPropertyName("activityDistributionRule")]
      public ActivityDistributionRule ActivityDistributionRule
      {
        get => activityDistributionRule ??= new();
        set => activityDistributionRule = value;
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
      /// A value of BusObject.
      /// </summary>
      [JsonPropertyName("busObject")]
      public Common BusObject
      {
        get => busObject ??= new();
        set => busObject = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common protect;
      private Common assignCode;
      private Common reasonCode;
      private ActivityDistributionRule hidden;
      private ActivityDistributionRule activityDistributionRule;
      private Common cuFunction;
      private Common busObject;
      private Common common;
    }

    /// <summary>
    /// A value of FromLinkActivityDetail.
    /// </summary>
    [JsonPropertyName("fromLinkActivityDetail")]
    public ActivityDetail FromLinkActivityDetail
    {
      get => fromLinkActivityDetail ??= new();
      set => fromLinkActivityDetail = value;
    }

    /// <summary>
    /// A value of ActivityDetail1.
    /// </summary>
    [JsonPropertyName("activityDetail1")]
    public ActivityDetail ActivityDetail1
    {
      get => activityDetail1 ??= new();
      set => activityDetail1 = value;
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
    /// A value of FromLinkActivity.
    /// </summary>
    [JsonPropertyName("fromLinkActivity")]
    public Activity FromLinkActivity
    {
      get => fromLinkActivity ??= new();
      set => fromLinkActivity = value;
    }

    /// <summary>
    /// A value of HiddenActivity.
    /// </summary>
    [JsonPropertyName("hiddenActivity")]
    public Activity HiddenActivity
    {
      get => hiddenActivity ??= new();
      set => hiddenActivity = value;
    }

    /// <summary>
    /// A value of ActivityDetail2.
    /// </summary>
    [JsonPropertyName("activityDetail2")]
    public Common ActivityDetail2
    {
      get => activityDetail2 ??= new();
      set => activityDetail2 = value;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
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

    private ActivityDetail fromLinkActivityDetail;
    private ActivityDetail activityDetail1;
    private CodeValue fromLinkCodeValue;
    private Activity fromLinkActivity;
    private Activity hiddenActivity;
    private Common activityDetail2;
    private Standard standard;
    private Activity activity;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of Protect.
      /// </summary>
      [JsonPropertyName("protect")]
      public Common Protect
      {
        get => protect ??= new();
        set => protect = value;
      }

      /// <summary>
      /// A value of AssignCode.
      /// </summary>
      [JsonPropertyName("assignCode")]
      public Common AssignCode
      {
        get => assignCode ??= new();
        set => assignCode = value;
      }

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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ActivityDistributionRule Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of ActivityDistributionRule.
      /// </summary>
      [JsonPropertyName("activityDistributionRule")]
      public ActivityDistributionRule ActivityDistributionRule
      {
        get => activityDistributionRule ??= new();
        set => activityDistributionRule = value;
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
      /// A value of BusObject.
      /// </summary>
      [JsonPropertyName("busObject")]
      public Common BusObject
      {
        get => busObject ??= new();
        set => busObject = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common protect;
      private Common assignCode;
      private Common reasonCode;
      private ActivityDistributionRule hidden;
      private ActivityDistributionRule activityDistributionRule;
      private Common cuFunction;
      private Common busObject;
      private Common common;
    }

    /// <summary>
    /// A value of ActivityDetail1.
    /// </summary>
    [JsonPropertyName("activityDetail1")]
    public ActivityDetail ActivityDetail1
    {
      get => activityDetail1 ??= new();
      set => activityDetail1 = value;
    }

    /// <summary>
    /// A value of ToList.
    /// </summary>
    [JsonPropertyName("toList")]
    public Code ToList
    {
      get => toList ??= new();
      set => toList = value;
    }

    /// <summary>
    /// A value of HiddenActivity.
    /// </summary>
    [JsonPropertyName("hiddenActivity")]
    public Activity HiddenActivity
    {
      get => hiddenActivity ??= new();
      set => hiddenActivity = value;
    }

    /// <summary>
    /// A value of ActivityDetail2.
    /// </summary>
    [JsonPropertyName("activityDetail2")]
    public Common ActivityDetail2
    {
      get => activityDetail2 ??= new();
      set => activityDetail2 = value;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
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

    private ActivityDetail activityDetail1;
    private Code toList;
    private Activity hiddenActivity;
    private Common activityDetail2;
    private Standard standard;
    private Activity activity;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Rsp.
    /// </summary>
    [JsonPropertyName("rsp")]
    public Common Rsp
    {
      get => rsp ??= new();
      set => rsp = value;
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
    /// A value of PassToValidateCodeValue.
    /// </summary>
    [JsonPropertyName("passToValidateCodeValue")]
    public CodeValue PassToValidateCodeValue
    {
      get => passToValidateCodeValue ??= new();
      set => passToValidateCodeValue = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public ActivityDistributionRule Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public ActivityDistributionRule ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    /// <summary>
    /// A value of ReturnFromValidate.
    /// </summary>
    [JsonPropertyName("returnFromValidate")]
    public Common ReturnFromValidate
    {
      get => returnFromValidate ??= new();
      set => returnFromValidate = value;
    }

    /// <summary>
    /// A value of PassToValidateCode.
    /// </summary>
    [JsonPropertyName("passToValidateCode")]
    public Code PassToValidateCode
    {
      get => passToValidateCode ??= new();
      set => passToValidateCode = value;
    }

    /// <summary>
    /// A value of PassToActivityDistributionRule.
    /// </summary>
    [JsonPropertyName("passToActivityDistributionRule")]
    public ActivityDistributionRule PassToActivityDistributionRule
    {
      get => passToActivityDistributionRule ??= new();
      set => passToActivityDistributionRule = value;
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
    /// A value of Errors.
    /// </summary>
    [JsonPropertyName("errors")]
    public Common Errors
    {
      get => errors ??= new();
      set => errors = value;
    }

    private DateWorkArea max;
    private DateWorkArea current;
    private Common rsp;
    private DateWorkArea passToDateWorkArea;
    private CodeValue passToValidateCodeValue;
    private ActivityDistributionRule initialize;
    private ActivityDistributionRule returnFrom;
    private Common returnFromValidate;
    private Code passToValidateCode;
    private ActivityDistributionRule passToActivityDistributionRule;
    private Common common;
    private NextTranInfo nextTranInfo;
    private Common prompt;
    private Common select;
    private DateWorkArea zero;
    private Common errors;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    /// <summary>
    /// A value of ActivityDistributionRule.
    /// </summary>
    [JsonPropertyName("activityDistributionRule")]
    public ActivityDistributionRule ActivityDistributionRule
    {
      get => activityDistributionRule ??= new();
      set => activityDistributionRule = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityDetail activityDetail;
    private ActivityDistributionRule activityDistributionRule;
    private Activity activity;
  }
#endregion
}

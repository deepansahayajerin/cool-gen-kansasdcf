// Program: SP_ADLM_ACTIVITY_DETAIL_LIST_MNT, ID: 371744509, model: 746.
// Short name: SWEADLMP
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
/// A program: SP_ADLM_ACTIVITY_DETAIL_LIST_MNT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAdlmActivityDetailListMnt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ADLM_ACTIVITY_DETAIL_LIST_MNT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAdlmActivityDetailListMnt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAdlmActivityDetailListMnt.
  /// </summary>
  public SpAdlmActivityDetailListMnt(IContext context, Import import,
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
    // ----- Start Of Maintenance History -------------------------
    // Date	 Developer 	Request #    Description
    // 11/04/96 Alan Samuels                Initial Development
    // 11/10/98 SWSRKEH                     Phase II Changes
    // ----- End Of Maintenance History ---------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Clear scrolling group if command=clear.
      // ---------------------------------------------
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    export.Activity2.Assign(import.Activity2);
    export.HiddenActivity.ControlNumber = import.HiddenActivity.ControlNumber;
    export.Activity1.SelectChar = import.Activity1.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // **** All valid commands for this AB is validated in the following CASE OF
    // ****
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

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.Standard, "nextTransaction");

            field1.Error = true;

            break;
          }

          return;
        }

        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "ADD":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "DELETE":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "LIST":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "UPDATE":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "RETURN":
        // ---------------------------------------------
        // Return back on a link to the calling
        // procedure.  A selection is not required, but
        // if made, only one selection is allowed.
        // ---------------------------------------------
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.BusObject.SelectChar =
            import.Group.Item.BusObject.SelectChar;
          export.Group.Update.CuFunction.SelectChar =
            import.Group.Item.CuFunction.SelectChar;
          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Update.ActivityDetail.Assign(
            import.Group.Item.ActivityDetail);
          export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;

            ++local.Select.Count;
            export.ToTranActivity.ControlNumber =
              import.Activity2.ControlNumber;
            export.ToTranActivityDetail.SystemGeneratedIdentifier =
              export.Group.Item.ActivityDetail.SystemGeneratedIdentifier;
          }

          export.Group.Next();
        }

        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_RETURN";

            break;
          case 1:
            ExitState = "ACO_NE0000_RETURN";

            break;
          default:
            ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTION";

            break;
        }

        return;
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

          export.Group.Update.BusObject.SelectChar =
            import.Group.Item.BusObject.SelectChar;
          export.Group.Update.CuFunction.SelectChar =
            import.Group.Item.CuFunction.SelectChar;
          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = false;
          }

          export.Group.Update.ActivityDetail.Assign(
            import.Group.Item.ActivityDetail);

          if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
          {
            export.Group.Update.BusObject.SelectChar = "";

            var field1 = GetField(export.Group.Item.BusObject, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.ActivityDetail.BusinessObjectCode =
                import.FromLinkCodeValue.Cdvalue;
            }
          }

          if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
          {
            export.Group.Update.CuFunction.SelectChar = "";

            var field1 = GetField(export.Group.Item.CuFunction, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            if (!IsEmpty(import.FromLinkCodeValue.Cdvalue))
            {
              export.Group.Update.ActivityDetail.CaseUnitFunction =
                import.FromLinkCodeValue.Cdvalue;
            }
          }

          export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);
          export.Group.Next();
        }

        return;
      case "RETATLM":
        export.Activity1.SelectChar = "";

        if (import.FromLinkActivity.ControlNumber > 0)
        {
          export.Activity2.ControlNumber =
            import.FromLinkActivity.ControlNumber;
        }

        var field = GetField(export.Activity2, "controlNumber");

        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;

        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

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

      if (AsChar(export.Activity1.SelectChar) == 'S')
      {
        ++local.Prompt.Count;

        var field = GetField(export.Activity1, "selectChar");

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

        export.Group.Update.BusObject.SelectChar =
          import.Group.Item.BusObject.SelectChar;
        export.Group.Update.CuFunction.SelectChar =
          import.Group.Item.CuFunction.SelectChar;
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Select.Count;

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;
        }

        export.Group.Update.ActivityDetail.Assign(
          import.Group.Item.ActivityDetail);

        if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
        {
          ++local.Prompt.Count;

          var field = GetField(export.Group.Item.BusObject, "selectChar");

          field.Error = true;
        }

        if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
        {
          ++local.Prompt.Count;

          var field = GetField(export.Group.Item.CuFunction, "selectChar");

          field.Error = true;
        }

        export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);
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
      if (import.Activity2.ControlNumber == import.HiddenActivity.ControlNumber)
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

        local.Current.Date = Now().Date;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // An add must be on a previously blank row.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.Hidden.BusinessObjectCode))
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                return;
              }

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Protected = false;

              local.PassToActivityDetail.
                Assign(export.Group.Item.ActivityDetail);

              // ---------------------------------------------
              // Perform data validation for add request.
              // ---------------------------------------------
              // ---------------------------------------------
              // Business object and regulation source are
              // required.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.ActivityDetail.RegulationSourceId))
              {
                var field1 =
                  GetField(export.Group.Item.ActivityDetail,
                  "regulationSourceId");

                field1.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else
              {
                // ---------------------------------------------
                // Regulation source code must be 'F' or 'O'.
                // ---------------------------------------------
                if (AsChar(export.Group.Item.ActivityDetail.RegulationSourceId) ==
                  'F' || AsChar
                  (export.Group.Item.ActivityDetail.RegulationSourceId) == 'O')
                {
                }
                else
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail,
                    "regulationSourceId");

                  field1.Error = true;

                  ExitState = "SP0000_INVALID_TYPE_CODE";
                }
              }

              // ---------------------------------------------
              // Default effective date is current date.
              // ---------------------------------------------
              if (Lt(local.Zero.Date,
                export.Group.Item.ActivityDetail.EffectiveDate))
              {
                if (Lt(export.Group.Item.ActivityDetail.EffectiveDate,
                  local.Current.Date))
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail, "effectiveDate");
                    

                  field1.Error = true;

                  ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";
                }
              }
              else
              {
                local.PassToActivityDetail.EffectiveDate = local.Current.Date;
                export.Group.Update.ActivityDetail.EffectiveDate =
                  local.Current.Date;
              }

              // ---------------------------------------------
              // Discontinue date, if entered, cannot
              // be < effective date.
              // ---------------------------------------------
              if (Lt(local.Zero.Date,
                export.Group.Item.ActivityDetail.DiscontinueDate))
              {
                if (Lt(export.Group.Item.ActivityDetail.DiscontinueDate,
                  export.Group.Item.ActivityDetail.EffectiveDate))
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail, "discontinueDate");
                    

                  field1.Error = true;

                  ExitState = "SP0000_INVALID_DISC_DATE";
                }
              }
              else
              {
                local.PassToActivityDetail.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }

              // ---------------------------------------------
              // If activity is manual, only one detail record
              // can be active.
              // ---------------------------------------------
              if (Equal(import.Activity2.TypeCode, "MAN"))
              {
                if (ReadActivityDetail1())
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail, "effectiveDate");
                    

                  field1.Error = true;

                  ExitState = "SP0000_MULTIPLE_ACTIVE_RECORDS";
                }
                else
                {
                  // Continue
                }
              }

              // ---------------------------------------------
              // Prompt is not valid on add.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.CuFunction.SelectChar))
              {
                var field1 =
                  GetField(export.Group.Item.CuFunction, "selectChar");

                field1.Error = true;

                ExitState = "SP0000_PROMPT_NOT_ALLOWED";
              }

              // ---------------------------------------------
              // CU function must be a valid value.  It is
              // required if business code is for a case unit.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.ActivityDetail.CaseUnitFunction))
              {
                local.PassToValidationCode.CodeName = "CASE UNIT FUNCTION";
                local.PassToValidationCodeValue.Cdvalue =
                  export.Group.Item.ActivityDetail.CaseUnitFunction ?? Spaces
                  (10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnFromValidation.Flag) == 'N')
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail,
                    "caseUnitFunction");

                  field1.Error = true;

                  ExitState = "SP0000_INVALID_TYPE_CODE";
                }
              }
              else if (Equal(export.Group.Item.ActivityDetail.
                BusinessObjectCode, "CUF"))
              {
                var field1 =
                  GetField(export.Group.Item.ActivityDetail, "caseUnitFunction");
                  

                field1.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }

              // ---------------------------------------------
              // Prompt is not valid on add.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.BusObject.SelectChar))
              {
                var field1 =
                  GetField(export.Group.Item.CuFunction, "selectChar");

                field1.Error = true;

                ExitState = "SP0000_PROMPT_NOT_ALLOWED";
              }

              // ---------------------------------------------
              // Business object code is required.  Must be
              // a valid value.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.ActivityDetail.BusinessObjectCode))
              {
                var field1 =
                  GetField(export.Group.Item.ActivityDetail,
                  "businessObjectCode");

                field1.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else
              {
                // ---------------------------------------------
                // Validate value.
                // ---------------------------------------------
                local.PassToValidationCode.CodeName = "BUSINESS OBJECT CODE";
                local.PassToValidationCodeValue.Cdvalue =
                  export.Group.Item.ActivityDetail.BusinessObjectCode ?? Spaces
                  (10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnFromValidation.Flag) == 'N')
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail,
                    "businessObjectCode");

                  field1.Error = true;

                  ExitState = "SP0000_INVALID_TYPE_CODE";
                }
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";
            }
          }
          else
          {
            continue;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_ADD_SUCCESSFUL"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Create activity
          // detail.
          // ---------------------------------------------
          UseSpCabCreateActivityDetail();

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

          export.Group.Update.ActivityDetail.Assign(local.ReturnFrom);
          export.Group.Update.Hidden.Assign(local.ReturnFrom);
        }

        break;
      case "UPDATE":
        local.Current.Date = Now().Date;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            local.PassToActivityDetail.Assign(export.Group.Item.ActivityDetail);

            // ---------------------------------------------
            // An update must be performed on a populated
            // row.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.Hidden.BusinessObjectCode))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

              return;
            }

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;

            // ---------------------------------------------
            // Perform data validation for update request.
            // ---------------------------------------------
            // ---------------------------------------------
            // A field other than business object must have
            // changed.
            // ---------------------------------------------
            if (Equal(export.Group.Item.ActivityDetail.
              RegulationSourceDescription,
              export.Group.Item.Hidden.RegulationSourceDescription) && AsChar
              (export.Group.Item.ActivityDetail.RegulationSourceId) == AsChar
              (export.Group.Item.Hidden.RegulationSourceId) && Equal
              (export.Group.Item.ActivityDetail.DiscontinueDate,
              export.Group.Item.Hidden.DiscontinueDate) && Equal
              (export.Group.Item.ActivityDetail.EffectiveDate,
              export.Group.Item.Hidden.EffectiveDate) && Equal
              (export.Group.Item.ActivityDetail.CaseUnitFunction,
              export.Group.Item.Hidden.CaseUnitFunction) && export
              .Group.Item.ActivityDetail.OtherNearNonComplDays.
                GetValueOrDefault() == export
              .Group.Item.Hidden.OtherNearNonComplDays.GetValueOrDefault() && export
              .Group.Item.ActivityDetail.OtherNonComplianceDays.
                GetValueOrDefault() == export
              .Group.Item.Hidden.OtherNonComplianceDays.GetValueOrDefault() && export
              .Group.Item.ActivityDetail.FedNearNonComplDays.
                GetValueOrDefault() == export
              .Group.Item.Hidden.FedNearNonComplDays.GetValueOrDefault() && export
              .Group.Item.ActivityDetail.FedNonComplianceDays.
                GetValueOrDefault() == export
              .Group.Item.Hidden.FedNonComplianceDays.GetValueOrDefault())
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_DATA_NOT_CHANGED";

              return;
            }

            // ---------------------------------------------
            // Regulation source is required.
            // ---------------------------------------------
            if (IsEmpty(export.Group.Item.ActivityDetail.RegulationSourceId))
            {
              var field1 =
                GetField(export.Group.Item.ActivityDetail, "regulationSourceId");
                

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              // ---------------------------------------------
              // Regulation source code must be 'F' or 'O'.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.ActivityDetail.RegulationSourceId) ==
                'F' || AsChar
                (export.Group.Item.ActivityDetail.RegulationSourceId) == 'O')
              {
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.ActivityDetail,
                  "regulationSourceId");

                field1.Error = true;

                ExitState = "SP0000_INVALID_TYPE_CODE";
              }
            }

            // ---------------------------------------------
            // Effective date cannot be changed if past current date.
            // ---------------------------------------------
            if (!Equal(export.Group.Item.ActivityDetail.EffectiveDate,
              export.Group.Item.Hidden.EffectiveDate))
            {
              if (!Lt(export.Group.Item.Hidden.EffectiveDate, local.Current.Date))
                
              {
                if (Equal(export.Group.Item.ActivityDetail.EffectiveDate,
                  local.Zero.Date))
                {
                  export.Group.Update.ActivityDetail.EffectiveDate =
                    local.Current.Date;
                  local.PassToActivityDetail.EffectiveDate = local.Current.Date;
                }
                else if (!Lt(export.Group.Item.ActivityDetail.EffectiveDate,
                  local.Current.Date))
                {
                }
                else
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail, "effectiveDate");
                    

                  field1.Error = true;

                  export.Group.Update.ActivityDetail.EffectiveDate =
                    export.Group.Item.Hidden.EffectiveDate;
                  ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";
                }
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.ActivityDetail, "effectiveDate");

                field1.Error = true;

                export.Group.Update.ActivityDetail.EffectiveDate =
                  export.Group.Item.Hidden.EffectiveDate;
                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }
            }

            // ---------------------------------------------
            // Discontinue date, if entered, cannot
            // be < effective date.
            // ---------------------------------------------
            if (Lt(local.Zero.Date,
              export.Group.Item.ActivityDetail.DiscontinueDate))
            {
              if (Lt(export.Group.Item.ActivityDetail.DiscontinueDate,
                export.Group.Item.ActivityDetail.EffectiveDate))
              {
                var field1 =
                  GetField(export.Group.Item.ActivityDetail, "discontinueDate");
                  

                field1.Error = true;

                ExitState = "SP0000_INVALID_DISC_DATE";
              }
            }
            else
            {
              local.PassToActivityDetail.DiscontinueDate =
                UseCabSetMaximumDiscontinueDate();
            }

            // ---------------------------------------------
            // If activity is manual, only one detail record
            // can be active.
            // ---------------------------------------------
            if (Equal(import.Activity2.TypeCode, "MAN"))
            {
              if (ReadActivityDetail1())
              {
                if (export.Group.Item.ActivityDetail.
                  SystemGeneratedIdentifier == entities
                  .ActivityDetail.SystemGeneratedIdentifier)
                {
                }
                else
                {
                  var field1 = GetField(export.Group.Item.Common, "selectChar");

                  field1.Error = true;

                  ExitState = "SP0000_MULTIPLE_ACTIVE_RECORDS";
                }
              }
              else
              {
                // Continue
              }
            }

            // ---------------------------------------------
            // Prompt is not valid on update.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.CuFunction.SelectChar))
            {
              var field1 = GetField(export.Group.Item.CuFunction, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            if (Equal(export.Group.Item.ActivityDetail.CaseUnitFunction,
              export.Group.Item.Hidden.CaseUnitFunction))
            {
            }
            else
            {
              // ---------------------------------------------
              // CU function must be a valid value.  It is
              // required if business code is for a case unit.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.ActivityDetail.CaseUnitFunction))
              {
                local.PassToValidationCode.CodeName = "CASE UNIT FUNCTION";
                local.PassToValidationCodeValue.Cdvalue =
                  export.Group.Item.ActivityDetail.CaseUnitFunction ?? Spaces
                  (10);
                UseCabValidateCodeValue();

                if (AsChar(local.ReturnFromValidation.Flag) == 'N')
                {
                  var field1 =
                    GetField(export.Group.Item.ActivityDetail,
                    "caseUnitFunction");

                  field1.Error = true;

                  ExitState = "SP0000_INVALID_TYPE_CODE";
                }
              }
              else if (Equal(export.Group.Item.Hidden.BusinessObjectCode, "CUF"))
                
              {
                var field1 =
                  GetField(export.Group.Item.ActivityDetail, "caseUnitFunction");
                  

                field1.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
            }

            // ---------------------------------------------
            // Prompt is not valid on update.
            // ---------------------------------------------
            if (!IsEmpty(export.Group.Item.BusObject.SelectChar))
            {
              var field1 = GetField(export.Group.Item.CuFunction, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_PROMPT_NOT_ALLOWED";
            }

            // ---------------------------------------------
            // Business object code cannot be changed.
            // ---------------------------------------------
            if (Equal(export.Group.Item.ActivityDetail.BusinessObjectCode,
              export.Group.Item.Hidden.BusinessObjectCode))
            {
            }
            else
            {
              var field1 =
                GetField(export.Group.Item.ActivityDetail, "businessObjectCode");
                

              field1.Error = true;

              export.Group.Update.ActivityDetail.BusinessObjectCode =
                export.Group.Item.Hidden.BusinessObjectCode ?? "";
              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }
          }
          else
          {
            continue;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Update Activity
          // Detail.
          // ---------------------------------------------
          UseSpCabUpdateActivityDetail();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
          }
          else
          {
            return;
          }

          export.Group.Update.ActivityDetail.Assign(local.ReturnFrom);
          export.Group.Update.Hidden.Assign(local.ReturnFrom);

          return;
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // A delete must be performed on a populated
              // row.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.Hidden.BusinessObjectCode))
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

                return;
              }

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Protected = false;

              // ---------------------------------------------
              // Prompt is not valid on delete.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.CuFunction.SelectChar))
              {
                var field1 =
                  GetField(export.Group.Item.CuFunction, "selectChar");

                field1.Error = true;

                ExitState = "SP0000_PROMPT_NOT_ALLOWED";
              }

              if (!IsEmpty(export.Group.Item.BusObject.SelectChar))
              {
                var field1 =
                  GetField(export.Group.Item.CuFunction, "selectChar");

                field1.Error = true;

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

              local.PassToActivityDetail.
                Assign(export.Group.Item.ActivityDetail);
              UseSpCabDeleteActivityDetail();

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_DELETE_SUCCESSFUL"))
              {
                ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
                export.Group.Update.ActivityDetail.Assign(local.Initialize);
                export.Group.Update.Hidden.Assign(local.Initialize);
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                return;
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";

              return;
            }
          }
        }

        break;
      case "LIST":
        switch(local.Prompt.Count)
        {
          case 0:
            ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

            return;
          case 1:
            break;
          default:
            ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S2";

            return;
        }

        if (AsChar(import.Activity1.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_ATLM";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.BusObject.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
            export.ToTranCode.CodeName = "BUSINESS OBJECT CODE";

            return;
          }

          if (AsChar(export.Group.Item.CuFunction.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
            export.ToTranCode.CodeName = "CASE UNIT FUNCTION";

            return;
          }
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadActivity())
      {
        export.Activity2.Assign(entities.Activity);
        export.HiddenActivity.ControlNumber = entities.Activity.ControlNumber;

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadActivityDetail2())
        {
          export.Group.Update.ActivityDetail.Assign(entities.ActivityDetail);
          export.Group.Update.Hidden.Assign(entities.ActivityDetail);
          local.PassToDateWorkArea.Date =
            entities.ActivityDetail.DiscontinueDate;
          export.Group.Update.ActivityDetail.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
          export.Group.Update.Hidden.DiscontinueDate =
            export.Group.Item.ActivityDetail.DiscontinueDate;
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
      else if (export.Activity2.ControlNumber > 0)
      {
        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        export.Activity1.SelectChar = "S";

        var field = GetField(export.Activity2, "controlNumber");

        field.Error = true;
      }
      else
      {
        ExitState = "ZD_ACO_NE_INVALID_CODE_PRES_PF4";
        export.Activity1.SelectChar = "S";

        var field = GetField(export.Activity2, "controlNumber");

        field.Protected = false;
        field.Focused = true;
      }
    }
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

    useImport.Code.CodeName = local.PassToValidationCode.CodeName;
    useImport.CodeValue.Cdvalue = local.PassToValidationCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnFromValidation.Flag = useExport.ValidCode.Flag;
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

  private void UseSpCabCreateActivityDetail()
  {
    var useImport = new SpCabCreateActivityDetail.Import();
    var useExport = new SpCabCreateActivityDetail.Export();

    useImport.Activity.ControlNumber = import.Activity2.ControlNumber;
    useImport.ActivityDetail.Assign(local.PassToActivityDetail);

    Call(SpCabCreateActivityDetail.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.ActivityDetail);
  }

  private void UseSpCabDeleteActivityDetail()
  {
    var useImport = new SpCabDeleteActivityDetail.Import();
    var useExport = new SpCabDeleteActivityDetail.Export();

    useImport.Activity.ControlNumber = import.Activity2.ControlNumber;
    useImport.ActivityDetail.SystemGeneratedIdentifier =
      local.PassToActivityDetail.SystemGeneratedIdentifier;

    Call(SpCabDeleteActivityDetail.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateActivityDetail()
  {
    var useImport = new SpCabUpdateActivityDetail.Import();
    var useExport = new SpCabUpdateActivityDetail.Export();

    useImport.Activity.ControlNumber = import.Activity2.ControlNumber;
    useImport.ActivityDetail.Assign(local.PassToActivityDetail);

    Call(SpCabUpdateActivityDetail.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.ActivityDetail);
  }

  private bool ReadActivity()
  {
    entities.Activity.Populated = false;

    return Read("ReadActivity",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Activity2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.Activity.TypeCode = db.GetString(reader, 2);
        entities.Activity.Populated = true;
      });
  }

  private bool ReadActivityDetail1()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.PassToActivityDetail.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          local.PassToActivityDetail.DiscontinueDate.GetValueOrDefault());
        db.SetInt32(command, "actNo", import.Activity2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.BusinessObjectCode =
          db.GetNullableString(reader, 1);
        entities.ActivityDetail.CaseUnitFunction =
          db.GetNullableString(reader, 2);
        entities.ActivityDetail.FedNonComplianceDays =
          db.GetNullableInt32(reader, 3);
        entities.ActivityDetail.FedNearNonComplDays =
          db.GetNullableInt32(reader, 4);
        entities.ActivityDetail.OtherNonComplianceDays =
          db.GetNullableInt32(reader, 5);
        entities.ActivityDetail.OtherNearNonComplDays =
          db.GetNullableInt32(reader, 6);
        entities.ActivityDetail.RegulationSourceId =
          db.GetNullableString(reader, 7);
        entities.ActivityDetail.RegulationSourceDescription =
          db.GetNullableString(reader, 8);
        entities.ActivityDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.ActivityDetail.DiscontinueDate = db.GetDate(reader, 10);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 11);
        entities.ActivityDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadActivityDetail2()
  {
    return ReadEach("ReadActivityDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", entities.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.BusinessObjectCode =
          db.GetNullableString(reader, 1);
        entities.ActivityDetail.CaseUnitFunction =
          db.GetNullableString(reader, 2);
        entities.ActivityDetail.FedNonComplianceDays =
          db.GetNullableInt32(reader, 3);
        entities.ActivityDetail.FedNearNonComplDays =
          db.GetNullableInt32(reader, 4);
        entities.ActivityDetail.OtherNonComplianceDays =
          db.GetNullableInt32(reader, 5);
        entities.ActivityDetail.OtherNearNonComplDays =
          db.GetNullableInt32(reader, 6);
        entities.ActivityDetail.RegulationSourceId =
          db.GetNullableString(reader, 7);
        entities.ActivityDetail.RegulationSourceDescription =
          db.GetNullableString(reader, 8);
        entities.ActivityDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.ActivityDetail.DiscontinueDate = db.GetDate(reader, 10);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 11);
        entities.ActivityDetail.Populated = true;

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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ActivityDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
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
      /// A value of ActivityDetail.
      /// </summary>
      [JsonPropertyName("activityDetail")]
      public ActivityDetail ActivityDetail
      {
        get => activityDetail ??= new();
        set => activityDetail = value;
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
      public const int Capacity = 75;

      private ActivityDetail hidden;
      private Common cuFunction;
      private Common busObject;
      private ActivityDetail activityDetail;
      private Common common;
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
    /// A value of Activity1.
    /// </summary>
    [JsonPropertyName("activity1")]
    public Common Activity1
    {
      get => activity1 ??= new();
      set => activity1 = value;
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
    /// A value of Activity2.
    /// </summary>
    [JsonPropertyName("activity2")]
    public Activity Activity2
    {
      get => activity2 ??= new();
      set => activity2 = value;
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

    private CodeValue fromLinkCodeValue;
    private Activity fromLinkActivity;
    private Activity hiddenActivity;
    private Common activity1;
    private Standard standard;
    private Activity activity2;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ActivityDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
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
      /// A value of ActivityDetail.
      /// </summary>
      [JsonPropertyName("activityDetail")]
      public ActivityDetail ActivityDetail
      {
        get => activityDetail ??= new();
        set => activityDetail = value;
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
      public const int Capacity = 75;

      private ActivityDetail hidden;
      private Common cuFunction;
      private Common busObject;
      private ActivityDetail activityDetail;
      private Common common;
    }

    /// <summary>
    /// A value of ToTranActivityDetail.
    /// </summary>
    [JsonPropertyName("toTranActivityDetail")]
    public ActivityDetail ToTranActivityDetail
    {
      get => toTranActivityDetail ??= new();
      set => toTranActivityDetail = value;
    }

    /// <summary>
    /// A value of ToTranActivity.
    /// </summary>
    [JsonPropertyName("toTranActivity")]
    public Activity ToTranActivity
    {
      get => toTranActivity ??= new();
      set => toTranActivity = value;
    }

    /// <summary>
    /// A value of ToTranCode.
    /// </summary>
    [JsonPropertyName("toTranCode")]
    public Code ToTranCode
    {
      get => toTranCode ??= new();
      set => toTranCode = value;
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
    /// A value of Activity1.
    /// </summary>
    [JsonPropertyName("activity1")]
    public Common Activity1
    {
      get => activity1 ??= new();
      set => activity1 = value;
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
    /// A value of Activity2.
    /// </summary>
    [JsonPropertyName("activity2")]
    public Activity Activity2
    {
      get => activity2 ??= new();
      set => activity2 = value;
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

    private ActivityDetail toTranActivityDetail;
    private Activity toTranActivity;
    private Code toTranCode;
    private Activity hiddenActivity;
    private Common activity1;
    private Standard standard;
    private Activity activity2;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of PassToDateWorkArea.
    /// </summary>
    [JsonPropertyName("passToDateWorkArea")]
    public DateWorkArea PassToDateWorkArea
    {
      get => passToDateWorkArea ??= new();
      set => passToDateWorkArea = value;
    }

    /// <summary>
    /// A value of ReturnFromValidation.
    /// </summary>
    [JsonPropertyName("returnFromValidation")]
    public Common ReturnFromValidation
    {
      get => returnFromValidation ??= new();
      set => returnFromValidation = value;
    }

    /// <summary>
    /// A value of PassToValidationCode.
    /// </summary>
    [JsonPropertyName("passToValidationCode")]
    public Code PassToValidationCode
    {
      get => passToValidationCode ??= new();
      set => passToValidationCode = value;
    }

    /// <summary>
    /// A value of PassToValidationCodeValue.
    /// </summary>
    [JsonPropertyName("passToValidationCodeValue")]
    public CodeValue PassToValidationCodeValue
    {
      get => passToValidationCodeValue ??= new();
      set => passToValidationCodeValue = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public ActivityDetail Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public ActivityDetail ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    /// <summary>
    /// A value of PassToActivityDetail.
    /// </summary>
    [JsonPropertyName("passToActivityDetail")]
    public ActivityDetail PassToActivityDetail
    {
      get => passToActivityDetail ??= new();
      set => passToActivityDetail = value;
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

    private DateWorkArea current;
    private DateWorkArea zero;
    private DateWorkArea passToDateWorkArea;
    private Common returnFromValidation;
    private Code passToValidationCode;
    private CodeValue passToValidationCodeValue;
    private ActivityDetail initialize;
    private ActivityDetail returnFrom;
    private ActivityDetail passToActivityDetail;
    private Common select;
    private NextTranInfo nextTranInfo;
    private Common prompt;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityDetail activityDetail;
    private Activity activity;
  }
#endregion
}

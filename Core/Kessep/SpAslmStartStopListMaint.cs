// Program: SP_ASLM_START_STOP_LIST_MAINT, ID: 371745050, model: 746.
// Short name: SWEASLMP
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
/// A program: SP_ASLM_START_STOP_LIST_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAslmStartStopListMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ASLM_START_STOP_LIST_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAslmStartStopListMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAslmStartStopListMaint.
  /// </summary>
  public SpAslmStartStopListMaint(IContext context, Import import, Export export)
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
    // ----- Start Of Maintenance History -------------------------
    // Date	 Developer 	Request #    Description
    // 11/05/96 Alan Samuels                Initial Development
    // 2/25/97  Siraj Konkader              Added logic to ensure
    //          Event and Activity Business Objects are compatible.
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

    export.Event1.Assign(import.Event1);
    MoveEventDetail(import.EventDetail1, export.EventDetail1);
    export.HiddenEvent.ControlNumber = import.HiddenEvent.ControlNumber;
    export.HiddenEventDetail.SystemGeneratedIdentifier =
      import.HiddenEventDetail.SystemGeneratedIdentifier;
    export.EventDetail2.SelectChar = import.EventDetail2.SelectChar;
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

          return;
        }

        break;
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
      case "RETADLM":
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.ActivityDetail2.SystemGeneratedIdentifier =
            import.Group.Item.ActivityDetail2.SystemGeneratedIdentifier;
          export.Group.Update.HiddenActivityDetail.SystemGeneratedIdentifier =
            import.Group.Item.HiddenActivityDetail.SystemGeneratedIdentifier;
          MoveActivity(import.Group.Item.Activity, export.Group.Update.Activity);
            
          MoveActivity(import.Group.Item.HiddenActivity,
            export.Group.Update.HiddenActivity);
          export.Group.Update.ActivityStartStop.Assign(
            import.Group.Item.ActivityStartStop);
          export.Group.Update.HiddenActivityStartStop.Assign(
            import.Group.Item.HiddenActivityStartStop);
          export.Group.Update.ActivityDetail1.SelectChar =
            import.Group.Item.ActivityDetail1.SelectChar;
          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;

          if (AsChar(import.Group.Item.ActivityDetail1.SelectChar) == 'S')
          {
            export.Group.Update.ActivityDetail1.SelectChar = "";

            if (import.FromLinkActivity.ControlNumber > 0)
            {
              export.Group.Update.Activity.ControlNumber =
                import.FromLinkActivity.ControlNumber;
              export.Group.Update.ActivityDetail2.SystemGeneratedIdentifier =
                import.FromLinkActivityDetail.SystemGeneratedIdentifier;
            }

            var field =
              GetField(export.Group.Item.ActivityDetail2,
              "systemGeneratedIdentifier");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;
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
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
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

        export.Group.Update.ActivityDetail2.SystemGeneratedIdentifier =
          import.Group.Item.ActivityDetail2.SystemGeneratedIdentifier;
        export.Group.Update.HiddenActivityDetail.SystemGeneratedIdentifier =
          import.Group.Item.HiddenActivityDetail.SystemGeneratedIdentifier;
        MoveActivity(import.Group.Item.Activity, export.Group.Update.Activity);
        MoveActivity(import.Group.Item.HiddenActivity,
          export.Group.Update.HiddenActivity);
        export.Group.Update.ActivityStartStop.Assign(
          import.Group.Item.ActivityStartStop);
        export.Group.Update.HiddenActivityStartStop.Assign(
          import.Group.Item.HiddenActivityStartStop);
        export.Group.Update.ActivityDetail1.SelectChar =
          import.Group.Item.ActivityDetail1.SelectChar;

        var field1 = GetField(export.Group.Item.Activity, "controlNumber");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Group.Item.ActivityDetail2,
          "systemGeneratedIdentifier");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Group.Item.ActivityStartStop, "actionCode");

        field3.Color = "cyan";
        field3.Protected = true;

        if (AsChar(export.Group.Item.ActivityDetail1.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Select.Count;
        }

        export.Group.Next();
      }
    }

    // ---------------------------------------------
    // Must display before maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
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
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        case 1:
          break;
        default:
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }

      local.Init1.Date = null;
      local.Current.Date = Now().Date;
    }

    // ---------------------------------------------
    // Prompt is only valid on PF4 List.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST"))
    {
      switch(local.Prompt.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";

          return;
        case 1:
          break;
        default:
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.ActivityDetail1.SelectChar) == 'S')
            {
              var field =
                GetField(export.Group.Item.ActivityDetail1, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "ADD":
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
              if (export.Group.Item.HiddenActivityDetail.
                SystemGeneratedIdentifier > 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                return;
              }

              // ---------------------------------------------
              // Prompt is not valid on add.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.ActivityDetail1.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Group.Item.ActivityDetail1, "selectChar");

                field.Error = true;

                ExitState = "SP0000_PROMPT_NOT_ALLOWED";

                return;
              }

              local.PassToActivityStartStop.Assign(
                export.Group.Item.ActivityStartStop);

              // ---------------------------------------------
              // Perform data validation for add request.
              // ---------------------------------------------
              // ---------------------------------------------
              // Effective date default is current date.
              // ---------------------------------------------
              if (Equal(export.Group.Item.ActivityStartStop.EffectiveDate,
                local.Init1.Date))
              {
                export.Group.Update.ActivityStartStop.EffectiveDate =
                  local.Current.Date;
                local.PassToActivityStartStop.EffectiveDate =
                  local.Current.Date;
              }
              else if (!Lt(export.Group.Item.ActivityStartStop.EffectiveDate,
                local.Current.Date))
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.ActivityStartStop, "effectiveDate");
                  

                field.Error = true;

                ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

                return;
              }

              // ---------------------------------------------
              // Discontinue date must be >= effective date.
              // Default value = 12/31/2099.
              // ---------------------------------------------
              if (Lt(local.Init1.Date,
                export.Group.Item.ActivityStartStop.DiscontinueDate))
              {
                if (Lt(export.Group.Item.ActivityStartStop.DiscontinueDate,
                  export.Group.Item.ActivityStartStop.EffectiveDate))
                {
                  var field =
                    GetField(export.Group.Item.ActivityStartStop,
                    "discontinueDate");

                  field.Error = true;

                  ExitState = "SP0000_INVALID_DISC_DATE";

                  return;
                }
              }
              else
              {
                local.PassToActivityStartStop.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }

              // ---------------------------------------------
              // Action code must be 'S' or 'E'.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.ActivityStartStop.ActionCode))
              {
                var field =
                  GetField(export.Group.Item.ActivityStartStop, "actionCode");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.ActivityStartStop.ActionCode) ==
                'S' || AsChar
                (export.Group.Item.ActivityStartStop.ActionCode) == 'E')
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.ActivityStartStop, "actionCode");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              // ---------------------------------------------
              // Validate activity and activity detail.
              // ---------------------------------------------
              if (ReadActivityActivityDetail())
              {
                // Continue
                // ------------------------------------------------------------
                // Begin field dependency edits
                // ------------------------------------------------------------
                switch(TrimEnd(export.Event1.BusinessObjectCode))
                {
                  case "CAS":
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
                  case "CAU":
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
                  case "LRF":
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LRF":
                        break;
                      default:
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
                  case "ADA":
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
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
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
                  case "LEA":
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
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
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
                  case "OAA":
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      case "LEA":
                        break;
                      case "OBL":
                        break;
                      case "OAA":
                        break;
                      case "ADA":
                        break;
                      default:
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
                  case "OBL":
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
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
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
                  case "INR":
                    if (Equal(entities.ActivityDetail.BusinessObjectCode, "INR"))
                      
                    {
                    }
                    else
                    {
                      ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                      var field1 =
                        GetField(export.Group.Item.Common, "selectChar");

                      field1.Error = true;

                      var field2 = GetField(export.Event1, "controlNumber");

                      field2.Error = true;
                    }

                    break;
                  default:
                    // ------------------------------------------------------------
                    // For all other Event Business Objects, the only 
                    // distribution
                    // must be on Case or Case Unit.
                    // ------------------------------------------------------------
                    switch(TrimEnd(entities.ActivityDetail.BusinessObjectCode))
                    {
                      case "CAS":
                        break;
                      case "CAU":
                        break;
                      default:
                        ExitState = "SP0000_EVNT_ACTVTY_DTL_NOT_COMPT";

                        var field1 =
                          GetField(export.Group.Item.Common, "selectChar");

                        field1.Error = true;

                        var field2 = GetField(export.Event1, "controlNumber");

                        field2.Error = true;

                        break;
                    }

                    break;
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
                // Set activity name.
                // ---------------------------------------------
                export.Group.Update.Activity.Name = entities.Activity.Name;
                export.Group.Update.HiddenActivity.Name =
                  entities.Activity.Name;

                // ---------------------------------------------
                // Data has passed validation. Create activity
                // start stop.
                // ---------------------------------------------
                local.PassToActivity.ControlNumber =
                  export.Group.Item.Activity.ControlNumber;
                local.PassToActivityDetail.SystemGeneratedIdentifier =
                  export.Group.Item.ActivityDetail2.SystemGeneratedIdentifier;
                UseSpCabCreateStartStop();

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

                export.Group.Update.ActivityStartStop.Assign(local.ReturnFrom);
                export.Group.Update.HiddenActivityStartStop.Assign(
                  local.ReturnFrom);
                MoveActivity(export.Group.Item.Activity,
                  export.Group.Update.HiddenActivity);
                export.Group.Update.HiddenActivityDetail.
                  SystemGeneratedIdentifier =
                    export.Group.Item.ActivityDetail2.SystemGeneratedIdentifier;
                  
              }
              else
              {
                export.Group.Update.ActivityDetail1.SelectChar = "S";

                var field1 =
                  GetField(export.Group.Item.Activity, "controlNumber");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.ActivityDetail2,
                  "systemGeneratedIdentifier");

                field2.Error = true;

                ExitState = "ZD_ACO_NE0_INVALID_CODE_PRES_PF4";
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
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // An update must be performed on a populated
              // row.
              // ---------------------------------------------
              if (export.Group.Item.HiddenActivityDetail.
                SystemGeneratedIdentifier == 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

                return;
              }

              // ---------------------------------------------
              // Prompt is not valid on update.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.ActivityDetail1.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Group.Item.ActivityDetail1, "selectChar");

                field.Error = true;

                ExitState = "SP0000_PROMPT_NOT_ALLOWED";

                return;
              }

              // ---------------------------------------------
              // Effective date or disc date must have
              // changed.
              // ---------------------------------------------
              if (Equal(export.Group.Item.ActivityStartStop.DiscontinueDate,
                export.Group.Item.HiddenActivityStartStop.DiscontinueDate) && Equal
                (export.Group.Item.ActivityStartStop.EffectiveDate,
                export.Group.Item.HiddenActivityStartStop.EffectiveDate))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DATA_NOT_CHANGED";

                return;
              }

              local.PassToActivityStartStop.Assign(
                export.Group.Item.ActivityStartStop);

              // ---------------------------------------------
              // Perform data validation for update request.
              // ---------------------------------------------
              // ---------------------------------------------
              // Discontinue date must be > effective date.
              // Default value = 12/31/2099.
              // ---------------------------------------------
              if (!Equal(export.Group.Item.ActivityStartStop.EffectiveDate,
                export.Group.Item.HiddenActivityStartStop.EffectiveDate))
              {
                if (!Lt(export.Group.Item.HiddenActivityStartStop.EffectiveDate,
                  local.Current.Date))
                {
                  if (Equal(export.Group.Item.ActivityStartStop.EffectiveDate,
                    local.Init1.Date))
                  {
                    local.PassToActivityStartStop.EffectiveDate =
                      local.Current.Date;
                    export.Group.Update.ActivityStartStop.EffectiveDate =
                      local.Current.Date;
                  }
                  else if (!Lt(export.Group.Item.ActivityStartStop.
                    EffectiveDate, local.Current.Date))
                  {
                  }
                  else
                  {
                    var field =
                      GetField(export.Group.Item.ActivityStartStop,
                      "effectiveDate");

                    field.Error = true;

                    ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

                    return;
                  }
                }
                else
                {
                  var field =
                    GetField(export.Group.Item.ActivityStartStop,
                    "effectiveDate");

                  field.Error = true;

                  export.Group.Update.ActivityStartStop.EffectiveDate =
                    export.Group.Item.HiddenActivityStartStop.EffectiveDate;
                  ExitState = "SP0000_FIELD_NOT_UPDATEABLE";

                  return;
                }
              }

              // ---------------------------------------------
              // Discontinue date must be >= effective date.
              // Default value = 12/31/2099.
              // ---------------------------------------------
              if (Lt(local.Init1.Date,
                export.Group.Item.ActivityStartStop.DiscontinueDate))
              {
                if (Lt(export.Group.Item.ActivityStartStop.DiscontinueDate,
                  export.Group.Item.ActivityStartStop.EffectiveDate))
                {
                  var field =
                    GetField(export.Group.Item.ActivityStartStop,
                    "discontinueDate");

                  field.Error = true;

                  ExitState = "SP0000_INVALID_DISC_DATE";

                  return;
                }

                if (Lt(export.Group.Item.ActivityStartStop.DiscontinueDate,
                  local.Current.Date))
                {
                  var field =
                    GetField(export.Group.Item.ActivityStartStop,
                    "discontinueDate");

                  field.Error = true;

                  ExitState = "SP0000_INVALID_DISC_DATE";

                  return;
                }
              }
              else
              {
                local.PassToActivityStartStop.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }

              // ---------------------------------------------
              // Action code cannot be changed.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.ActivityStartStop.ActionCode) == AsChar
                (export.Group.Item.HiddenActivityStartStop.ActionCode))
              {
              }
              else
              {
                export.Group.Update.ActivityStartStop.ActionCode =
                  export.Group.Item.HiddenActivityStartStop.ActionCode;

                var field =
                  GetField(export.Group.Item.ActivityStartStop, "actionCode");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              // ---------------------------------------------
              // Activity number cannot be changed.
              // ---------------------------------------------
              if (export.Group.Item.Activity.ControlNumber == export
                .Group.Item.HiddenActivity.ControlNumber)
              {
              }
              else
              {
                export.Group.Update.Activity.ControlNumber =
                  export.Group.Item.HiddenActivity.ControlNumber;

                var field =
                  GetField(export.Group.Item.Activity, "controlNumber");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              // ---------------------------------------------
              // Activity detail number cannot be changed.
              // ---------------------------------------------
              if (export.Group.Item.ActivityDetail2.SystemGeneratedIdentifier ==
                export
                .Group.Item.HiddenActivityDetail.SystemGeneratedIdentifier)
              {
              }
              else
              {
                export.Group.Update.ActivityDetail2.SystemGeneratedIdentifier =
                  export.Group.Item.HiddenActivityDetail.
                    SystemGeneratedIdentifier;

                var field =
                  GetField(export.Group.Item.Activity, "controlNumber");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
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
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Update Activity
          // Start Stop.
          // ---------------------------------------------
          local.PassToActivity.ControlNumber =
            export.Group.Item.Activity.ControlNumber;
          local.PassToActivityDetail.SystemGeneratedIdentifier =
            export.Group.Item.ActivityDetail2.SystemGeneratedIdentifier;
          UseSpCabUpdateStartStop();

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

          export.Group.Update.ActivityStartStop.Assign(local.ReturnFrom);
          export.Group.Update.HiddenActivityStartStop.Assign(local.ReturnFrom);
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
              if (export.Group.Item.HiddenActivityDetail.
                SystemGeneratedIdentifier == 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

                return;
              }

              local.PassToActivityStartStop.ActionCode =
                export.Group.Item.ActivityStartStop.ActionCode;
              local.PassToActivity.ControlNumber =
                export.Group.Item.Activity.ControlNumber;
              local.PassToActivityDetail.SystemGeneratedIdentifier =
                export.Group.Item.ActivityDetail2.SystemGeneratedIdentifier;
              UseSpCabDeleteStartStop();

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_DELETE_SUCCESSFUL"))
              {
                ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
                MoveActivity(local.InitializeActivity,
                  export.Group.Update.Activity);
                MoveActivity(local.InitializeActivity,
                  export.Group.Update.HiddenActivity);
                export.Group.Update.ActivityStartStop.Assign(
                  local.InitializeActivityStartStop);
                export.Group.Update.HiddenActivityStartStop.Assign(
                  local.InitializeActivityStartStop);
                export.Group.Update.ActivityDetail2.SystemGeneratedIdentifier =
                  0;
                export.Group.Update.HiddenActivityDetail.
                  SystemGeneratedIdentifier = 0;
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
        if (!IsEmpty(import.EventDetail2.SelectChar))
        {
          if (AsChar(import.EventDetail2.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_EDLM";
            ++local.Common.Count;
          }
          else
          {
            var field = GetField(export.EventDetail2, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";

            return;
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.ActivityDetail1.SelectChar))
          {
            if (AsChar(export.Group.Item.ActivityDetail1.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_ADLM";
              ++local.Common.Count;
            }
            else
            {
              var field =
                GetField(export.Group.Item.ActivityDetail1, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_PROMPT_VAL";

              return;
            }
          }
        }

        if (local.Common.Count == 0)
        {
          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";
        }
        else if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (export.Event1.ControlNumber > 0 || AsChar
        (export.EventDetail2.SelectChar) == 'S')
      {
      }
      else
      {
        var field = GetField(export.Event1, "controlNumber");

        field.Protected = false;
        field.Focused = true;

        ExitState = "ZD_ACO_NE_INVALID_CODE_PRES_PF4";

        return;
      }

      if (ReadEventEventDetail())
      {
        export.Event1.Assign(entities.Event1);
        export.HiddenEvent.ControlNumber = entities.Event1.ControlNumber;
        MoveEventDetail(entities.EventDetail, export.EventDetail1);
        export.HiddenEventDetail.SystemGeneratedIdentifier =
          entities.EventDetail.SystemGeneratedIdentifier;

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadActivityStartStopActivityDetailActivity())
        {
          MoveActivity(entities.Activity, export.Group.Update.Activity);

          var field1 = GetField(export.Group.Item.Activity, "controlNumber");

          field1.Color = "cyan";
          field1.Protected = true;

          MoveActivity(entities.Activity, export.Group.Update.HiddenActivity);
          export.Group.Update.ActivityDetail2.SystemGeneratedIdentifier =
            entities.ActivityDetail.SystemGeneratedIdentifier;

          var field2 =
            GetField(export.Group.Item.ActivityDetail2,
            "systemGeneratedIdentifier");

          field2.Color = "cyan";
          field2.Protected = true;

          export.Group.Update.HiddenActivityDetail.SystemGeneratedIdentifier =
            entities.ActivityDetail.SystemGeneratedIdentifier;
          export.Group.Update.ActivityStartStop.Assign(
            entities.ActivityStartStop);

          var field3 =
            GetField(export.Group.Item.ActivityStartStop, "actionCode");

          field3.Color = "cyan";
          field3.Protected = true;

          export.Group.Update.HiddenActivityStartStop.Assign(
            entities.ActivityStartStop);
          local.PassToDateWorkArea.Date =
            entities.ActivityStartStop.DiscontinueDate;
          export.Group.Update.ActivityStartStop.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
          export.Group.Update.HiddenActivityStartStop.DiscontinueDate =
            export.Group.Item.ActivityStartStop.DiscontinueDate;
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
        export.EventDetail2.SelectChar = "S";
      }
      else
      {
        var field = GetField(export.Event1, "controlNumber");

        field.Protected = false;
        field.Focused = true;

        ExitState = "ZD_ACO_NE_INVALID_CODE_PRES_PF4";
      }
    }
  }

  private static void MoveActivity(Activity source, Activity target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Name = source.Name;
  }

  private static void MoveEventDetail(EventDetail source, EventDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DetailName = source.DetailName;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.PassToDateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private void UseSpCabCreateStartStop()
  {
    var useImport = new SpCabCreateStartStop.Import();
    var useExport = new SpCabCreateStartStop.Export();

    useImport.EventDetail.SystemGeneratedIdentifier =
      import.EventDetail1.SystemGeneratedIdentifier;
    useImport.Event1.ControlNumber = import.Event1.ControlNumber;
    useImport.Activity.ControlNumber = local.PassToActivity.ControlNumber;
    useImport.ActivityDetail.SystemGeneratedIdentifier =
      local.PassToActivityDetail.SystemGeneratedIdentifier;
    useImport.ActivityStartStop.Assign(local.PassToActivityStartStop);

    Call(SpCabCreateStartStop.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.ActivityStartStop);
  }

  private void UseSpCabDeleteStartStop()
  {
    var useImport = new SpCabDeleteStartStop.Import();
    var useExport = new SpCabDeleteStartStop.Export();

    useImport.EventDetail.SystemGeneratedIdentifier =
      import.EventDetail1.SystemGeneratedIdentifier;
    useImport.Event1.ControlNumber = import.Event1.ControlNumber;
    useImport.Activity.ControlNumber = local.PassToActivity.ControlNumber;
    useImport.ActivityDetail.SystemGeneratedIdentifier =
      local.PassToActivityDetail.SystemGeneratedIdentifier;
    useImport.ActivityStartStop.Assign(local.PassToActivityStartStop);

    Call(SpCabDeleteStartStop.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateStartStop()
  {
    var useImport = new SpCabUpdateStartStop.Import();
    var useExport = new SpCabUpdateStartStop.Export();

    useImport.EventDetail.SystemGeneratedIdentifier =
      import.EventDetail1.SystemGeneratedIdentifier;
    useImport.Event1.ControlNumber = import.Event1.ControlNumber;
    useImport.Activity.ControlNumber = local.PassToActivity.ControlNumber;
    useImport.ActivityDetail.SystemGeneratedIdentifier =
      local.PassToActivityDetail.SystemGeneratedIdentifier;
    useImport.ActivityStartStop.Assign(local.PassToActivityStartStop);

    Call(SpCabUpdateStartStop.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.ActivityStartStop);
  }

  private bool ReadActivityActivityDetail()
  {
    entities.ActivityDetail.Populated = false;
    entities.Activity.Populated = false;

    return Read("ReadActivityActivityDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "controlNumber", export.Group.Item.Activity.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          export.Group.Item.ActivityDetail2.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ActivityDetail.BusinessObjectCode =
          db.GetNullableString(reader, 3);
        entities.ActivityDetail.Populated = true;
        entities.Activity.Populated = true;
      });
  }

  private IEnumerable<bool> ReadActivityStartStopActivityDetailActivity()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);

    return ReadEach("ReadActivityStartStopActivityDetailActivity",
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

        entities.ActivityStartStop.ActionCode = db.GetString(reader, 0);
        entities.ActivityStartStop.EffectiveDate = db.GetDate(reader, 1);
        entities.ActivityStartStop.DiscontinueDate = db.GetDate(reader, 2);
        entities.ActivityStartStop.ActNo = db.GetInt32(reader, 3);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 3);
        entities.Activity.ControlNumber = db.GetInt32(reader, 3);
        entities.ActivityStartStop.AcdId = db.GetInt32(reader, 4);
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ActivityStartStop.EveNo = db.GetInt32(reader, 5);
        entities.ActivityStartStop.EvdId = db.GetInt32(reader, 6);
        entities.ActivityDetail.BusinessObjectCode =
          db.GetNullableString(reader, 7);
        entities.Activity.Name = db.GetString(reader, 8);
        entities.ActivityStartStop.Populated = true;
        entities.ActivityDetail.Populated = true;
        entities.Activity.Populated = true;

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
      /// A value of HiddenActivityStartStop.
      /// </summary>
      [JsonPropertyName("hiddenActivityStartStop")]
      public ActivityStartStop HiddenActivityStartStop
      {
        get => hiddenActivityStartStop ??= new();
        set => hiddenActivityStartStop = value;
      }

      /// <summary>
      /// A value of ActivityStartStop.
      /// </summary>
      [JsonPropertyName("activityStartStop")]
      public ActivityStartStop ActivityStartStop
      {
        get => activityStartStop ??= new();
        set => activityStartStop = value;
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
      /// A value of Activity.
      /// </summary>
      [JsonPropertyName("activity")]
      public Activity Activity
      {
        get => activity ??= new();
        set => activity = value;
      }

      /// <summary>
      /// A value of HiddenActivityDetail.
      /// </summary>
      [JsonPropertyName("hiddenActivityDetail")]
      public ActivityDetail HiddenActivityDetail
      {
        get => hiddenActivityDetail ??= new();
        set => hiddenActivityDetail = value;
      }

      /// <summary>
      /// A value of ActivityDetail1.
      /// </summary>
      [JsonPropertyName("activityDetail1")]
      public Common ActivityDetail1
      {
        get => activityDetail1 ??= new();
        set => activityDetail1 = value;
      }

      /// <summary>
      /// A value of ActivityDetail2.
      /// </summary>
      [JsonPropertyName("activityDetail2")]
      public ActivityDetail ActivityDetail2
      {
        get => activityDetail2 ??= new();
        set => activityDetail2 = value;
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

      private ActivityStartStop hiddenActivityStartStop;
      private ActivityStartStop activityStartStop;
      private Activity hiddenActivity;
      private Activity activity;
      private ActivityDetail hiddenActivityDetail;
      private Common activityDetail1;
      private ActivityDetail activityDetail2;
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
    /// A value of FromLinkActivity.
    /// </summary>
    [JsonPropertyName("fromLinkActivity")]
    public Activity FromLinkActivity
    {
      get => fromLinkActivity ??= new();
      set => fromLinkActivity = value;
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

    private ActivityDetail fromLinkActivityDetail;
    private Activity fromLinkActivity;
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
      /// A value of HiddenActivityStartStop.
      /// </summary>
      [JsonPropertyName("hiddenActivityStartStop")]
      public ActivityStartStop HiddenActivityStartStop
      {
        get => hiddenActivityStartStop ??= new();
        set => hiddenActivityStartStop = value;
      }

      /// <summary>
      /// A value of ActivityStartStop.
      /// </summary>
      [JsonPropertyName("activityStartStop")]
      public ActivityStartStop ActivityStartStop
      {
        get => activityStartStop ??= new();
        set => activityStartStop = value;
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
      /// A value of Activity.
      /// </summary>
      [JsonPropertyName("activity")]
      public Activity Activity
      {
        get => activity ??= new();
        set => activity = value;
      }

      /// <summary>
      /// A value of HiddenActivityDetail.
      /// </summary>
      [JsonPropertyName("hiddenActivityDetail")]
      public ActivityDetail HiddenActivityDetail
      {
        get => hiddenActivityDetail ??= new();
        set => hiddenActivityDetail = value;
      }

      /// <summary>
      /// A value of ActivityDetail1.
      /// </summary>
      [JsonPropertyName("activityDetail1")]
      public Common ActivityDetail1
      {
        get => activityDetail1 ??= new();
        set => activityDetail1 = value;
      }

      /// <summary>
      /// A value of ActivityDetail2.
      /// </summary>
      [JsonPropertyName("activityDetail2")]
      public ActivityDetail ActivityDetail2
      {
        get => activityDetail2 ??= new();
        set => activityDetail2 = value;
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

      private ActivityStartStop hiddenActivityStartStop;
      private ActivityStartStop activityStartStop;
      private Activity hiddenActivity;
      private Activity activity;
      private ActivityDetail hiddenActivityDetail;
      private Common activityDetail1;
      private ActivityDetail activityDetail2;
      private Common common;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
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
    /// A value of PassToDateWorkArea.
    /// </summary>
    [JsonPropertyName("passToDateWorkArea")]
    public DateWorkArea PassToDateWorkArea
    {
      get => passToDateWorkArea ??= new();
      set => passToDateWorkArea = value;
    }

    /// <summary>
    /// A value of InitializeActivityStartStop.
    /// </summary>
    [JsonPropertyName("initializeActivityStartStop")]
    public ActivityStartStop InitializeActivityStartStop
    {
      get => initializeActivityStartStop ??= new();
      set => initializeActivityStartStop = value;
    }

    /// <summary>
    /// A value of InitializeActivity.
    /// </summary>
    [JsonPropertyName("initializeActivity")]
    public Activity InitializeActivity
    {
      get => initializeActivity ??= new();
      set => initializeActivity = value;
    }

    /// <summary>
    /// A value of PassToActivity.
    /// </summary>
    [JsonPropertyName("passToActivity")]
    public Activity PassToActivity
    {
      get => passToActivity ??= new();
      set => passToActivity = value;
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
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public ActivityStartStop ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    /// <summary>
    /// A value of PassToActivityStartStop.
    /// </summary>
    [JsonPropertyName("passToActivityStartStop")]
    public ActivityStartStop PassToActivityStartStop
    {
      get => passToActivityStartStop ??= new();
      set => passToActivityStartStop = value;
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

    private DateWorkArea current;
    private DateWorkArea init1;
    private Common select;
    private Common prompt;
    private DateWorkArea passToDateWorkArea;
    private ActivityStartStop initializeActivityStartStop;
    private Activity initializeActivity;
    private Activity passToActivity;
    private ActivityDetail passToActivityDetail;
    private ActivityStartStop returnFrom;
    private ActivityStartStop passToActivityStartStop;
    private Common common;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    /// <summary>
    /// A value of ActivityStartStop.
    /// </summary>
    [JsonPropertyName("activityStartStop")]
    public ActivityStartStop ActivityStartStop
    {
      get => activityStartStop ??= new();
      set => activityStartStop = value;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private EventDetail eventDetail;
    private Event1 event1;
    private ActivityStartStop activityStartStop;
    private ActivityDetail activityDetail;
    private Activity activity;
  }
#endregion
}

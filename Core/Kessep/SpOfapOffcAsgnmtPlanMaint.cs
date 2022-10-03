// Program: SP_OFAP_OFFC_ASGNMT_PLAN_MAINT, ID: 372330979, model: 746.
// Short name: SWEOFAPP
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
/// A program: SP_OFAP_OFFC_ASGNMT_PLAN_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpOfapOffcAsgnmtPlanMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_OFAP_OFFC_ASGNMT_PLAN_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpOfapOffcAsgnmtPlanMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpOfapOffcAsgnmtPlanMaint.
  /// </summary>
  public SpOfapOffcAsgnmtPlanMaint(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       request #    Description
    // 08/29/95 Alan Hackler              RETRO FITS
    // 12/24/96 Jack Rookard  Refit for new security and modify to work within 
    // new Service Plan architecture.
    // 01/06/97 Jack Rookard  Continue refit and modify for new Service Plan 
    // architecture.
    // 01/15/97 Jack Rookard  Continue refit and modify for new Service Plan 
    // architecture.
    // 04/10/97 Jack Rookard  Add dialog flow to CDVA to list TWO code values 
    // for Assignment type attribute.
    // 04/30/97 Jack Rookard  Current Date retrofit to Local_Current 
    // Date_Work_Area date.
    // 08/16/13 GVandy        CQ38147 Change assignment by county to assignment 
    // by tribunal.
    // 07/31/18 R Mathews     CQ60203 Allow referral
    // assignment by alpha and/or program
    // 
    // ---------------------------------------------
    // ---------------------------------------------------------------------------------------------------------
    // 04/23/03 Bonnie Lee - PR 173689 - Changed
    // the read office_address to not look for only a type M
    // 
    // but just get the first address and use it.  Only gets city from
    // office_address.
    // ---------------------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------------
    // 10/10/03 Bonnie Lee - PR 187969 -  Added the
    // command of display to validate action level security.
    // ---------------------------------------------------------------------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.Office.Assign(import.Office);
    export.HiddenOffice.SystemGeneratedId =
      import.HiddenOffice.SystemGeneratedId;
    export.ListOffice.Flag = import.ListOffice.Flag;

    if (AsChar(export.ListOffice.Flag) == '*')
    {
      export.ListOffice.Flag = "";
    }

    export.OfficeAddress.City = import.OfficeAddress.City;
    MoveCodeValue(import.CodeValue, export.CodeValue);
    export.PrevCodeValue.Description = import.PrevCodeValue.Description;
    export.PrevOffice.Assign(import.PrevOffice);
    export.PrevOfficeAddress.City = import.PrevOfficeAddress.City;
    export.Search.AssignmentType = import.Search.AssignmentType;
    export.Code.CodeName = import.Code.CodeName;
    export.ListCodeValue.Flag = import.ListCodeValue.Flag;

    if (AsChar(export.ListCodeValue.Flag) == '*')
    {
      export.ListCodeValue.Flag = "";
    }

    MoveCodeValue(import.HiddenCodeValue, export.HiddenCodeValue);

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;
        export.Export1.Update.OfficeAssignmentPlan.Assign(
          import.Import1.Item.OfficeAssignmentPlan);
        export.Export1.Update.PrevCommon.SelectChar =
          import.Import1.Item.PrevCommon.SelectChar;
        export.Export1.Update.PrevOfficeAssignmentPlan.Assign(
          import.Import1.Item.PrevOfficeAssignmentPlan);

        if (Equal(export.Export1.Item.OfficeAssignmentPlan.CreatedTstamp,
          local.Null1.Timestamp))
        {
          export.Export1.Next();

          continue;
        }

        var field1 =
          GetField(export.Export1.Item.OfficeAssignmentPlan, "assignmentType");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Export1.Item.OfficeAssignmentPlan, "effectiveDate");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Export1.Item.OfficeAssignmentPlan,
          "alphaAssignmentInd");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Export1.Item.OfficeAssignmentPlan, "tribunalInd");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.OfficeAssignmentPlan,
          "functionAssignmentInd");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 =
          GetField(export.Export1.Item.OfficeAssignmentPlan,
          "programAssignmentInd");

        field6.Color = "cyan";
        field6.Protected = true;

        if (Lt(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
          local.Current.Date))
        {
          if (Equal(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
            local.Null1.Date))
          {
            export.Export1.Next();

            continue;
          }

          var field =
            GetField(export.Export1.Item.OfficeAssignmentPlan, "discontinueDate");
            

          field.Color = "cyan";
          field.Protected = true;
        }

        export.Export1.Next();
      }
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // **** Validate all Commands ****
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        break;
      case "DELETE":
        break;
      case "DISPLAY":
        break;
      case "ENTER":
        // if the next tran info is not equal to spaces, this implies the user 
        // requested a next tran action. now validate
        if (IsEmpty(import.Standard.NextTransaction))
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
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

            break;
          }

          return;
        }

        break;
      case "LIST":
        if (AsChar(export.ListOffice.Flag) != 'S' && AsChar
          (export.ListOffice.Flag) != '+' && !IsEmpty(export.ListOffice.Flag))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListOffice, "flag");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListCodeValue.Flag) != 'S' && AsChar
          (export.ListCodeValue.Flag) != '+' && !
          IsEmpty(export.ListCodeValue.Flag))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListCodeValue, "flag");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListCodeValue.Flag) == 'S' && AsChar
          (export.ListOffice.Flag) == 'S')
        {
          var field3 = GetField(export.ListCodeValue, "flag");

          field3.Error = true;

          var field4 = GetField(export.ListOffice, "flag");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.ListOffice.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_OFFICE";

          return;
        }

        if (AsChar(export.ListCodeValue.Flag) == 'S')
        {
          export.Code.CodeName = "ASSIGNMENT TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RETOFCL":
        export.ListOffice.Flag = "";

        var field1 = GetField(export.ListOffice, "flag");

        field1.Protected = false;
        field1.Focused = true;

        if (import.Office.SystemGeneratedId == 0)
        {
          // In case the user returned w/o selecting a value.
          if (export.HiddenOffice.SystemGeneratedId == 0)
          {
            return;
          }

          export.Office.SystemGeneratedId =
            export.HiddenOffice.SystemGeneratedId;
        }
        else
        {
        }

        global.Command = "DISPLAY";

        break;
      case "RETCDVL":
        export.ListCodeValue.Flag = "";

        var field2 = GetField(export.ListCodeValue, "flag");

        field2.Protected = false;
        field2.Focused = true;

        if (IsEmpty(export.CodeValue.Cdvalue))
        {
          // User returned without selecting a value
          MoveCodeValue(export.HiddenCodeValue, export.CodeValue);
        }
        else
        {
          export.Search.AssignmentType = export.CodeValue.Cdvalue;
        }

        global.Command = "DISPLAY";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "UPDATE":
        break;
      case "XXFMMENU":
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        return;
      case "XXNEXTXX":
        // this is where you set your export value to the export hidden next 
        // tran values if the user is coming into this procedure on a next tran
        // action.
        UseScCabNextTranGet();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------------------------
    // 10/10/03 Bonnie Lee - PR 187969 -
    // Added the command of display to validate action level security.
    // -------------------------------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // During Add/Update/Delete
    // 1. Key (Eff Date, Assignment Type, and relationship to Office) cannot be 
    // changed
    // 2. A line must be selected
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "ADD"))
    {
      if (export.Office.SystemGeneratedId != export
        .PrevOffice.SystemGeneratedId && !
        Equal(export.Search.AssignmentType, export.PrevSearch.AssignmentType))
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      // Check if any selections made.
      local.SelCount.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.Common.SelectChar))
        {
          case ' ':
            break;
          case '*':
            export.Export1.Update.Common.SelectChar = "";

            break;
          case 'S':
            local.SelCount.Count = (int)((long)local.SelCount.Count + 1);

            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            break;
          default:
            export.Export1.Update.Common.SelectChar = "";

            break;
        }
      }

      switch(local.SelCount.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        case 1:
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            switch(AsChar(export.Export1.Item.Common.SelectChar))
            {
              case ' ':
                break;
              case '*':
                if (Equal(global.Command, "DISPLAY"))
                {
                  export.Export1.Update.Common.SelectChar = "";
                }

                break;
              case 'S':
                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Protected = false;

                break;
              default:
                var field2 = GetField(export.Export1.Item.Common, "selectChar");

                field2.Error = true;

                ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

                break;
            }
          }

          break;
        default:
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
      }
    }

    local.Common.Count = 0;

    switch(TrimEnd(global.Command))
    {
      case "ADD":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case '*':
              break;
            case 'S':
              if (!Equal(export.Export1.Item.OfficeAssignmentPlan.CreatedTstamp,
                local.Null1.Timestamp))
              {
                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                goto Test;
              }

              var field1 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "assignmentType");

              field1.Color = "";
              field1.Protected = false;

              var field2 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "effectiveDate");

              field2.Color = "";
              field2.Protected = false;

              var field3 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "alphaAssignmentInd");

              field3.Color = "";
              field3.Protected = false;

              var field4 =
                GetField(export.Export1.Item.OfficeAssignmentPlan, "tribunalInd");
                

              field4.Protected = false;

              var field5 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "functionAssignmentInd");

              field5.Color = "";
              field5.Protected = false;

              var field6 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "programAssignmentInd");

              field6.Color = "";
              field6.Protected = false;

              var field7 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "effectiveDate");

              field7.Protected = false;

              var field8 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "discontinueDate");

              field8.Protected = false;

              if (IsEmpty(export.Export1.Item.OfficeAssignmentPlan.
                AssignmentType))
              {
                var field =
                  GetField(export.Export1.Item.OfficeAssignmentPlan,
                  "assignmentType");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
                ++local.Common.Count;
              }
              else
              {
                export.Code.CodeName = "ASSIGNMENT TYPE";
                export.CodeValue.Cdvalue =
                  export.Export1.Item.OfficeAssignmentPlan.AssignmentType;
                UseCabValidateCodeValue();

                if (AsChar(local.ValidCodeValue.Flag) == 'Y')
                {
                  ExitState = "ACO_NN0000_ALL_OK";
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "assignmentType");

                  field.Error = true;

                  ExitState = "SP0000_INVALID_TYPE_CODE";
                  ++local.Common.Count;
                }
              }

              if (Equal(export.Export1.Item.OfficeAssignmentPlan.AssignmentType,
                "CA"))
              {
                // The Office Assignment Plan occurrence is a plan for CASE 
                // assignments.
                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.
                  AlphaAssignmentInd) != 'Y' && AsChar
                  (export.Export1.Item.OfficeAssignmentPlan.AlphaAssignmentInd) !=
                    'N')
                {
                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "alphaAssignmentInd");

                  field.Error = true;

                  ++local.Common.Count;
                }

                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.TribunalInd) !=
                  'Y' && AsChar
                  (export.Export1.Item.OfficeAssignmentPlan.TribunalInd) != 'N'
                  )
                {
                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "tribunalInd");

                  field.Error = true;

                  ++local.Common.Count;
                }

                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.
                  FunctionAssignmentInd) != 'Y' && AsChar
                  (export.Export1.Item.OfficeAssignmentPlan.
                    FunctionAssignmentInd) != 'N')
                {
                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "functionAssignmentInd");

                  field.Error = true;

                  ++local.Common.Count;
                }

                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.
                  ProgramAssignmentInd) != 'Y' && AsChar
                  (export.Export1.Item.OfficeAssignmentPlan.ProgramAssignmentInd)
                  != 'N')
                {
                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "programAssignmentInd");

                  field.Error = true;

                  ++local.Common.Count;
                }
              }

              if (Equal(export.Export1.Item.OfficeAssignmentPlan.AssignmentType,
                "RE"))
              {
                // The Office Assignment Plan occurrence is a PA Referral 
                // assignment plan.
                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.
                  AlphaAssignmentInd) != 'Y' && AsChar
                  (export.Export1.Item.OfficeAssignmentPlan.AlphaAssignmentInd) !=
                    'N')
                {
                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  // CQ60203 Indicator can be "Y" or "N" since this is a PA 
                  // Referral assignment plan occurrence
                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "alphaAssignmentInd");

                  field.Error = true;

                  ++local.Common.Count;
                }

                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.TribunalInd) !=
                  'N' && !
                  IsEmpty(export.Export1.Item.OfficeAssignmentPlan.TribunalInd))
                {
                  ExitState = "CO0000_INVALID_IND_MUST_BE_N";

                  // Indicator must be "N" since this is a PA Referral 
                  // assignment plan occurrence.
                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "tribunalInd");

                  field.Error = true;

                  ++local.Common.Count;
                }

                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.
                  FunctionAssignmentInd) != 'N' && !
                  IsEmpty(export.Export1.Item.OfficeAssignmentPlan.
                    FunctionAssignmentInd))
                {
                  ExitState = "CO0000_INVALID_IND_MUST_BE_N";

                  // Indicator must be "N" since this is a PA Referral 
                  // assignment plan occurrence.
                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "functionAssignmentInd");

                  field.Error = true;

                  ++local.Common.Count;
                }

                if (AsChar(export.Export1.Item.OfficeAssignmentPlan.
                  ProgramAssignmentInd) != 'Y' && AsChar
                  (export.Export1.Item.OfficeAssignmentPlan.ProgramAssignmentInd)
                  != 'N')
                {
                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  // CQ60203 Indicator can be "Y" or "N" since this is a PA 
                  // Referral assignment plan occurrence
                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "programAssignmentInd");

                  field.Error = true;

                  ++local.Common.Count;
                }
              }

              if (AsChar(export.Export1.Item.OfficeAssignmentPlan.
                ProgramAssignmentInd) == 'N' && AsChar
                (export.Export1.Item.OfficeAssignmentPlan.FunctionAssignmentInd) ==
                  'N' && AsChar
                (export.Export1.Item.OfficeAssignmentPlan.TribunalInd) == 'N'
                && AsChar
                (export.Export1.Item.OfficeAssignmentPlan.AlphaAssignmentInd) ==
                  'N')
              {
                var field9 =
                  GetField(export.Export1.Item.OfficeAssignmentPlan,
                  "alphaAssignmentInd");

                field9.Error = true;

                var field10 =
                  GetField(export.Export1.Item.OfficeAssignmentPlan,
                  "tribunalInd");

                field10.Error = true;

                var field11 =
                  GetField(export.Export1.Item.OfficeAssignmentPlan,
                  "functionAssignmentInd");

                field11.Error = true;

                var field12 =
                  GetField(export.Export1.Item.OfficeAssignmentPlan,
                  "programAssignmentInd");

                field12.Error = true;

                ++local.Common.Count;
                ExitState = "NO_INDICATOR_MAKED";
              }

              // End Date cannot be less than Current date.
              if (!Equal(export.Export1.Item.OfficeAssignmentPlan.
                DiscontinueDate, local.Null1.Date) && Lt
                (export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
                local.Current.Date))
              {
                ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

                var field =
                  GetField(export.Export1.Item.OfficeAssignmentPlan,
                  "discontinueDate");

                field.Error = true;

                ++local.Common.Count;
              }

              // Effective Date defaults to current date on an add.
              if (Equal(export.Export1.Item.OfficeAssignmentPlan.EffectiveDate,
                local.Null1.Date))
              {
                export.Export1.Update.OfficeAssignmentPlan.EffectiveDate =
                  local.Current.Date;
              }
              else
              {
                if (Lt(export.Export1.Item.OfficeAssignmentPlan.EffectiveDate,
                  local.Current.Date))
                {
                  ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "effectiveDate");

                  field.Error = true;

                  ++local.Common.Count;
                }

                if (Lt(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
                  export.Export1.Item.OfficeAssignmentPlan.EffectiveDate) && !
                  Equal(export.Export1.Item.OfficeAssignmentPlan.
                    DiscontinueDate, local.Null1.Date))
                {
                  ExitState = "DISC_DATE_CANNOT_LT_EFF_DATE";

                  var field =
                    GetField(export.Export1.Item.OfficeAssignmentPlan,
                    "discontinueDate");

                  field.Error = true;

                  ++local.Common.Count;
                }
              }

              switch(local.Common.Count)
              {
                case 0:
                  if (Equal(export.Export1.Item.OfficeAssignmentPlan.
                    DiscontinueDate, local.Null1.Date))
                  {
                    export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
                      UseCabSetMaximumDiscontinueDate1();
                  }

                  UseSpCreateOfficeAsgnmntScheme();

                  if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                    ("ACO_NI0000_SUCCESSFUL_ADD"))
                  {
                    export.Export1.Update.Common.SelectChar = "*";
                    ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
                    global.Command = "DISPLAY";
                  }
                  else
                  {
                    var field =
                      GetField(export.Export1.Item.Common, "selectChar");

                    field.Error = true;
                  }

                  if (Equal(export.Export1.Item.OfficeAssignmentPlan.
                    DiscontinueDate, local.Null1.Date))
                  {
                    break;
                  }

                  // Reset Discontinue dt
                  local.MaxDt.Date =
                    export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate;
                  export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
                    UseCabSetMaximumDiscontinueDate2();

                  break;
                case 1:
                  break;
                default:
                  ExitState = "FN0000_MULTIPLE_ERRORS_FOUND";

                  break;
              }

              break;
            default:
              break;
          }
        }

        break;
      case "DELETE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.PrevOfficeAssignmentPlan.
              AlphaAssignmentInd))
            {
              ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";

              return;
            }

            if (!Equal(export.Export1.Item.OfficeAssignmentPlan.EffectiveDate,
              export.Export1.Item.PrevOfficeAssignmentPlan.EffectiveDate))
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

              return;
            }

            UseSpDeleteOfficeAsgnmntScheme();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
              export.Export1.Update.Common.SelectChar = "*";
              ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
            }
            else
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;
            }
          }
        }

        break;
      case "UPDATE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            // During U/D
            // 1. Blank line cannot be Selected for U/D
            // 2. Only the Discontinue Date can be changed. It has to be =
            //     to current date of date in future - can not be in the past
            if (Equal(export.Export1.Item.OfficeAssignmentPlan.CreatedTstamp,
              local.Null1.Timestamp))
            {
              ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";

              return;
            }

            if (!Equal(export.Export1.Item.OfficeAssignmentPlan.EffectiveDate,
              export.Export1.Item.PrevOfficeAssignmentPlan.EffectiveDate))
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

              return;
            }

            // Validations...
            if (Equal(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
              local.Null1.Date))
            {
              export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
                local.Current.Date;
            }

            if (Lt(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
              local.Current.Date))
            {
              var field =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "discontinueDate");

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              return;
            }

            if (Lt(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
              export.Export1.Item.OfficeAssignmentPlan.EffectiveDate))
            {
              var field =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "discontinueDate");

              field.Error = true;

              ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

              return;
            }

            UseSpUpdateOfficeAsgnmntScheme();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
              export.Export1.Update.Common.SelectChar = "*";
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "discontinueDate");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Common, "selectChar");

              field2.Error = true;
            }

            // Reset Discontinue dt
            local.MaxDt.Date =
              export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate;
            export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate2();
          }
        }

        break;
      default:
        break;
    }

Test:

    if (Equal(global.Command, "DISPLAY"))
    {
      // Office must be entered
      if (export.Office.SystemGeneratedId == 0)
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        return;
      }

      if (!IsEmpty(export.Search.AssignmentType) && !
        Equal(export.Search.AssignmentType, "*"))
      {
        export.Code.CodeName = "ASSIGNMENT TYPE";
        export.CodeValue.Cdvalue = export.Search.AssignmentType;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCodeValue.Flag) == 'Y')
        {
        }
        else
        {
          var field = GetField(export.Search, "assignmentType");

          field.Error = true;

          ExitState = "SP0000_INVALID_TYPE_CODE";

          return;
        }
      }

      if (!export.Export1.IsEmpty)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          export.Export1.Update.OfficeAssignmentPlan.AlphaAssignmentInd = "";
          export.Export1.Update.OfficeAssignmentPlan.AssignmentType = "";
          export.Export1.Update.OfficeAssignmentPlan.TribunalInd = "";
          export.Export1.Update.OfficeAssignmentPlan.FunctionAssignmentInd = "";
          export.Export1.Update.OfficeAssignmentPlan.ProgramAssignmentInd = "";
          export.Export1.Update.Common.SelectChar = "";
          export.Export1.Update.OfficeAssignmentPlan.EffectiveDate =
            local.Null1.Date;
          export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
            local.Null1.Date;
        }
      }

      if (ReadOfficeOfficeAddress())
      {
        export.Office.Assign(entities.Office);
        export.HiddenOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
        export.OfficeAddress.City = entities.OfficeAddress.City;
        local.Code.CodeName = "OFFICE TYPE";
        local.Code.Id = 0;
        local.CodeValue.Cdvalue = export.Office.TypeCode;
        UseCabGetCodeValueDescription();

        if (AsChar(local.ErrorInDecoding.Flag) == 'N')
        {
          MoveCodeValue(export.CodeValue, export.HiddenCodeValue);
        }

        if (!IsEmpty(export.Search.AssignmentType) && !
          Equal(export.Search.AssignmentType, "*"))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadOfficeAssignmentPlan1())
          {
            export.Export1.Update.OfficeAssignmentPlan.Assign(
              entities.OfficeAssignmentPlan);
            export.Export1.Update.PrevOfficeAssignmentPlan.Assign(
              entities.OfficeAssignmentPlan);

            var field1 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "assignmentType");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.OfficeAssignmentPlan, "effectiveDate");
              

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "alphaAssignmentInd");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Export1.Item.OfficeAssignmentPlan, "tribunalInd");
              

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "functionAssignmentInd");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "programAssignmentInd");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "discontinueDate");

            field7.Color = "cyan";
            field7.Protected = true;

            local.MaxDt.Date =
              export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate;
            export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate2();

            if (!Lt(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
              local.Current.Date) || Equal
              (export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "discontinueDate");

              field.Color = "green";
              field.Protected = false;
            }

            export.Export1.Next();
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadOfficeAssignmentPlan2())
          {
            export.Export1.Update.OfficeAssignmentPlan.Assign(
              entities.OfficeAssignmentPlan);
            export.Export1.Update.PrevOfficeAssignmentPlan.Assign(
              entities.OfficeAssignmentPlan);

            var field1 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "assignmentType");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.OfficeAssignmentPlan, "effectiveDate");
              

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "alphaAssignmentInd");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Export1.Item.OfficeAssignmentPlan, "tribunalInd");
              

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "functionAssignmentInd");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "programAssignmentInd");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Export1.Item.OfficeAssignmentPlan,
              "discontinueDate");

            field7.Color = "cyan";
            field7.Protected = true;

            local.MaxDt.Date =
              export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate;
            export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate2();

            if (!Lt(export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
              local.Current.Date) || Equal
              (export.Export1.Item.OfficeAssignmentPlan.DiscontinueDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Export1.Item.OfficeAssignmentPlan,
                "discontinueDate");

              field.Color = "green";
              field.Protected = false;
            }

            export.Export1.Next();
          }
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
      }
      else
      {
        // Clear Screen
        export.Office.Name = "";
        export.OfficeAddress.City = "";
        export.CodeValue.Description = Spaces(CodeValue.Description_MaxLength);

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          export.Export1.Update.OfficeAssignmentPlan.AlphaAssignmentInd = "";
          export.Export1.Update.OfficeAssignmentPlan.TribunalInd = "";
          export.Export1.Update.OfficeAssignmentPlan.FunctionAssignmentInd = "";
          export.Export1.Update.OfficeAssignmentPlan.ProgramAssignmentInd = "";
          export.Export1.Update.OfficeAssignmentPlan.EffectiveDate =
            local.Null1.Date;
          export.Export1.Update.OfficeAssignmentPlan.DiscontinueDate =
            local.Null1.Date;
          export.Export1.Update.Common.SelectChar = "";
        }

        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        ExitState = "OFFICE_NF";
      }

      export.HiddenOffice.SystemGeneratedId = export.Office.SystemGeneratedId;

      // Move imports to prev exports
      export.PrevOffice.Assign(export.Office);
      export.PrevCodeValue.Description = export.CodeValue.Description;
      export.PrevOfficeAddress.City = export.OfficeAddress.City;
      export.PrevSearch.AssignmentType = export.Search.AssignmentType;
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
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

  private static void MoveOfficeAssignmentPlan1(OfficeAssignmentPlan source,
    OfficeAssignmentPlan target)
  {
    target.AssignmentType = source.AssignmentType;
    target.CountyAssignmentInd = source.CountyAssignmentInd;
    target.AlphaAssignmentInd = source.AlphaAssignmentInd;
    target.FunctionAssignmentInd = source.FunctionAssignmentInd;
    target.ProgramAssignmentInd = source.ProgramAssignmentInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveOfficeAssignmentPlan2(OfficeAssignmentPlan source,
    OfficeAssignmentPlan target)
  {
    target.AssignmentType = source.AssignmentType;
    target.CountyAssignmentInd = source.CountyAssignmentInd;
    target.AlphaAssignmentInd = source.AlphaAssignmentInd;
    target.FunctionAssignmentInd = source.FunctionAssignmentInd;
    target.ProgramAssignmentInd = source.ProgramAssignmentInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTstamp = source.CreatedTstamp;
    target.TribunalInd = source.TribunalInd;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    MoveCode(local.Code, useImport.Code);

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
    local.ErrorInDecoding.Flag = useExport.ErrorInDecoding.Flag;
    MoveCodeValue(useExport.CodeValue, export.CodeValue);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDt.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = export.Code.CodeName;
    useImport.CodeValue.Cdvalue = export.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCodeValue.Flag = useExport.ValidCode.Flag;
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

  private void UseSpCreateOfficeAsgnmntScheme()
  {
    var useImport = new SpCreateOfficeAsgnmntScheme.Import();
    var useExport = new SpCreateOfficeAsgnmntScheme.Export();

    MoveOfficeAssignmentPlan2(export.Export1.Item.OfficeAssignmentPlan,
      useImport.OfficeAssignmentPlan);
    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;

    Call(SpCreateOfficeAsgnmntScheme.Execute, useImport, useExport);

    export.Export1.Update.OfficeAssignmentPlan.Assign(
      useExport.OfficeAssignmentPlan);
  }

  private void UseSpDeleteOfficeAsgnmntScheme()
  {
    var useImport = new SpDeleteOfficeAsgnmntScheme.Import();
    var useExport = new SpDeleteOfficeAsgnmntScheme.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveOfficeAssignmentPlan1(export.Export1.Item.OfficeAssignmentPlan,
      useImport.OfficeAssignmentPlan);

    Call(SpDeleteOfficeAsgnmntScheme.Execute, useImport, useExport);
  }

  private void UseSpUpdateOfficeAsgnmntScheme()
  {
    var useImport = new SpUpdateOfficeAsgnmntScheme.Import();
    var useExport = new SpUpdateOfficeAsgnmntScheme.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveOfficeAssignmentPlan1(export.Export1.Item.OfficeAssignmentPlan,
      useImport.OfficeAssignmentPlan);

    Call(SpUpdateOfficeAsgnmntScheme.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadOfficeAssignmentPlan1()
  {
    return ReadEach("ReadOfficeAssignmentPlan1",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetString(command, "assignmentType", export.Search.AssignmentType);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.OfficeAssignmentPlan.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAssignmentPlan.EffectiveDate = db.GetDate(reader, 1);
        entities.OfficeAssignmentPlan.AssignmentType = db.GetString(reader, 2);
        entities.OfficeAssignmentPlan.CountyAssignmentInd =
          db.GetString(reader, 3);
        entities.OfficeAssignmentPlan.AlphaAssignmentInd =
          db.GetString(reader, 4);
        entities.OfficeAssignmentPlan.FunctionAssignmentInd =
          db.GetString(reader, 5);
        entities.OfficeAssignmentPlan.ProgramAssignmentInd =
          db.GetString(reader, 6);
        entities.OfficeAssignmentPlan.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.OfficeAssignmentPlan.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.OfficeAssignmentPlan.TribunalInd = db.GetString(reader, 9);
        entities.OfficeAssignmentPlan.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeAssignmentPlan2()
  {
    return ReadEach("ReadOfficeAssignmentPlan2",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.OfficeAssignmentPlan.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAssignmentPlan.EffectiveDate = db.GetDate(reader, 1);
        entities.OfficeAssignmentPlan.AssignmentType = db.GetString(reader, 2);
        entities.OfficeAssignmentPlan.CountyAssignmentInd =
          db.GetString(reader, 3);
        entities.OfficeAssignmentPlan.AlphaAssignmentInd =
          db.GetString(reader, 4);
        entities.OfficeAssignmentPlan.FunctionAssignmentInd =
          db.GetString(reader, 5);
        entities.OfficeAssignmentPlan.ProgramAssignmentInd =
          db.GetString(reader, 6);
        entities.OfficeAssignmentPlan.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.OfficeAssignmentPlan.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.OfficeAssignmentPlan.TribunalInd = db.GetString(reader, 9);
        entities.OfficeAssignmentPlan.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeAddress",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.OfficeAddress.Type1 = db.GetString(reader, 4);
        entities.OfficeAddress.City = db.GetString(reader, 5);
        entities.OfficeAddress.Populated = true;
        entities.Office.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of PrevCommon.
      /// </summary>
      [JsonPropertyName("prevCommon")]
      public Common PrevCommon
      {
        get => prevCommon ??= new();
        set => prevCommon = value;
      }

      /// <summary>
      /// A value of PrevOfficeAssignmentPlan.
      /// </summary>
      [JsonPropertyName("prevOfficeAssignmentPlan")]
      public OfficeAssignmentPlan PrevOfficeAssignmentPlan
      {
        get => prevOfficeAssignmentPlan ??= new();
        set => prevOfficeAssignmentPlan = value;
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
      /// A value of OfficeAssignmentPlan.
      /// </summary>
      [JsonPropertyName("officeAssignmentPlan")]
      public OfficeAssignmentPlan OfficeAssignmentPlan
      {
        get => officeAssignmentPlan ??= new();
        set => officeAssignmentPlan = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common prevCommon;
      private OfficeAssignmentPlan prevOfficeAssignmentPlan;
      private Common common;
      private OfficeAssignmentPlan officeAssignmentPlan;
    }

    /// <summary>
    /// A value of PrevSearch.
    /// </summary>
    [JsonPropertyName("prevSearch")]
    public OfficeAssignmentPlan PrevSearch
    {
      get => prevSearch ??= new();
      set => prevSearch = value;
    }

    /// <summary>
    /// A value of HiddenCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenCodeValue")]
    public CodeValue HiddenCodeValue
    {
      get => hiddenCodeValue ??= new();
      set => hiddenCodeValue = value;
    }

    /// <summary>
    /// A value of ListCodeValue.
    /// </summary>
    [JsonPropertyName("listCodeValue")]
    public Common ListCodeValue
    {
      get => listCodeValue ??= new();
      set => listCodeValue = value;
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
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of PrevOfficeAddress.
    /// </summary>
    [JsonPropertyName("prevOfficeAddress")]
    public OfficeAddress PrevOfficeAddress
    {
      get => prevOfficeAddress ??= new();
      set => prevOfficeAddress = value;
    }

    /// <summary>
    /// A value of PrevCodeValue.
    /// </summary>
    [JsonPropertyName("prevCodeValue")]
    public CodeValue PrevCodeValue
    {
      get => prevCodeValue ??= new();
      set => prevCodeValue = value;
    }

    /// <summary>
    /// A value of PrevOffice.
    /// </summary>
    [JsonPropertyName("prevOffice")]
    public Office PrevOffice
    {
      get => prevOffice ??= new();
      set => prevOffice = value;
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
    /// A value of ListOffice.
    /// </summary>
    [JsonPropertyName("listOffice")]
    public Common ListOffice
    {
      get => listOffice ??= new();
      set => listOffice = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public OfficeAssignmentPlan Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
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

    private OfficeAssignmentPlan prevSearch;
    private CodeValue hiddenCodeValue;
    private Common listCodeValue;
    private Code code;
    private Office hiddenOffice;
    private OfficeAddress prevOfficeAddress;
    private CodeValue prevCodeValue;
    private Office prevOffice;
    private CodeValue codeValue;
    private Common listOffice;
    private OfficeAddress officeAddress;
    private Office office;
    private OfficeAssignmentPlan search;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of PrevCommon.
      /// </summary>
      [JsonPropertyName("prevCommon")]
      public Common PrevCommon
      {
        get => prevCommon ??= new();
        set => prevCommon = value;
      }

      /// <summary>
      /// A value of PrevOfficeAssignmentPlan.
      /// </summary>
      [JsonPropertyName("prevOfficeAssignmentPlan")]
      public OfficeAssignmentPlan PrevOfficeAssignmentPlan
      {
        get => prevOfficeAssignmentPlan ??= new();
        set => prevOfficeAssignmentPlan = value;
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
      /// A value of OfficeAssignmentPlan.
      /// </summary>
      [JsonPropertyName("officeAssignmentPlan")]
      public OfficeAssignmentPlan OfficeAssignmentPlan
      {
        get => officeAssignmentPlan ??= new();
        set => officeAssignmentPlan = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common prevCommon;
      private OfficeAssignmentPlan prevOfficeAssignmentPlan;
      private Common common;
      private OfficeAssignmentPlan officeAssignmentPlan;
    }

    /// <summary>
    /// A value of PrevSearch.
    /// </summary>
    [JsonPropertyName("prevSearch")]
    public OfficeAssignmentPlan PrevSearch
    {
      get => prevSearch ??= new();
      set => prevSearch = value;
    }

    /// <summary>
    /// A value of HiddenCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenCodeValue")]
    public CodeValue HiddenCodeValue
    {
      get => hiddenCodeValue ??= new();
      set => hiddenCodeValue = value;
    }

    /// <summary>
    /// A value of ListCodeValue.
    /// </summary>
    [JsonPropertyName("listCodeValue")]
    public Common ListCodeValue
    {
      get => listCodeValue ??= new();
      set => listCodeValue = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public OfficeAssignmentPlan Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of PrevOfficeAddress.
    /// </summary>
    [JsonPropertyName("prevOfficeAddress")]
    public OfficeAddress PrevOfficeAddress
    {
      get => prevOfficeAddress ??= new();
      set => prevOfficeAddress = value;
    }

    /// <summary>
    /// A value of PrevCodeValue.
    /// </summary>
    [JsonPropertyName("prevCodeValue")]
    public CodeValue PrevCodeValue
    {
      get => prevCodeValue ??= new();
      set => prevCodeValue = value;
    }

    /// <summary>
    /// A value of PrevOffice.
    /// </summary>
    [JsonPropertyName("prevOffice")]
    public Office PrevOffice
    {
      get => prevOffice ??= new();
      set => prevOffice = value;
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
    /// A value of ListOffice.
    /// </summary>
    [JsonPropertyName("listOffice")]
    public Common ListOffice
    {
      get => listOffice ??= new();
      set => listOffice = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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

    private OfficeAssignmentPlan prevSearch;
    private CodeValue hiddenCodeValue;
    private Common listCodeValue;
    private Code code;
    private OfficeAssignmentPlan search;
    private Office hiddenOffice;
    private OfficeAddress prevOfficeAddress;
    private CodeValue prevCodeValue;
    private Office prevOffice;
    private CodeValue codeValue;
    private Common listOffice;
    private OfficeAddress officeAddress;
    private Office office;
    private Array<ExportGroup> export1;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of ErrorInDecoding.
    /// </summary>
    [JsonPropertyName("errorInDecoding")]
    public Common ErrorInDecoding
    {
      get => errorInDecoding ??= new();
      set => errorInDecoding = value;
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
    /// A value of ValidCodeValue.
    /// </summary>
    [JsonPropertyName("validCodeValue")]
    public Common ValidCodeValue
    {
      get => validCodeValue ??= new();
      set => validCodeValue = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of MaxDt.
    /// </summary>
    [JsonPropertyName("maxDt")]
    public DateWorkArea MaxDt
    {
      get => maxDt ??= new();
      set => maxDt = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common returnCode;
    private Common errorInDecoding;
    private DateWorkArea current;
    private Common validCodeValue;
    private DateWorkArea null1;
    private DateWorkArea maxDt;
    private Common selCount;
    private CodeValue codeValue;
    private Code code;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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
    /// A value of OfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("officeAssignmentPlan")]
    public OfficeAssignmentPlan OfficeAssignmentPlan
    {
      get => officeAssignmentPlan ??= new();
      set => officeAssignmentPlan = value;
    }

    private OfficeAddress officeAddress;
    private Office office;
    private OfficeAssignmentPlan officeAssignmentPlan;
  }
#endregion
}

// Program: SP_OFCD_OFC_CASELOD_ASGNMT_DSPLY, ID: 372563997, model: 746.
// Short name: SWEOFCDP
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
/// A program: SP_OFCD_OFC_CASELOD_ASGNMT_DSPLY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpOfcdOfcCaselodAsgnmtDsply: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_OFCD_OFC_CASELOD_ASGNMT_DSPLY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpOfcdOfcCaselodAsgnmtDsply(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpOfcdOfcCaselodAsgnmtDsply.
  /// </summary>
  public SpOfcdOfcCaselodAsgnmtDsply(IContext context, Import import,
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
    // ***************************************************************************************
    // **   DATE    Developer   PR/WR   DESCRIPTION
    // ** 04/13/95     J. Kemp
    // ** 02/15/96     a. hackler   retrofits
    // ** 12/24/96	J. Rookard   refit to new security, modify to reflect new 
    // Service Plan
    // **                           architecture.
    // ** 01/06/97     J. Rookard   Continue refit to new Service Plan 
    // architecture.
    // ** 01/20/97     J. Rookard   Continue refit to new Service Plan 
    // architecture.
    // ** 04/11/97     J. Rookard   Add prompt to CDVL to allow pick and return 
    // of Caseload
    // **                           Assignment Type.
    // ** 04/21/97     J. Rookard   Clarify command of Display logic.
    // ** 09/12/00  SWSRCHF   H00102871 Check security before flowing to OFCA
    // ** 12/21/00  SWSRCHF    000265   Increase length of Last Name(s) in the 
    // ALPHA THRU
    // **
    // 
    // area of the screen
    // ** 11/01/01      K. Doshi    PR130467- Change Sort criteria.
    // ** 08/16/13      GVandy      CQ38147 Change assignment by county to 
    // assignment by tribunal.
    // ** 11/04/13      GVandy      CQ41845 Change display order to priority, 
    // program, tribunal,
    // 				     function, and alpha.
    // ** 08/28/18      R Mathews   CQ60203 Allow referral assignment by alpha 
    // and/or program
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.Office.Assign(import.Office);
    export.OfficeAddress.City = import.OfficeAddress.City;
    export.OfficeType.Description = import.OfficeType.Description;
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.Priority.Priority = import.Priority.Priority;
    MoveOfficeCaseloadAssignment3(import.SearchAsgnInd, export.SearchAsgnInd);
    export.SearchType.AssignmentType = import.SearchType.AssignmentType;
    export.Code.CodeName = import.Code.CodeName;
    export.CodeValue.Cdvalue = import.CodeValue.Cdvalue;
    export.ListCodeValue.Flag = import.ListCodeValue.Flag;
    export.ListOffice.Flag = import.ListOffice.Flag;
    export.ActiveOffice.Flag = import.ActiveOffice.Flag;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.OfficeCaseloadAssignment.Assign(
        import.Import1.Item.OfficeCaseloadAssignment);
      export.Export1.Update.Program.Code = import.Import1.Item.Program.Code;
      export.Export1.Update.Tribunal.Identifier =
        import.Import1.Item.Tribunal.Identifier;
      export.Export1.Update.ServiceProvider.Assign(
        import.Import1.Item.ServiceProvider);
      export.Export1.Update.OfficeServiceProvider.RoleCode =
        import.Import1.Item.OfficeServiceProvider.RoleCode;
      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // **** validate action all commands  ****
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (IsEmpty(import.Standard.NextTransaction))
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }
        else
        {
          // if the next tran info is not equal to spaces, this implies the user
          // requested a next tran action. now validate
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

        break;
      case "HELP":
        global.Command = "DISPLAY";

        break;
      case "LIST":
        local.PromptCount.Count = 0;

        if (!IsEmpty(export.ListCodeValue.Flag) && AsChar
          (export.ListCodeValue.Flag) != '+')
        {
          var field = GetField(export.ListCodeValue, "flag");

          field.Error = true;

          local.PromptCount.Count = 1;
        }

        if (!IsEmpty(export.ListOffice.Flag) && AsChar
          (export.ListOffice.Flag) != '+')
        {
          var field = GetField(export.ListOffice, "flag");

          field.Error = true;

          ++local.PromptCount.Count;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          case 1:
            if (AsChar(export.ListCodeValue.Flag) == 'S' || AsChar
              (export.ListOffice.Flag) == 'S')
            {
              if (AsChar(export.ListOffice.Flag) == 'S')
              {
                var field = GetField(export.ListOffice, "flag");

                field.Protected = false;

                ExitState = "ECO_LNK_TO_LIST_OFFICE";

                return;
              }
              else if (AsChar(export.ListCodeValue.Flag) == 'S')
              {
                var field = GetField(export.ListCodeValue, "flag");

                field.Protected = false;

                export.Code.CodeName = "ASSIGNMENT TYPE";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                return;
              }
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "DISPLAY":
        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "OFCA":
        if (export.Office.SystemGeneratedId == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          break;
        }

        if (IsEmpty(export.OfficeAddress.City))
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          break;
        }

        if (!Equal(export.SearchType.AssignmentType, "CA") && !
          Equal(export.SearchType.AssignmentType, "RE"))
        {
          ExitState = "INVALID_WITH_TYPE_CODE";

          var field = GetField(export.SearchType, "assignmentType");

          field.Error = true;

          break;
        }

        if (Equal(export.SearchType.AssignmentType, "RE"))
        {
          // *** Problem report H00102871
          // *** 09/12/00 SWSRCHF
          // *** start
          UseScCabTestSecurity();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // *** end
          // *** 09/12/00 SWSRCHF
          // *** Problem report H00102871
          global.Command = "DISPLAY";
          ExitState = "ECO_LNK_TO_OFCA";

          return;
        }

        if (AsChar(export.ActiveOffice.Flag) == 'Y')
        {
          ExitState = "SP0000_INVALID_COM_INACT_OFFCE";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          break;
        }
        else if (IsEmpty(export.ActiveOffice.Flag))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        // *** Problem report H00102871
        // *** 09/12/00 SWSRCHF
        // *** start
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // *** end
        // *** 09/12/00 SWSRCHF
        // *** Problem report H00102871
        // **** IF any R or P Assignment types exists  - set command to Display 
        // *****
        // **** If none exists set command
        // to copy
        // 
        // ****
        global.Command = "COPY";

        if (ReadOfficeCaseloadAssignment1())
        {
          global.Command = "DISPLAY";
        }

        ExitState = "ECO_LNK_TO_OFCA";

        return;
      case "RETCDVL":
        export.ListCodeValue.Flag = "";

        var field1 = GetField(export.ListCodeValue, "flag");

        field1.Protected = false;
        field1.Focused = true;

        if (IsEmpty(import.SelectedCodeValue.Cdvalue))
        {
          // User flowed to CDVL to select an Assignment Type, did not select 
          // anything, and returned.
          return;
        }
        else
        {
          export.SearchType.AssignmentType = import.SelectedCodeValue.Cdvalue;
          global.Command = "DISPLAY";
        }

        break;
      case "RETOFCL":
        export.ListOffice.Flag = "";

        var field2 = GetField(export.ListOffice, "flag");

        field2.Protected = false;
        field2.Focused = true;

        if (import.SelectedOffice.SystemGeneratedId == 0)
        {
          // User flowed to OFCL to select an Office, did not select anything, 
          // and returned.
          return;
        }
        else
        {
          export.Office.Assign(import.SelectedOffice);
          global.Command = "DISPLAY";
        }

        break;
      case "XXFMMENU":
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
        return;
      case "XXNEXTXX":
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        UseScCabNextTranGet();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.ListOffice.Flag = "";

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        MoveOfficeCaseloadAssignment2(local.InitOfficeCaseloadAssignment,
          export.Export1.Update.OfficeCaseloadAssignment);
        export.Export1.Update.Program.Code = local.InitProgram.Code;
        export.Export1.Update.Tribunal.Identifier =
          local.InitTribunal.Identifier;
      }

      if (export.Office.SystemGeneratedId == 0)
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        return;
      }

      if (Equal(export.SearchType.AssignmentType, "CA"))
      {
        if (AsChar(export.SearchAsgnInd.AssignmentIndicator) == 'A' || AsChar
          (export.SearchAsgnInd.AssignmentIndicator) == 'P' || AsChar
          (export.SearchAsgnInd.AssignmentIndicator) == 'S' || AsChar
          (export.SearchAsgnInd.AssignmentIndicator) == 'R')
        {
          local.LowOfficeCaseloadAssignment.AssignmentIndicator =
            export.SearchAsgnInd.AssignmentIndicator;
          local.HiOfficeCaseloadAssignment.AssignmentIndicator =
            export.SearchAsgnInd.AssignmentIndicator;
        }
        else
        {
          local.LowOfficeCaseloadAssignment.AssignmentIndicator = "A";
          local.HiOfficeCaseloadAssignment.AssignmentIndicator = "S";
        }
      }
      else if (Equal(export.SearchType.AssignmentType, "RE"))
      {
        local.LowOfficeCaseloadAssignment.AssignmentIndicator = "A";
        local.HiOfficeCaseloadAssignment.AssignmentIndicator = "S";
      }
      else if (IsEmpty(export.SearchType.AssignmentType))
      {
        export.SearchType.AssignmentType = "CA";
        local.LowOfficeCaseloadAssignment.AssignmentIndicator = "A";
        local.HiOfficeCaseloadAssignment.AssignmentIndicator = "S";
      }
      else
      {
        var field = GetField(export.SearchType, "assignmentType");

        field.Error = true;

        ExitState = "INVALID_TYPE_CODE";

        return;
      }

      local.ServiceProvider.LastName = export.ServiceProvider.LastName;

      if (export.ServiceProvider.SystemGeneratedId != 0)
      {
        local.LowServiceProvider.SystemGeneratedId =
          export.ServiceProvider.SystemGeneratedId;
        local.HiServiceProvider.SystemGeneratedId =
          export.ServiceProvider.SystemGeneratedId;
        local.ServiceProvider.LastName = "";
      }
      else
      {
        local.LowServiceProvider.SystemGeneratedId = 0;
        local.HiServiceProvider.SystemGeneratedId = 99999;
      }

      if (ReadOffice())
      {
        // currency on Office acquired.
        export.Office.Assign(entities.Office);

        if (Lt(entities.Office.DiscontinueDate, local.Current.Date))
        {
          export.ActiveOffice.Flag = "Y";
        }
        else
        {
          export.ActiveOffice.Flag = "N";
        }
      }
      else
      {
        export.Office.Name = "";

        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        ExitState = "OFFICE_NF";

        return;
      }

      if (ReadOfficeAddress())
      {
        // currency on Office Address acquired.
        export.OfficeAddress.City = entities.OfficeAddress.City;
      }
      else
      {
        var field = GetField(export.OfficeAddress, "city");

        field.Error = true;

        ExitState = "OFFICE_ADDRESS_NF";
      }

      if (ReadCodeValue())
      {
        export.OfficeType.Description = entities.CodeValue.Description;
      }
      else
      {
        ExitState = "CODE_VALUE_NF";
      }

      // -- 11/04/13 GVandy CQ41845 Change display order to priority, program, 
      // tribunal,
      //    function, and alpha.  Original code commented out below.
      local.Local1.Index = -1;

      foreach(var item in ReadOfficeCaseloadAssignmentOfficeServiceProvider())
      {
        if (local.Local1.Index + 1 < Local.LocalGroup.Capacity)
        {
          ++local.Local1.Index;
          local.Local1.CheckSize();
        }
        else
        {
          break;
        }

        MoveOfficeCaseloadAssignment1(entities.OfficeCaseloadAssignment,
          local.Local1.Update.OfficeCaseloadAssignment);
        local.Local1.Update.ServiceProvider.Assign(entities.ServiceProvider);
        local.Local1.Update.OfficeServiceProvider.RoleCode =
          entities.OfficeServiceProvider.RoleCode;

        // CQ60203 Add program lookup for referral assignment display
        if (Equal(export.SearchType.AssignmentType, "CA") || Equal
          (export.SearchType.AssignmentType, "RE"))
        {
          if (ReadProgram())
          {
            local.Local1.Update.Program.Code = entities.Program.Code;
          }
        }

        if (Equal(export.SearchType.AssignmentType, "CA"))
        {
          if (ReadTribunal())
          {
            local.Local1.Update.Tribunal.Identifier =
              entities.Tribunal.Identifier;
          }
        }
      }

      // -- Sort assignment by Priority, Program, Tribunal, Function, and Alpha.
      //    A value in one of these sort criteria displays before blanks.
      local.I.Count = 1;

      for(var limit = local.Local1.Count; local.I.Count <= limit; ++
        local.I.Count)
      {
        local.Local1.Index = local.I.Count - 1;
        local.Local1.CheckSize();

        local.Compare.CompareOfficeCaseloadAssignment.Assign(
          local.Local1.Item.OfficeCaseloadAssignment);
        local.Compare.CompareOfficeServiceProvider.RoleCode =
          local.Local1.Item.OfficeServiceProvider.RoleCode;
        local.Compare.CompareProgram.Code = local.Local1.Item.Program.Code;
        local.Compare.CompareServiceProvider.Assign(
          local.Local1.Item.ServiceProvider);
        local.Compare.CompareTribunal.Identifier =
          local.Local1.Item.Tribunal.Identifier;
        local.J.Count = local.I.Count + 1;

        for(var limit1 = local.Local1.Count; local.J.Count <= limit1; ++
          local.J.Count)
        {
          local.Local1.Index = local.J.Count - 1;
          local.Local1.CheckSize();

          local.Swap1.Flag = "N";

          if (AsChar(local.Swap1.Flag) == 'N')
          {
            // -- Priority is the first sort criteria.
            if (local.Local1.Item.OfficeCaseloadAssignment.Priority < local
              .Compare.CompareOfficeCaseloadAssignment.Priority)
            {
              local.Swap1.Flag = "Y";

              goto Test;
            }
            else if (local.Local1.Item.OfficeCaseloadAssignment.Priority > local
              .Compare.CompareOfficeCaseloadAssignment.Priority)
            {
              continue;
            }

            // -- Program is the second sort criteria.
            if (IsEmpty(local.Local1.Item.Program.Code))
            {
              if (!IsEmpty(local.Compare.CompareProgram.Code))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.Program.Code,
              local.Compare.CompareProgram.Code) || IsEmpty
              (local.Compare.CompareProgram.Code))
            {
              local.Swap1.Flag = "Y";

              goto Test;
            }
            else if (Lt(local.Compare.CompareProgram.Code,
              local.Local1.Item.Program.Code))
            {
              continue;
            }

            // -- Tribunal is the third sort criteria.
            if (local.Local1.Item.Tribunal.Identifier == 0)
            {
              if (local.Compare.CompareTribunal.Identifier != 0)
              {
                continue;
              }
            }
            else if (local.Local1.Item.Tribunal.Identifier < local
              .Compare.CompareTribunal.Identifier || local
              .Compare.CompareTribunal.Identifier == 0)
            {
              local.Swap1.Flag = "Y";

              goto Test;
            }
            else if (local.Local1.Item.Tribunal.Identifier > local
              .Compare.CompareTribunal.Identifier)
            {
              continue;
            }

            // -- Function is the fourth sort criteria.
            if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.Function))
            {
              if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
                Function))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.Function,
              local.Compare.CompareOfficeCaseloadAssignment.Function) || IsEmpty
              (local.Compare.CompareOfficeCaseloadAssignment.Function))
            {
              local.Swap1.Flag = "Y";

              goto Test;
            }
            else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.Function,
              local.Local1.Item.OfficeCaseloadAssignment.Function))
            {
              continue;
            }

            // -- Beginning Alpha is the fifth sort criteria.
            if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha))
              
            {
              if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
                BeginingAlpha))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.
              BeginingAlpha,
              local.Compare.CompareOfficeCaseloadAssignment.BeginingAlpha) || IsEmpty
              (local.Compare.CompareOfficeCaseloadAssignment.BeginingAlpha))
            {
              local.Swap1.Flag = "Y";

              goto Test;
            }
            else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.
              BeginingAlpha,
              local.Local1.Item.OfficeCaseloadAssignment.BeginingAlpha))
            {
              continue;
            }

            // -- Ending Alpha is the sixth sort criteria.
            if (IsEmpty(local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha))
            {
              if (!IsEmpty(local.Compare.CompareOfficeCaseloadAssignment.
                EndingAlpha))
              {
                continue;
              }
            }
            else if (Lt(local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha,
              local.Compare.CompareOfficeCaseloadAssignment.EndingAlpha) || IsEmpty
              (local.Compare.CompareOfficeCaseloadAssignment.EndingAlpha))
            {
              local.Swap1.Flag = "Y";
            }
            else if (Lt(local.Compare.CompareOfficeCaseloadAssignment.
              EndingAlpha,
              local.Local1.Item.OfficeCaseloadAssignment.EndingAlpha))
            {
              continue;
            }
          }

Test:

          if (AsChar(local.Swap1.Flag) == 'N')
          {
            continue;
          }

          local.Swap.SwapOfficeCaseloadAssignment.Assign(
            local.Local1.Item.OfficeCaseloadAssignment);
          local.Swap.SwapOfficeServiceProvider.RoleCode =
            local.Local1.Item.OfficeServiceProvider.RoleCode;
          local.Swap.SwapProgram.Code = local.Local1.Item.Program.Code;
          local.Swap.SwapServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          local.Swap.SwapTribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
          local.Local1.Update.OfficeCaseloadAssignment.Assign(
            local.Compare.CompareOfficeCaseloadAssignment);
          local.Local1.Update.OfficeServiceProvider.RoleCode =
            local.Compare.CompareOfficeServiceProvider.RoleCode;
          local.Local1.Update.Program.Code = local.Compare.CompareProgram.Code;
          local.Local1.Update.ServiceProvider.Assign(
            local.Compare.CompareServiceProvider);
          local.Local1.Update.Tribunal.Identifier =
            local.Compare.CompareTribunal.Identifier;

          local.Local1.Index = local.I.Count - 1;
          local.Local1.CheckSize();

          local.Local1.Update.OfficeCaseloadAssignment.Assign(
            local.Swap.SwapOfficeCaseloadAssignment);
          local.Local1.Update.OfficeServiceProvider.RoleCode =
            local.Swap.SwapOfficeServiceProvider.RoleCode;
          local.Local1.Update.Program.Code = local.Swap.SwapProgram.Code;
          local.Local1.Update.ServiceProvider.Assign(
            local.Swap.SwapServiceProvider);
          local.Local1.Update.Tribunal.Identifier =
            local.Swap.SwapTribunal.Identifier;
          local.Compare.CompareOfficeCaseloadAssignment.Assign(
            local.Local1.Item.OfficeCaseloadAssignment);
          local.Compare.CompareOfficeServiceProvider.RoleCode =
            local.Local1.Item.OfficeServiceProvider.RoleCode;
          local.Compare.CompareProgram.Code = local.Local1.Item.Program.Code;
          local.Compare.CompareServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          local.Compare.CompareTribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
        }
      }

      if (local.Local1.Count == 0)
      {
        // -- Initialize export group
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(local.Initialized.Index = 0; local.Initialized.Index < local
          .Initialized.Count; ++local.Initialized.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Next();
        }
      }
      else
      {
        // -- Move date from the local group to the export group
        local.Local1.Index = -1;

        export.Export1.Index = 0;
        export.Export1.Clear();

        do
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          ++local.Local1.Index;
          local.Local1.CheckSize();

          export.Export1.Update.OfficeCaseloadAssignment.Assign(
            local.Local1.Item.OfficeCaseloadAssignment);
          export.Export1.Update.OfficeServiceProvider.RoleCode =
            local.Local1.Item.OfficeServiceProvider.RoleCode;
          export.Export1.Update.Program.Code = local.Local1.Item.Program.Code;
          export.Export1.Update.ServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          export.Export1.Update.Tribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
          export.Export1.Next();
        }
        while(local.Local1.Index + 1 != local.Local1.Count);
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "SP0000_ONE_ACTIVE_A_TO_Z_REQUIRD";

        return;
      }

      foreach(var item in ReadOfficeCaseloadAssignment2())
      {
        if (ReadProgram())
        {
          continue;
        }

        if (ReadTribunal())
        {
          continue;
        }

        local.ActiveOffice.Flag = "Y";

        break;
      }

      if (AsChar(local.ActiveOffice.Flag) == 'Y')
      {
        ExitState = "SP0000_DISP_SUCC_ONE_A_Z_EXISTS";
      }
      else
      {
        ExitState = "SP0000_ONE_ACTIVE_A_TO_Z_REQUIRD";
      }
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

  private static void MoveOfficeCaseloadAssignment1(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.AssignmentIndicator = source.AssignmentIndicator;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EndingAlpha = source.EndingAlpha;
    target.EndingFirstInitial = source.EndingFirstInitial;
    target.BeginingAlpha = source.BeginingAlpha;
    target.BeginningFirstIntial = source.BeginningFirstIntial;
    target.Priority = source.Priority;
    target.Function = source.Function;
    target.AssignmentType = source.AssignmentType;
  }

  private static void MoveOfficeCaseloadAssignment2(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.AssignmentIndicator = source.AssignmentIndicator;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EndingAlpha = source.EndingAlpha;
    target.EndingFirstInitial = source.EndingFirstInitial;
    target.BeginingAlpha = source.BeginingAlpha;
    target.BeginningFirstIntial = source.BeginningFirstIntial;
    target.Function = source.Function;
    target.AssignmentType = source.AssignmentType;
  }

  private static void MoveOfficeCaseloadAssignment3(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.AssignmentIndicator = source.AssignmentIndicator;
    target.BeginingAlpha = source.BeginingAlpha;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetString(command, "typeCode", entities.Office.TypeCode);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.City = db.GetString(reader, 2);
        entities.OfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeCaseloadAssignment1()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignment1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.CreatedBy = db.GetString(reader, 8);
        entities.OfficeCaseloadAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 12);
        entities.OfficeCaseloadAssignment.EndingFirstInitial =
          db.GetNullableString(reader, 13);
        entities.OfficeCaseloadAssignment.BeginningFirstIntial =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 16);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 17);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 20);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 21);
        entities.OfficeCaseloadAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment2()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.CreatedBy = db.GetString(reader, 8);
        entities.OfficeCaseloadAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 12);
        entities.OfficeCaseloadAssignment.EndingFirstInitial =
          db.GetNullableString(reader, 13);
        entities.OfficeCaseloadAssignment.BeginningFirstIntial =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 16);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 17);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 20);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 21);
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignmentOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "assignmentIndicator1",
          local.LowOfficeCaseloadAssignment.AssignmentIndicator);
        db.SetString(
          command, "assignmentIndicator2",
          local.HiOfficeCaseloadAssignment.AssignmentIndicator);
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.
          SetString(command, "assignmentType", export.SearchType.AssignmentType);
          
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "priority", export.Priority.Priority);
        db.SetString(command, "lastName", local.ServiceProvider.LastName);
        db.SetInt32(
          command, "systemGeneratedId1",
          local.LowServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "systemGeneratedId2",
          local.HiServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.CreatedBy = db.GetString(reader, 8);
        entities.OfficeCaseloadAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 11);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 12);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 12);
        entities.OfficeCaseloadAssignment.EndingFirstInitial =
          db.GetNullableString(reader, 13);
        entities.OfficeCaseloadAssignment.BeginningFirstIntial =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 16);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 17);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 19);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 20);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 20);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 20);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 21);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 22);
        entities.ServiceProvider.UserId = db.GetString(reader, 23);
        entities.ServiceProvider.LastName = db.GetString(reader, 24);
        entities.ServiceProvider.FirstName = db.GetString(reader, 25);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadProgram()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId",
          entities.OfficeCaseloadAssignment.PrgGeneratedId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.OfficeCaseloadAssignment.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
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
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OfficeServiceProvider officeServiceProvider;
      private Program program;
      private Tribunal tribunal;
      private ServiceProvider serviceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
    }

    /// <summary>
    /// A value of Priority.
    /// </summary>
    [JsonPropertyName("priority")]
    public OfficeCaseloadAssignment Priority
    {
      get => priority ??= new();
      set => priority = value;
    }

    /// <summary>
    /// A value of ActiveOffice.
    /// </summary>
    [JsonPropertyName("activeOffice")]
    public Common ActiveOffice
    {
      get => activeOffice ??= new();
      set => activeOffice = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
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
    /// A value of ListCodeValue.
    /// </summary>
    [JsonPropertyName("listCodeValue")]
    public Common ListCodeValue
    {
      get => listCodeValue ??= new();
      set => listCodeValue = value;
    }

    /// <summary>
    /// A value of SearchType.
    /// </summary>
    [JsonPropertyName("searchType")]
    public OfficeCaseloadAssignment SearchType
    {
      get => searchType ??= new();
      set => searchType = value;
    }

    /// <summary>
    /// A value of OfficeType.
    /// </summary>
    [JsonPropertyName("officeType")]
    public CodeValue OfficeType
    {
      get => officeType ??= new();
      set => officeType = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of SearchAsgnInd.
    /// </summary>
    [JsonPropertyName("searchAsgnInd")]
    public OfficeCaseloadAssignment SearchAsgnInd
    {
      get => searchAsgnInd ??= new();
      set => searchAsgnInd = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
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

    private OfficeCaseloadAssignment priority;
    private Common activeOffice;
    private Office selectedOffice;
    private CodeValue selectedCodeValue;
    private CodeValue codeValue;
    private Code code;
    private Common listCodeValue;
    private OfficeCaseloadAssignment searchType;
    private CodeValue officeType;
    private Common listOffice;
    private ServiceProvider serviceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private OfficeCaseloadAssignment searchAsgnInd;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
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
      /// A value of OfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("officeServiceProvider")]
      public OfficeServiceProvider OfficeServiceProvider
      {
        get => officeServiceProvider ??= new();
        set => officeServiceProvider = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OfficeServiceProvider officeServiceProvider;
      private Program program;
      private Tribunal tribunal;
      private ServiceProvider serviceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
    }

    /// <summary>
    /// A value of Priority.
    /// </summary>
    [JsonPropertyName("priority")]
    public OfficeCaseloadAssignment Priority
    {
      get => priority ??= new();
      set => priority = value;
    }

    /// <summary>
    /// A value of ActiveOffice.
    /// </summary>
    [JsonPropertyName("activeOffice")]
    public Common ActiveOffice
    {
      get => activeOffice ??= new();
      set => activeOffice = value;
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
    /// A value of ListCodeValue.
    /// </summary>
    [JsonPropertyName("listCodeValue")]
    public Common ListCodeValue
    {
      get => listCodeValue ??= new();
      set => listCodeValue = value;
    }

    /// <summary>
    /// A value of SearchType.
    /// </summary>
    [JsonPropertyName("searchType")]
    public OfficeCaseloadAssignment SearchType
    {
      get => searchType ??= new();
      set => searchType = value;
    }

    /// <summary>
    /// A value of OfficeType.
    /// </summary>
    [JsonPropertyName("officeType")]
    public CodeValue OfficeType
    {
      get => officeType ??= new();
      set => officeType = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of SearchAsgnInd.
    /// </summary>
    [JsonPropertyName("searchAsgnInd")]
    public OfficeCaseloadAssignment SearchAsgnInd
    {
      get => searchAsgnInd ??= new();
      set => searchAsgnInd = value;
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
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public OfficeCaseloadAssignment HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
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

    private OfficeCaseloadAssignment priority;
    private Common activeOffice;
    private CodeValue codeValue;
    private Code code;
    private Common listCodeValue;
    private OfficeCaseloadAssignment searchType;
    private CodeValue officeType;
    private Common listOffice;
    private ServiceProvider serviceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private OfficeCaseloadAssignment searchAsgnInd;
    private Array<ExportGroup> export1;
    private OfficeCaseloadAssignment hiddenSelection;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A InitializedGroup group.</summary>
    [Serializable]
    public class InitializedGroup
    {
      /// <summary>
      /// A value of InitializedOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("initializedOfficeServiceProvider")]
      public OfficeServiceProvider InitializedOfficeServiceProvider
      {
        get => initializedOfficeServiceProvider ??= new();
        set => initializedOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of InitializedProgram.
      /// </summary>
      [JsonPropertyName("initializedProgram")]
      public Program InitializedProgram
      {
        get => initializedProgram ??= new();
        set => initializedProgram = value;
      }

      /// <summary>
      /// A value of InitializedTribunal.
      /// </summary>
      [JsonPropertyName("initializedTribunal")]
      public Tribunal InitializedTribunal
      {
        get => initializedTribunal ??= new();
        set => initializedTribunal = value;
      }

      /// <summary>
      /// A value of InitializedServiceProvider.
      /// </summary>
      [JsonPropertyName("initializedServiceProvider")]
      public ServiceProvider InitializedServiceProvider
      {
        get => initializedServiceProvider ??= new();
        set => initializedServiceProvider = value;
      }

      /// <summary>
      /// A value of InitializedOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("initializedOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment InitializedOfficeCaseloadAssignment
      {
        get => initializedOfficeCaseloadAssignment ??= new();
        set => initializedOfficeCaseloadAssignment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private OfficeServiceProvider initializedOfficeServiceProvider;
      private Program initializedProgram;
      private Tribunal initializedTribunal;
      private ServiceProvider initializedServiceProvider;
      private OfficeCaseloadAssignment initializedOfficeCaseloadAssignment;
    }

    /// <summary>A CompareGroup group.</summary>
    [Serializable]
    public class CompareGroup
    {
      /// <summary>
      /// A value of CompareOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("compareOfficeServiceProvider")]
      public OfficeServiceProvider CompareOfficeServiceProvider
      {
        get => compareOfficeServiceProvider ??= new();
        set => compareOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of CompareProgram.
      /// </summary>
      [JsonPropertyName("compareProgram")]
      public Program CompareProgram
      {
        get => compareProgram ??= new();
        set => compareProgram = value;
      }

      /// <summary>
      /// A value of CompareTribunal.
      /// </summary>
      [JsonPropertyName("compareTribunal")]
      public Tribunal CompareTribunal
      {
        get => compareTribunal ??= new();
        set => compareTribunal = value;
      }

      /// <summary>
      /// A value of CompareServiceProvider.
      /// </summary>
      [JsonPropertyName("compareServiceProvider")]
      public ServiceProvider CompareServiceProvider
      {
        get => compareServiceProvider ??= new();
        set => compareServiceProvider = value;
      }

      /// <summary>
      /// A value of CompareOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("compareOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment CompareOfficeCaseloadAssignment
      {
        get => compareOfficeCaseloadAssignment ??= new();
        set => compareOfficeCaseloadAssignment = value;
      }

      private OfficeServiceProvider compareOfficeServiceProvider;
      private Program compareProgram;
      private Tribunal compareTribunal;
      private ServiceProvider compareServiceProvider;
      private OfficeCaseloadAssignment compareOfficeCaseloadAssignment;
    }

    /// <summary>A SwapGroup group.</summary>
    [Serializable]
    public class SwapGroup
    {
      /// <summary>
      /// A value of SwapOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("swapOfficeServiceProvider")]
      public OfficeServiceProvider SwapOfficeServiceProvider
      {
        get => swapOfficeServiceProvider ??= new();
        set => swapOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of SwapProgram.
      /// </summary>
      [JsonPropertyName("swapProgram")]
      public Program SwapProgram
      {
        get => swapProgram ??= new();
        set => swapProgram = value;
      }

      /// <summary>
      /// A value of SwapTribunal.
      /// </summary>
      [JsonPropertyName("swapTribunal")]
      public Tribunal SwapTribunal
      {
        get => swapTribunal ??= new();
        set => swapTribunal = value;
      }

      /// <summary>
      /// A value of SwapServiceProvider.
      /// </summary>
      [JsonPropertyName("swapServiceProvider")]
      public ServiceProvider SwapServiceProvider
      {
        get => swapServiceProvider ??= new();
        set => swapServiceProvider = value;
      }

      /// <summary>
      /// A value of SwapOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("swapOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment SwapOfficeCaseloadAssignment
      {
        get => swapOfficeCaseloadAssignment ??= new();
        set => swapOfficeCaseloadAssignment = value;
      }

      private OfficeServiceProvider swapOfficeServiceProvider;
      private Program swapProgram;
      private Tribunal swapTribunal;
      private ServiceProvider swapServiceProvider;
      private OfficeCaseloadAssignment swapOfficeCaseloadAssignment;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
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
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of OfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("officeCaseloadAssignment")]
      public OfficeCaseloadAssignment OfficeCaseloadAssignment
      {
        get => officeCaseloadAssignment ??= new();
        set => officeCaseloadAssignment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OfficeServiceProvider officeServiceProvider;
      private Program program;
      private Tribunal tribunal;
      private ServiceProvider serviceProvider;
      private OfficeCaseloadAssignment officeCaseloadAssignment;
    }

    /// <summary>
    /// Gets a value of Initialized.
    /// </summary>
    [JsonIgnore]
    public Array<InitializedGroup> Initialized => initialized ??= new(
      InitializedGroup.Capacity);

    /// <summary>
    /// Gets a value of Initialized for json serialization.
    /// </summary>
    [JsonPropertyName("initialized")]
    [Computed]
    public IList<InitializedGroup> Initialized_Json
    {
      get => initialized;
      set => Initialized.Assign(value);
    }

    /// <summary>
    /// A value of Swap1.
    /// </summary>
    [JsonPropertyName("swap1")]
    public Common Swap1
    {
      get => swap1 ??= new();
      set => swap1 = value;
    }

    /// <summary>
    /// Gets a value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public CompareGroup Compare
    {
      get => compare ?? (compare = new());
      set => compare = value;
    }

    /// <summary>
    /// Gets a value of Swap.
    /// </summary>
    [JsonPropertyName("swap")]
    public SwapGroup Swap
    {
      get => swap ?? (swap = new());
      set => swap = value;
    }

    /// <summary>
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public Common J
    {
      get => j ??= new();
      set => j = value;
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of InitTribunal.
    /// </summary>
    [JsonPropertyName("initTribunal")]
    public Tribunal InitTribunal
    {
      get => initTribunal ??= new();
      set => initTribunal = value;
    }

    /// <summary>
    /// A value of InitProgram.
    /// </summary>
    [JsonPropertyName("initProgram")]
    public Program InitProgram
    {
      get => initProgram ??= new();
      set => initProgram = value;
    }

    /// <summary>
    /// A value of InitOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("initOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment InitOfficeCaseloadAssignment
    {
      get => initOfficeCaseloadAssignment ??= new();
      set => initOfficeCaseloadAssignment = value;
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
    /// A value of HiOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("hiOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment HiOfficeCaseloadAssignment
    {
      get => hiOfficeCaseloadAssignment ??= new();
      set => hiOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of LowOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("lowOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment LowOfficeCaseloadAssignment
    {
      get => lowOfficeCaseloadAssignment ??= new();
      set => lowOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of HiServiceProvider.
    /// </summary>
    [JsonPropertyName("hiServiceProvider")]
    public ServiceProvider HiServiceProvider
    {
      get => hiServiceProvider ??= new();
      set => hiServiceProvider = value;
    }

    /// <summary>
    /// A value of LowServiceProvider.
    /// </summary>
    [JsonPropertyName("lowServiceProvider")]
    public ServiceProvider LowServiceProvider
    {
      get => lowServiceProvider ??= new();
      set => lowServiceProvider = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

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
    /// A value of ActiveOffice.
    /// </summary>
    [JsonPropertyName("activeOffice")]
    public Common ActiveOffice
    {
      get => activeOffice ??= new();
      set => activeOffice = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private Array<InitializedGroup> initialized;
    private Common swap1;
    private CompareGroup compare;
    private SwapGroup swap;
    private Common j;
    private Common i;
    private Array<LocalGroup> local1;
    private Tribunal tribunal;
    private Tribunal initTribunal;
    private Program initProgram;
    private OfficeCaseloadAssignment initOfficeCaseloadAssignment;
    private DateWorkArea current;
    private OfficeCaseloadAssignment hiOfficeCaseloadAssignment;
    private OfficeCaseloadAssignment lowOfficeCaseloadAssignment;
    private ServiceProvider hiServiceProvider;
    private ServiceProvider lowServiceProvider;
    private Common promptCount;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Program program;
    private ServiceProvider serviceProvider;
    private Common activeOffice;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of OfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("officeAssignmentPlan")]
    public OfficeAssignmentPlan OfficeAssignmentPlan
    {
      get => officeAssignmentPlan ??= new();
      set => officeAssignmentPlan = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    private Tribunal tribunal;
    private OfficeAssignmentPlan officeAssignmentPlan;
    private Code code;
    private CodeValue codeValue;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private Program program;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }
#endregion
}

// Program: SI_RMAD_ROLE_MAINT_FOR_DISB, ID: 373475206, model: 746.
// Short name: SWERMADP
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
/// <para>
/// A program: SI_RMAD_ROLE_MAINT_FOR_DISB.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists, adds, and updates the role of CSE PERSONs on a 
/// specified CASE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiRmadRoleMaintForDisb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_ROLE_MAINT_FOR_DISB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadRoleMaintForDisb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadRoleMaintForDisb.
  /// </summary>
  public SiRmadRoleMaintForDisb(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------------------------------
    //                                
    // M A I N T E N A N C E   L O G
    // ----------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ------------------------------------------------------------
    // 04/06/01  Marek Lachowicz		Initial Code
    // 02/06/03  GVandy	PR169107	Corrected logic which identifies overlaps.
    // 02/06/03  GVandy	PR 168472	Modify to alleviate the following scenario:  
    // The case
    // 					contains 1 or more additional children who are on cases not
    // 					in common with the child listed in the header on RMAD.
    // 					If there is an overlap with the dates of one of these non
    // 					common cases the user cannot fix the dates because they
    // 					don't appear on the screen.  In this scenario we want to
    // 					allow the save to continue but give a message indicating that
    // 					the user should check the dates on other cases for each child.
    // 02/10/03  GVandy	PR170301	Call the security cab.
    // 07/25/03  GVandy	PR183805	Correct -811 abend.
    // 12/13/07  Arun Mathias  CQ#408          Do not allow Start date to be 
    // greater than end date or current date
    // 02/04/08  Arun Mathias  CQ#2715         Do not allow AR end date to be 
    // less than the case end date
    // 03/14/08  Arun Mathias  CQ#2715         Backed out the changes
    // 03/17/08  Arun Mathias  CQ#2827         Do not allow to Add a person past
    // the case closure date
    // ----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.ErrOnAdabasUnavailable.Flag = "Y";
    local.Current.Timestamp = Now();

    // 05/23/00 M.L End
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.PageNumber.PageNumber = import.PageNumber.PageNumber;

    for(import.Display.Index = 0; import.Display.Index < import.Display.Count; ++
      import.Display.Index)
    {
      if (!import.Display.CheckSize())
      {
        break;
      }

      export.Display.Index = import.Display.Index;
      export.Display.CheckSize();

      MoveServiceProvider(import.Display.Item.DisplayServiceProvider,
        export.Display.Update.DisplayServiceProvider);
      export.Display.Update.DisplayOffice.Name =
        import.Display.Item.DisplayOffice.Name;
      export.Display.Update.DisplayRowOperation.OneChar =
        import.Display.Item.DisplayRowOperation.OneChar;
      export.Display.Update.DisplayRowNumber.Count =
        import.Display.Item.DisplayRowNumber.Count;
      export.Display.Update.DisplayCommon.SelectChar =
        import.Display.Item.DisplayCommon.SelectChar;
      MoveCase1(import.Display.Item.DisplayCase,
        export.Display.Update.DisplayCase);
      MoveCsePersonsWorkSet2(import.Display.Item.DisplayCsePersonsWorkSet,
        export.Display.Update.DisplayCsePersonsWorkSet);
      export.Display.Update.DisplayCaseRole.Assign(
        import.Display.Item.DisplayCaseRole);

      if (Equal(export.Display.Item.DisplayCaseRole.StartDate, local.Null1.Date))
        
      {
      }
      else
      {
        if (Equal(export.Display.Item.DisplayCaseRole.EndDate, local.Null1.Date))
          
        {
          var field = GetField(export.Display.Item.DisplayCaseRole, "endDate");

          field.Color = "cyan";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.Display.Item.DisplayCaseRole, "endDate");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
        }

        var field1 = GetField(export.Display.Item.DisplayCaseRole, "startDate");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;

        var field2 = GetField(export.Display.Item.DisplayCommon, "selectChar");

        field2.Color = "green";
        field2.Highlighting = Highlighting.Underscore;
        field2.Protected = false;
      }
    }

    import.Display.CheckIndex();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      MoveServiceProvider(import.Import1.Item.AllServiceProvider,
        export.Export1.Update.AllServiceProvider);
      export.Export1.Update.AllOffice.Name = import.Import1.Item.AllOffice.Name;
      export.Export1.Update.AllExportRowOperation.OneChar =
        import.Import1.Item.AllImportRowOperation.OneChar;
      export.Export1.Update.AllExportRowNumber.Count =
        import.Import1.Item.AllImportRowNumber.Count;
      export.Export1.Update.AllExportRowIndicator.SelectChar =
        import.Import1.Item.AllImportRowIndicator.SelectChar;
      MoveCase1(import.Import1.Item.AllCase, export.Export1.Update.AllCase);
      MoveCsePersonsWorkSet2(import.Import1.Item.AllCsePersonsWorkSet,
        export.Export1.Update.AllCsePersonsWorkSet);
      export.Export1.Update.AllCaseRole.Assign(import.Import1.Item.AllCaseRole);
    }

    import.Import1.CheckIndex();

    for(import.HiddenDisplay.Index = 0; import.HiddenDisplay.Index < import
      .HiddenDisplay.Count; ++import.HiddenDisplay.Index)
    {
      if (!import.HiddenDisplay.CheckSize())
      {
        break;
      }

      export.HiddenDisplay.Index = import.HiddenDisplay.Index;
      export.HiddenDisplay.CheckSize();

      MoveCaseRole3(import.HiddenDisplay.Item.HiddenDisplayRole,
        export.HiddenDisplay.Update.HiddenDisplayRole);
    }

    import.HiddenDisplay.CheckIndex();
    export.NewCaseRole.Assign(import.NewCaseRole);
    export.NewCsePersonsWorkSet.Assign(import.NewCsePersonsWorkSet);
    MoveCsePersonsWorkSet2(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.Standard.ScrollingMessage = import.Standard.ScrollingMessage;
    export.CaseRoleText.Text2 = import.CaseRoleText.Text2;
    export.BeenToName.Flag = import.BeenToName.Flag;
    export.GapCase.Number = import.GapCase.Number;
    export.GapCommon.Flag = import.GapCommon.Flag;
    export.Response.Flag = import.Response.Flag;
    export.Message.Text60 = import.Message.Text60;
    export.NewSsnWorkArea.Assign(import.NewSsnWorkArea);
    export.Prompt.SelectChar = import.Prompt.SelectChar;
    export.CaseRole.Type1 = import.CaseRole.Type1;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "REMOVE") || Equal(global.Command, "SAVE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "RETNAME") || Equal(global.Command, "RETORGZ"))
    {
      export.BeenToName.Flag = "Y";

      if (!IsEmpty(export.NewCsePersonsWorkSet.Number))
      {
        local.NewSsnWorkArea.SsnText9 = export.NewCsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();

        return;
      }
      else
      {
        // ---------------------------------------------
        // W.Campbell 9/18/98 - Added the following
        // move statement in order to reinitialize the
        // new case_role attributes on the screen.
        // ---------------------------------------------
        export.NewCaseRole.Assign(local.BlankCaseRole);
        export.NewSsnWorkArea.SsnNumPart1 = 0;
        export.NewSsnWorkArea.SsnNumPart2 = 0;
        export.NewSsnWorkArea.SsnNumPart3 = 0;

        return;
      }
    }

    if (AsChar(export.GapCommon.Flag) == 'Y')
    {
      if (!Equal(global.Command, "SAVE"))
      {
        var field = GetField(export.Response, "flag");

        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          var field1 =
            GetField(export.Display.Item.DisplayCaseRole, "startDate");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Display.Item.DisplayCaseRole, "endDate");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Display.Item.DisplayCommon, "selectChar");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        export.Display.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
        ExitState = "SI000_RMAD_GAP_CONFIRMATION";

        return;
      }

      // ** Added Else section on 01/10/08 **
      // *** Changes Begin Here on 01/10/08 ***
    }
    else
    {
      export.Message.Text60 = "";

      // *** Changes End   Here on 01/10/08 ***
    }

    if (Equal(global.Command, "REMOVE"))
    {
      local.RowCounter.Count = 0;

      for(export.Display.Index = 0; export.Display.Index < export
        .Display.Count; ++export.Display.Index)
      {
        if (!export.Display.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Display.Item.DisplayCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (Equal(export.Display.Item.DisplayCaseRole.Type1, "FA") || Equal
              (export.Display.Item.DisplayCaseRole.Type1, "MO"))
            {
              ++local.RowCounter.Count;
            }
            else
            {
              var field1 =
                GetField(export.Display.Item.DisplayCaseRole, "type1");

              field1.Error = true;

              ExitState = "SI0000_ROLE_NOT_ALLOWED_TO_DEL";
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field =
              GetField(export.Display.Item.DisplayCommon, "selectChar");

            field.Error = true;

            break;
        }
      }

      export.Display.CheckIndex();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.RowCounter.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      for(export.Display.Index = 0; export.Display.Index < export
        .Display.Count; ++export.Display.Index)
      {
        if (!export.Display.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Display.Item.DisplayCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (export.Display.Item.DisplayCaseRole.Identifier > 0)
            {
              if (ReadCaseRole4())
              {
                // SAN 9/14/06  ADDED LOGIC TO GIVE AN ERROR MESSAGE RATHER THAN
                // ABEND BECAUSE YOU CANNOT DELETE A CASE ROLE WHEN GENETIC
                // TESTING EXISTS. ----- START
                if (Equal(entities.Delete.Type1, "MO"))
                {
                  if (ReadMother())
                  {
                    ExitState = "AC0_SI0000_CANNOT_DELETE_CASE_RO";

                    return;
                  }
                }

                if (Equal(entities.Delete.Type1, "CH"))
                {
                  if (ReadChild())
                  {
                    ExitState = "AC0_SI0000_CANNOT_DELETE_CASE_RO";

                    return;
                  }
                }

                // SAN 9/14/06  ADDED LOGIC TO GIVE AN ERROR MESSAGE RATHER THAN
                // ABEND BECAUSE YOU CANNOT DELETE A CASE ROLE WHEN GENETIC
                // TESTING EXISTS. ----- END
                DeleteCaseRole();
              }
            }

            break;
          default:
            break;
        }
      }

      export.Display.CheckIndex();
      local.SucessfullyDeleted.Flag = "Y";
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "RTLIST":
        export.CaseRole.Type1 = import.CodeValue.Cdvalue;
        export.CaseRoleText.Text2 = import.CodeValue.Cdvalue;

        return;
      case "ORGZ":
        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          if (IsEmpty(export.Display.Item.DisplayCommon.SelectChar))
          {
          }
          else
          {
            var field =
              GetField(export.Display.Item.DisplayCommon, "selectChar");

            field.Error = true;

            ExitState = "DO_NOT_MAKE_SELECTION_WITH_OPT";

            return;
          }
        }

        export.Display.CheckIndex();
        ExitState = "ECO_LNK_TO_ORGZ";

        return;
      case "SAVE":
        if (AsChar(export.GapCommon.Flag) == 'Y')
        {
          switch(AsChar(export.Response.Flag))
          {
            case 'Y':
              export.Message.Text60 = "";
              export.Response.Flag = "";
              export.GapCommon.Flag = "";
              export.GapCase.Number = "";

              break;
            case 'N':
              export.Message.Text60 = "";
              export.Response.Flag = "";
              export.GapCommon.Flag = "";
              export.GapCase.Number = "";
              ExitState = "SI0000_SAVE_NOT_PROCESSED";

              return;
            default:
              var field = GetField(export.Response, "flag");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;

              for(export.Display.Index = 0; export.Display.Index < export
                .Display.Count; ++export.Display.Index)
              {
                if (!export.Display.CheckSize())
                {
                  break;
                }

                var field1 =
                  GetField(export.Display.Item.DisplayCaseRole, "startDate");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Display.Item.DisplayCaseRole, "endDate");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.Display.Item.DisplayCommon, "selectChar");

                field3.Color = "cyan";
                field3.Protected = true;
              }

              export.Display.CheckIndex();
              ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

              return;
          }
        }

        UseSiRmadSaveData();

        if (AsChar(local.ArGapCommon.Flag) == 'Y')
        {
          export.GapCommon.Flag = "Y";
          export.GapCase.Number = local.ArGapCase.Number;
          export.Message.Text60 =
            "There are gaps in AR time frames. Do you want to continue ?";

          var field = GetField(export.Response, "flag");

          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            var field1 =
              GetField(export.Display.Item.DisplayCaseRole, "startDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Display.Item.DisplayCaseRole, "endDate");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Display.Item.DisplayCommon, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          export.Display.CheckIndex();

          return;
        }

        if (local.ErrorNumber.Count == 0)
        {
          local.Validated.Number = "";
          export.Export1.Index = 0;

          for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
            export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (Equal(local.Validated.Number, export.Export1.Item.AllCase.Number))
              
            {
            }
            else
            {
              local.Validated.Number = export.Export1.Item.AllCase.Number;
              local.Header.Number = export.CsePersonsWorkSet.Number;
              UseSiRmadForChWithDiffAr();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }
            }
          }

          export.Export1.CheckIndex();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ErrorRowNumber.Count = 0;

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (Equal(export.Export1.Item.AllCase.Number,
                local.Validated.Number) && Equal
                (export.Export1.Item.AllCsePersonsWorkSet.Number,
                local.InvalidChCsePerson.Number) && export
                .Export1.Item.AllCaseRole.Identifier == local
                .InvalidChCaseRole.Identifier && Equal
                (export.Export1.Item.AllCaseRole.Type1, "CH"))
              {
                local.ErrorRowNumber.Count =
                  export.Export1.Item.AllExportRowNumber.Count;
              }
            }

            export.Export1.CheckIndex();

            if (local.ErrorRowNumber.Count == 0)
            {
              UseEabRollbackCics();

              return;
            }

            local.PageNumber.PageNumber = 1;

            export.Export1.Index = 0;
            export.Export1.CheckSize();

            local.Prev.Number = export.Export1.Item.AllCase.Number;
            export.Export1.Index = 0;

            for(var limit = local.ErrorRowNumber.Count; export.Export1.Index < limit
              ; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              ++local.RowCounter.Count;

              if (!Equal(local.Prev.Number, export.Export1.Item.AllCase.Number))
              {
                local.Prev.Number = export.Export1.Item.AllCase.Number;
                ++local.RowCounter.Count;
              }

              if (local.RowCounter.Count > Export.DisplayGroup.Capacity)
              {
                ++local.PageNumber.PageNumber;
                local.RowCounter.Count -= Export.DisplayGroup.Capacity;
              }
            }

            export.Export1.CheckIndex();
            export.PageNumber.PageNumber = local.PageNumber.PageNumber;
            UseSiRmadDisplayPage();

            for(export.Display.Index = 0; export.Display.Index < export
              .Display.Count; ++export.Display.Index)
            {
              if (!export.Display.CheckSize())
              {
                break;
              }

              if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
                local.Null1.Date))
              {
                var field1 =
                  GetField(export.Display.Item.DisplayCaseRole, "startDate");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Display.Item.DisplayCaseRole, "endDate");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.Display.Item.DisplayCommon, "selectChar");

                field3.Color = "cyan";
                field3.Protected = true;
              }
              else
              {
                if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
                  local.Null1.Date))
                {
                  var field =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field.Color = "white";
                  field.Highlighting = Highlighting.Underscore;
                  field.Protected = true;
                }
                else
                {
                  var field =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field.Color = "green";
                  field.Highlighting = Highlighting.Underscore;
                  field.Protected = false;
                }

                if (export.Display.Item.DisplayRowNumber.Count == local
                  .ErrorRowNumber.Count)
                {
                  if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
                    local.Null1.Date))
                  {
                    var field =
                      GetField(export.Display.Item.DisplayCaseRole, "endDate");

                    field.Color = "cyan";
                    field.Highlighting = Highlighting.Underscore;
                    field.Protected = true;
                  }
                  else
                  {
                    var field =
                      GetField(export.Display.Item.DisplayCaseRole, "endDate");

                    field.Error = true;
                  }

                  var field1 =
                    GetField(export.Display.Item.DisplayCaseRole, "startDate");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Display.Item.DisplayCommon, "selectChar");

                  field2.Color = "green";
                  field2.Highlighting = Highlighting.Underscore;
                  field2.Protected = false;
                  field2.Focused = true;
                }
                else
                {
                  if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
                    local.Null1.Date))
                  {
                    var field =
                      GetField(export.Display.Item.DisplayCaseRole, "endDate");

                    field.Color = "cyan";
                    field.Highlighting = Highlighting.Underscore;
                    field.Protected = true;
                  }
                  else
                  {
                    var field =
                      GetField(export.Display.Item.DisplayCaseRole, "endDate");

                    field.Color = "green";
                    field.Highlighting = Highlighting.Underscore;
                    field.Protected = false;
                  }

                  var field1 =
                    GetField(export.Display.Item.DisplayCaseRole, "startDate");

                  field1.Color = "green";
                  field1.Highlighting = Highlighting.Underscore;
                  field1.Protected = false;

                  var field2 =
                    GetField(export.Display.Item.DisplayCommon, "selectChar");

                  field2.Color = "green";
                  field2.Highlighting = Highlighting.Underscore;
                  field2.Protected = false;
                }
              }

              export.HiddenDisplay.Index = export.HiddenDisplay.Index;
              export.HiddenDisplay.CheckSize();

              MoveCaseRole3(export.Display.Item.DisplayCaseRole,
                export.HiddenDisplay.Update.HiddenDisplayRole);
            }

            export.Display.CheckIndex();
            UseEabRollbackCics();

            return;
          }

          local.SavingOk.Flag = "Y";
          global.Command = "DISPLAY";
        }
        else
        {
          switch(local.ErrorNumber.Count)
          {
            case 7:
              break;
            case 6:
              ExitState = "SI0000_NO_ROLES_BEFORE_AR";

              break;
            case 5:
              ExitState = "SI0000_NO_CHILD_ROLE";

              break;
            case 1:
              ExitState = "SI000_OVERLAPPINNG_TIMEFRAME";

              break;
            case 2:
              ExitState = "SI000_OVERLAPPING_ARS";

              break;
            case 3:
              ExitState = "SI000_ROLES_WITHOUT_ARS";

              break;
            case 4:
              break;
            default:
              break;
          }

          local.PageNumber.PageNumber = 1;

          export.Export1.Index = 0;
          export.Export1.CheckSize();

          local.Prev.Number = export.Export1.Item.AllCase.Number;
          export.Export1.Index = 0;

          for(var limit = local.ErrorRowNumber.Count; export.Export1.Index < limit
            ; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            ++local.RowCounter.Count;

            if (!Equal(local.Prev.Number, export.Export1.Item.AllCase.Number))
            {
              local.Prev.Number = export.Export1.Item.AllCase.Number;
              ++local.RowCounter.Count;
            }

            if (local.RowCounter.Count > Export.DisplayGroup.Capacity)
            {
              ++local.PageNumber.PageNumber;
              local.RowCounter.Count -= Export.DisplayGroup.Capacity;
            }
          }

          export.Export1.CheckIndex();
          export.PageNumber.PageNumber = local.PageNumber.PageNumber;
          UseSiRmadDisplayPage();

          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
              local.Null1.Date))
            {
              var field1 =
                GetField(export.Display.Item.DisplayCaseRole, "startDate");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Display.Item.DisplayCommon, "selectChar");

              field3.Color = "cyan";
              field3.Protected = true;
            }
            else
            {
              if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
                local.Null1.Date))
              {
                var field =
                  GetField(export.Display.Item.DisplayCaseRole, "endDate");

                field.Color = "white";
                field.Highlighting = Highlighting.Underscore;
                field.Protected = true;
              }
              else
              {
                var field =
                  GetField(export.Display.Item.DisplayCaseRole, "endDate");

                field.Color = "green";
                field.Highlighting = Highlighting.Underscore;
                field.Protected = false;
              }

              if (export.Display.Item.DisplayRowNumber.Count == local
                .ErrorRowNumber.Count)
              {
                if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
                  local.Null1.Date))
                {
                  var field =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field.Color = "cyan";
                  field.Highlighting = Highlighting.Underscore;
                  field.Protected = true;
                }
                else
                {
                  var field =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field.Error = true;
                }

                var field1 =
                  GetField(export.Display.Item.DisplayCaseRole, "startDate");

                field1.Error = true;

                var field2 =
                  GetField(export.Display.Item.DisplayCommon, "selectChar");

                field2.Color = "green";
                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;
                field2.Focused = true;
              }
              else
              {
                if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
                  local.Null1.Date))
                {
                  var field =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field.Color = "cyan";
                  field.Highlighting = Highlighting.Underscore;
                  field.Protected = true;
                }
                else
                {
                  var field =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field.Color = "green";
                  field.Highlighting = Highlighting.Underscore;
                  field.Protected = false;
                }

                var field1 =
                  GetField(export.Display.Item.DisplayCaseRole, "startDate");

                field1.Color = "green";
                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;

                var field2 =
                  GetField(export.Display.Item.DisplayCommon, "selectChar");

                field2.Color = "green";
                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;
              }
            }

            export.HiddenDisplay.Index = export.HiddenDisplay.Index;
            export.HiddenDisplay.CheckSize();

            MoveCaseRole3(export.Display.Item.DisplayCaseRole,
              export.HiddenDisplay.Update.HiddenDisplayRole);
          }

          export.Display.CheckIndex();

          return;
        }

        break;
      case "ADD":
        if (AsChar(export.BeenToName.Flag) != 'Y')
        {
          ExitState = "NAME_SEARCH_REQUIRED";

          return;
        }

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Display.Item.DisplayCommon.SelectChar))
          {
            case 'S':
              ++local.NumberSelectedRows.Count;

              if (local.NumberSelectedRows.Count > 1)
              {
                ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

                var field1 =
                  GetField(export.Display.Item.DisplayCommon, "selectChar");

                field1.Error = true;

                return;
              }

              MoveCase1(export.Display.Item.DisplayCase, local.NewCase);
              MoveServiceProvider(export.Display.Item.DisplayServiceProvider,
                local.NewServiceProvider);
              local.NewOffice.Name = export.Display.Item.DisplayOffice.Name;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field =
                GetField(export.Display.Item.DisplayCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

        export.Display.CheckIndex();

        if (local.NumberSelectedRows.Count == 0)
        {
          ExitState = "SI0000_CASE_SELECTION_REQUIRED";

          return;
        }

        if (IsEmpty(export.NewCsePersonsWorkSet.Number))
        {
          // ------------------------------------------------------------
          // If this is a new person, validate Last Name, First Name,
          // Gender, and DOB.
          // ------------------------------------------------------------
          // ------------------
          // Last Name Required
          // ------------------
          if (IsEmpty(export.NewCsePersonsWorkSet.LastName))
          {
            var field = GetField(export.NewCsePersonsWorkSet, "lastName");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          // -------------------
          // First Name Required
          // -------------------
          if (IsEmpty(export.NewCsePersonsWorkSet.FirstName))
          {
            var field = GetField(export.NewCsePersonsWorkSet, "firstName");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          // 11/04/2002 M.Lachowicz Start
          UseSiCheckName();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.NewCsePersonsWorkSet, "lastName");

            field1.Error = true;

            var field2 = GetField(export.NewCsePersonsWorkSet, "firstName");

            field2.Error = true;

            var field3 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

            field3.Error = true;

            ExitState = "SI0001_INVALID_NAME";

            return;
          }

          // 11/04/2002 M.Lachowicz End
          // ------------------
          // Validate Gender
          // ------------------
          switch(AsChar(export.NewCsePersonsWorkSet.Sex))
          {
            case 'M':
              break;
            case 'F':
              break;
            default:
              var field = GetField(export.NewCsePersonsWorkSet, "sex");

              field.Error = true;

              ExitState = "INVALID_SEX";

              return;
          }

          // ------------------
          // Validate DOB
          // ------------------
          if (Lt(local.Current.Date, export.NewCsePersonsWorkSet.Dob))
          {
            var field = GetField(export.NewCsePersonsWorkSet, "dob");

            field.Error = true;

            ExitState = "INVALID_DATE_OF_BIRTH";

            return;
          }
        }

        if (Equal(import.NewCaseRole.StartDate, local.Null1.Date))
        {
          ExitState = "SI0000_ENTER_START_DATE";

          var field = GetField(export.NewCaseRole, "startDate");

          field.Error = true;

          return;
        }

        if (Equal(import.NewCaseRole.EndDate, local.Null1.Date))
        {
          ExitState = "SI0000_ENTER_END_DATE";

          var field = GetField(export.NewCaseRole, "endDate");

          field.Error = true;

          return;
        }

        if (Lt(local.Current.Date, export.NewCaseRole.EndDate))
        {
          var field = GetField(export.NewCaseRole, "endDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (Lt(local.Current.Date, export.NewCaseRole.StartDate))
        {
          var field = GetField(export.NewCaseRole, "startDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (Lt(export.NewCaseRole.EndDate, export.NewCaseRole.StartDate))
        {
          var field1 = GetField(export.NewCaseRole, "startDate");

          field1.Error = true;

          var field2 = GetField(export.NewCaseRole, "endDate");

          field2.Error = true;

          ExitState = "START_DATE_GREATER_THAN_END_DATE";

          return;
        }

        if (!Equal(export.NewCsePersonsWorkSet.Dob, local.Null1.Date))
        {
          if (Lt(export.NewCaseRole.StartDate, export.NewCsePersonsWorkSet.Dob))
          {
            var field = GetField(export.NewCaseRole, "startDate");

            field.Error = true;

            ExitState = "START_DT_BEFORE_DOB";

            return;
          }
        }

        // ** CQ#2827 Do not allow add persons after the case closure date **
        // **** CQ#2827 Changes Begin Here  ****
        if (AsChar(local.NewCase.Status) == 'C')
        {
          if (ReadCase2())
          {
            if (Lt(entities.Case1.StatusDate, export.NewCaseRole.EndDate))
            {
              ExitState = "END_DATE_GREATER_CASE_END_DATE";

              var field = GetField(export.NewCaseRole, "endDate");

              field.Error = true;

              return;
            }
          }
          else
          {
            ExitState = "CASE_NF";

            return;
          }
        }

        // **** CQ#2827 Changes End   Here  ****
        // ---------------------------
        // Validate Case Role
        // ---------------------------
        switch(TrimEnd(export.NewCaseRole.Type1))
        {
          case "AR":
            break;
          case "AP":
            break;
          case "CH":
            break;
          case "MO":
            // ------------------------------------------------
            // Verify mother is not Male
            // ------------------------------------------------
            if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'M' || IsEmpty
              (export.NewCsePersonsWorkSet.Sex))
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "MOTHER_MUST_BE_FEMALE";

              return;
            }

            // ------------------------------------------------
            // Verify only one mother active on the case
            // ------------------------------------------------
            UseSiVerifyOneActiveCaseRole2();

            if (AsChar(local.ErrorCommon.Flag) == 'Y')
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "INVALID_NUMBER_OF_MOTHERS";

              return;
            }

            break;
          case "FA":
            // ------------------------------------------------
            // Verify father is not female
            // ------------------------------------------------
            if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'F' || IsEmpty
              (export.NewCsePersonsWorkSet.Sex))
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "FATHER_MUST_BE_MALE";

              return;
            }

            // ------------------------------------------------
            // Verify only one father active on the case
            // ------------------------------------------------
            UseSiVerifyOneActiveCaseRole2();

            if (AsChar(local.ErrorCommon.Flag) == 'Y')
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "INVALID_NUMBER_OF_FATHERS";

              return;
            }

            break;
          default:
            var field = GetField(export.NewCaseRole, "type1");

            field.Error = true;

            ExitState = "INVALID_ROLE";

            return;
        }

        MoveSsnWorkArea(export.NewSsnWorkArea, local.NewSsnWorkArea);
        local.NewSsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.NewSsnWorkArea.SsnText9))
        {
          export.NewCsePersonsWorkSet.Ssn = export.NewSsnWorkArea.SsnText9;
        }
        else
        {
          export.NewCsePersonsWorkSet.Ssn = "000000000";
        }

        if (!IsEmpty(export.NewCsePersonsWorkSet.Number))
        {
          local.CsePerson.Number = export.NewCsePersonsWorkSet.Number;

          if (CharAt(local.CsePerson.Number, 10) != 'O')
          {
            // ------------------------------------------------
            // 01/29/99 W.Campbell - Added a local view
            // (local_cse ief_supplied flag) to allow for the saving and
            // passing of a flag to indicate that the person being passed
            // is a non-case related CSE person (flag = N).  This
            // flag is being passed to SI_VERIFY_AND_CREATE_CSE_PERSON
            // used in the logic below.  Work done on IDCR477.
            // The flag is view matched to the export view below
            // which is obtained from CAB_READ_ADABAS_PERSON.
            // ------------------------------------------------
            UseCabReadAdabasPerson1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (!Equal(export.NewCsePersonsWorkSet.Ssn,
              export.NewSsnWorkArea.SsnText9))
            {
              local.NewSsnWorkArea.SsnText9 = export.NewCsePersonsWorkSet.Ssn;
              UseCabSsnConvertTextToNum();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            if (Equal(export.NewCaseRole.Type1, "CH"))
            {
              // ------------------------------------------------------------
              // Person can be an Active Child only on two cases.
              // ------------------------------------------------------------
              UseSiVerifyChildIsOnACase();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              // ** CQ#381 Commented the below code and moved at the end so that
              // it would be an warning message instead of error message **
              // *** Changes Begin Here 01/10/08 ***
              // *** Changes End   Here 01/10/08 ***
            }

            switch(TrimEnd(export.NewCaseRole.Type1))
            {
              case "AR":
                // ------------------------------------------------------------
                // AR can be an AP on this case, but the AP role will be end
                // date when the AR role is created.
                // ------------------------------------------------------------
                // ------------------------------------------------------------
                // AR can be a Child on this case, but the CH role will be end
                // dated when the AR role is created.  Check needs to be
                // performed to determine if there is at least one other active
                // CH role before continuing.
                // ------------------------------------------------------------
                break;
              case "AP":
                // -----------------------------------------------
                // AP cannot be a Child on this case.
                // -----------------------------------------------
                if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'F')
                {
                  // -----------------------------------------------
                  // If AP is female, no other APs allowed.
                  // -----------------------------------------------
                  local.CaseRole.Type1 = "AP";
                  UseSiVerifyOneActiveCaseRole1();

                  if (AsChar(local.ErrorCommon.Flag) == 'Y')
                  {
                    var field = GetField(export.NewCaseRole, "type1");

                    field.Error = true;

                    ExitState = "INVALID_AP_MALE_AND_FEMALE";

                    return;
                  }
                }

                // -----------------------------------------------
                // 02/12/99 W.Campbell - New Code added.
                // The following READ was added to add the
                // validation that if the MO is the AP then no
                // other AP's are allowed.
                // -----------------------------------------------
                if (ReadCaseRole2())
                {
                  // Currency on the MOther Case Role is acquired.
                  // 06/23/99 M.L
                  //              Change property of the following READ to 
                  // generate
                  //              SELECT ONLY
                  if (ReadCsePerson2())
                  {
                    // The new incoming AP Case Role is the
                    // same CSE Person playing the MOther
                    // Case Role for this Case.
                    // Allow creation of the new AP.
                  }
                  else
                  {
                    // 06/23/99 M.L
                    //              Change property of the following READ to 
                    // generate
                    //              SELECT ONLY
                    if (ReadCsePerson3())
                    {
                      if (ReadCaseRole1())
                      {
                        var field = GetField(export.NewCaseRole, "type1");

                        field.Error = true;

                        ExitState = "INVALID_MO_AND_MULTIPLE_AP";

                        return;
                      }
                    }
                    else
                    {
                      UseEabRollbackCics();
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      return;
                    }
                  }
                }

                // -----------------------------------------------
                // 02/12/99 W.Campbell - End of new code added.
                // -----------------------------------------------
                break;
              case "MO":
                // -----------------------------------------------
                // Mother cannot be a child on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "CH";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                local.CaseRole.Type1 = "AR";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  local.SetNewRelToAr.Flag = "Y";
                }

                break;
              case "FA":
                // -----------------------------------------------
                // Father cannot be a child on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "CH";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                local.SetNewRelToAr.Flag = "Y";

                break;
              case "CH":
                // -----------------------------------------------
                // Child cannot be an AR on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "AR";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // -----------------------------------------------
                // Child cannot be an AP on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "AP";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // -----------------------------------------------
                // Child cannot be a Mother on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "MO";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // -----------------------------------------------
                // Child cannot be a Father on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "FA";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                break;
              default:
                break;
            }

            if (Equal(export.NewCaseRole.Type1, "AR"))
            {
              // Determine if the new AR is currently a child on the Case.  If 
              // so, determine if the child is of age (10) to become the new AR.
              // 07/25/03 GVandy PR183805  Modify properties of read to "Both 
              // Select and Cursor" to correct -811 abend.
              if (ReadCaseRole3())
              {
                if (Equal(export.NewCsePersonsWorkSet.Dob, local.Null1.Date))
                {
                }
                else
                {
                  local.OfAge.Date = AddYears(local.Current.Date, -10);

                  if (!Lt(local.OfAge.Date, export.NewCsePersonsWorkSet.Dob))
                  {
                  }
                  else
                  {
                    ExitState = "SI0000_CH_NOT_OF_AGE_FOR_AR";

                    return;
                  }
                }
              }

              // 10/22/99 M.L - PR#77725 Start
              if (ReadCaseRole5())
              {
                ExitState = "SI0000_AP_CAN_NOT_BECOME_AR";

                return;
              }

              // 10/22/99 M.L - PR#77436 Start
            }
            else
            {
              local.CaseRole.Type1 = "CH";
              UseSiCheckCaseRoleCombinations();

              if (AsChar(local.Common.Flag) == 'Y')
              {
                var field = GetField(export.NewCaseRole, "type1");

                field.Error = true;

                ExitState = "CONFLICT_OF_ROLES";

                return;
              }
            }
          }
          else if (!Equal(export.NewCaseRole.Type1, "AR"))
          {
            var field = GetField(export.NewCaseRole, "type1");

            field.Error = true;

            ExitState = "ORGANIZATION_MUST_BE_AR";

            return;
          }
        }
        else
        {
          if (Equal(export.NewCaseRole.Type1, "AR"))
          {
            // -----------------------------------------------
            // A person must be at least 10 years old to
            // become the AR, as per Cheryl Deghand on 9-16-98.
            // -----------------------------------------------
            if (Equal(export.NewCsePersonsWorkSet.Dob, local.Null1.Date))
            {
            }
            else
            {
              local.OfAge.Date = AddYears(local.Current.Date, -10);

              if (!Lt(local.OfAge.Date, export.NewCsePersonsWorkSet.Dob))
              {
              }
              else
              {
                ExitState = "SI0000_CH_NOT_OF_AGE_FOR_AR";

                return;
              }
            }
          }

          // -----------------------------------------------
          // End of ELSE statement added
          // by W. Campbell on 09/16/98.  The
          // purpose of this logic is to make
          // sure the AR is at least 10 yrs old
          // provided a DOB has been input for
          // the new person.
          // -----------------------------------------------
          // 08/11/2000 M.L Start
          // -----------------------------------------------
          // If father is determined, no other male APs allowed, except a new AP
          // role for the FAther.
          // -----------------------------------------------
          if (Equal(export.NewCaseRole.Type1, "AP") && AsChar
            (export.NewCsePersonsWorkSet.Sex) == 'M')
          {
            MoveCaseRole1(export.NewCaseRole, local.CaseRole);
            local.CaseRole.Type1 = "FA";
            UseSiVerifyOneActiveCaseRole1();

            if (AsChar(local.ErrorCommon.Flag) == 'Y')
            {
              var field = GetField(export.NewCaseRole, "type1");

              field.Error = true;

              ExitState = "INVALID_FA_AND_MULTIPLE_AP";

              return;
            }
          }
        }

        // 09/21/99 M.L Start
        // 09/21/99 M.L End
        // 03/20/00 M.L Start
        switch(TrimEnd(export.NewCaseRole.Type1))
        {
          case "CH":
            break;
          case "AP":
            break;
          case "FA":
            if (ReadCsePerson1())
            {
              ExitState = "SI0000_FATHER_ROLE_NOT_ALLOWED";

              return;
            }

            break;
          default:
            break;
        }

        // 03/20/00 M.L End
        // -----------------------------------------------
        // Create the person on ADABAS and DB2 if necessary
        // -----------------------------------------------
        local.RetreivePersonProgram.Date = export.NewCaseRole.StartDate;

        // ------------------------------------------------
        // 01/29/99 W.Campbell - Added a local view
        // (local_cse ief_supplied flag) to allow for the saving and
        // passing of a flag to indicate that the person being passed
        // is a non-case related CSE person (flag = N).  This
        // flag is being passed to SI_VERIFY_AND_CREATE_CSE_PERSON
        // used in the logic below.  Work done on IDCR477.
        // The flag is view matched to the import view below.
        // ------------------------------------------------
        if (CharAt(export.NewCsePersonsWorkSet.Number, 10) != 'O')
        {
          UseSiVerifyAndCreateCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            UseEabRollbackCics();

            return;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          local.CsePerson.Number = export.NewCsePersonsWorkSet.Number;

          if (!Equal(export.NewCsePersonsWorkSet.Ssn,
            local.NewSsnWorkArea.SsnText9))
          {
            local.NewSsnWorkArea.SsnText9 = export.NewCsePersonsWorkSet.Ssn;
            UseCabSsnConvertTextToNum();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }
          }

          UseSiFormatCsePersonName();
        }

        if (CharAt(local.CsePerson.Number, 10) == 'O')
        {
          UseSiReadCsePerson2();
        }

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          if (Equal(local.NewCase.Number, import.Import1.Item.AllCase.Number))
          {
            local.NewCase.Status = import.Import1.Item.AllCase.Status ?? "";

            break;
          }
        }

        import.Import1.CheckIndex();
        UseSiRmadAddRow();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseSiRmadDisplayPage();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
            local.Null1.Date))
          {
            var field1 =
              GetField(export.Display.Item.DisplayCaseRole, "startDate");

            field1.Color = "cyan";
            field1.Highlighting = Highlighting.Normal;
            field1.Protected = true;

            var field2 =
              GetField(export.Display.Item.DisplayCaseRole, "endDate");

            field2.Color = "cyan";
            field2.Highlighting = Highlighting.Normal;
            field2.Protected = true;

            var field3 =
              GetField(export.Display.Item.DisplayCommon, "selectChar");

            field3.Color = "cyan";
            field3.Highlighting = Highlighting.Normal;
            field3.Protected = true;
          }
          else
          {
            if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "cyan";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = true;
            }
            else
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
            }

            var field1 =
              GetField(export.Display.Item.DisplayCaseRole, "startDate");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;

            var field2 =
              GetField(export.Display.Item.DisplayCommon, "selectChar");

            field2.Color = "green";
            field2.Highlighting = Highlighting.Underscore;
            field2.Protected = false;
          }

          export.HiddenDisplay.Index = export.HiddenDisplay.Index;
          export.HiddenDisplay.CheckSize();

          MoveCaseRole3(export.Display.Item.DisplayCaseRole,
            export.HiddenDisplay.Update.HiddenDisplayRole);
        }

        export.Display.CheckIndex();
        export.NewCaseRole.Type1 = "";
        export.NewCaseRole.EndDate = local.DateWorkArea.Date;
        export.NewCaseRole.StartDate = local.DateWorkArea.Date;
        export.NewCsePersonsWorkSet.Number = "";
        export.NewCsePersonsWorkSet.Ssn = "";
        export.NewSsnWorkArea.SsnNumPart1 = 0;
        export.NewSsnWorkArea.SsnNumPart2 = 0;
        export.NewSsnWorkArea.SsnNumPart3 = 0;
        export.NewCsePersonsWorkSet.FirstName = "";
        export.NewCsePersonsWorkSet.LastName = "";
        export.NewCsePersonsWorkSet.MiddleInitial = "";
        export.NewCsePersonsWorkSet.Sex = "";
        export.NewCsePersonsWorkSet.Dob = local.Default1.Date;

        // ** CQ#381 Moved the code from above so that the message will be a 
        // warning message instead of error message **
        // *** Changes Begin Here 01/10/08 ***
        if (AsChar(local.ActiveCaseCh.Flag) == 'Y')
        {
          if (local.ActiveCaseCh.Count > 1)
          {
            export.Message.Text60 =
              "Person has two active child roles on other cases.";
          }
        }

        // *** Changes End   Here 01/10/08 ***
        ExitState = "SI0000_RMAD_ROW_WAS_ADDED";

        break;
      case "NAME":
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "DELETE":
        local.RowCounter.Count = 0;

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Display.Item.DisplayCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.RowCounter.Count;

              if (AsChar(export.Display.Item.DisplayRowOperation.OneChar) == 'A'
                )
              {
              }
              else
              {
                var field1 =
                  GetField(export.Display.Item.DisplayCommon, "selectChar");

                field1.Error = true;

                var field2 =
                  GetField(export.Display.Item.DisplayCaseRole, "startDate");

                field2.Error = true;

                var field3 =
                  GetField(export.Display.Item.DisplayCaseRole, "endDate");

                field3.Error = true;

                var field4 =
                  GetField(export.Display.Item.DisplayCaseRole, "type1");

                field4.Error = true;

                var field5 =
                  GetField(export.Display.Item.DisplayCsePersonsWorkSet,
                  "number");

                field5.Error = true;

                var field6 =
                  GetField(export.Display.Item.DisplayCsePersonsWorkSet,
                  "formattedName");

                field6.Error = true;

                if (Equal(export.Display.Item.DisplayCaseRole.Type1, "FA") || Equal
                  (export.Display.Item.DisplayCaseRole.Type1, "MO"))
                {
                  ExitState = "AC0_SI0000_USE_PF20_DEL_FA_MO";
                }
                else
                {
                  ExitState = "SI000_ROW_NOT_ADDED_PREVIOUSLY";
                }

                return;
              }

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field =
                GetField(export.Display.Item.DisplayCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

        export.Display.CheckIndex();

        if (local.RowCounter.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        UseSiRmadRemoveRow();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseSiRmadDisplayPage();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
            local.Null1.Date))
          {
            var field1 =
              GetField(export.Display.Item.DisplayCaseRole, "startDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Display.Item.DisplayCaseRole, "endDate");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Display.Item.DisplayCommon, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Display.Item.DisplayCaseRole, "startDate");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;

            if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "cyan";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = true;
            }
            else
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
            }

            var field2 =
              GetField(export.Display.Item.DisplayCommon, "selectChar");

            field2.Color = "green";
            field2.Highlighting = Highlighting.Underscore;
            field2.Protected = false;
          }

          export.HiddenDisplay.Index = export.HiddenDisplay.Index;
          export.HiddenDisplay.CheckSize();

          MoveCaseRole3(export.Display.Item.DisplayCaseRole,
            export.HiddenDisplay.Update.HiddenDisplayRole);
        }

        export.Display.CheckIndex();
        ExitState = "SI0000_ROW_DELETED_OK";

        break;
      case "NEXT":
        if (Equal(export.Standard.ScrollingMessage, "MORE - +") || Equal
          (export.Standard.ScrollingMessage, "MORE +"))
        {
          ++export.PageNumber.PageNumber;
        }
        else
        {
          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
              local.Null1.Date))
            {
            }
            else if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "cyan";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = true;
            }
            else
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
            }
          }

          export.Display.CheckIndex();
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        break;
      case "PREV":
        if (export.PageNumber.PageNumber == 1)
        {
          for(export.Display.Index = 0; export.Display.Index < export
            .Display.Count; ++export.Display.Index)
          {
            if (!export.Display.CheckSize())
            {
              break;
            }

            if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
              local.Null1.Date))
            {
            }
            else if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "cyan";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = true;
            }
            else
            {
              var field =
                GetField(export.Display.Item.DisplayCaseRole, "endDate");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
            }
          }

          export.Display.CheckIndex();
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.PageNumber.PageNumber;

        break;
      case "UPDATE":
        local.RowCounter.Count = 0;
        export.Display.Index = 0;

        for(var limit = export.Display.Count; export.Display.Index < limit; ++
          export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Display.Item.DisplayCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.RowCounter.Count;

              // ** CQ#408 Commented the below code and added the check to to 
              // have start date as a mandatory field **
              // ****** CQ#408 Changes Begin Here ******
              if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
                local.Default1.Date))
              {
                var field1 =
                  GetField(export.Display.Item.DisplayCaseRole, "startDate");

                field1.Error = true;

                ExitState = "SI0000_ENTER_START_DATE";

                return;
              }

              // ****** CQ#408 Changes End   Here ******
              export.HiddenDisplay.Index = export.Display.Index;
              export.HiddenDisplay.CheckSize();

              if (!Equal(export.Display.Item.DisplayCaseRole.StartDate,
                export.HiddenDisplay.Item.HiddenDisplayRole.StartDate) && CharAt
                (import.Display.Item.DisplayCsePersonsWorkSet.Number, 10) != 'O'
                )
              {
                UseCabReadAdabasPerson2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (Lt(export.Display.Item.DisplayCaseRole.StartDate,
                  local.CsePersonsWorkSet.Dob))
                {
                  var field1 =
                    GetField(export.Display.Item.DisplayCaseRole, "startDate");

                  field1.Error = true;

                  ExitState = "START_DT_BEFORE_DOB";

                  return;
                }
              }

              if (!Equal(export.Display.Item.DisplayCaseRole.EndDate,
                export.HiddenDisplay.Item.HiddenDisplayRole.EndDate))
              {
                if (Lt(local.Current.Date,
                  export.Display.Item.DisplayCaseRole.EndDate))
                {
                  var field1 =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field1.Error = true;

                  ExitState = "END_DT_GREATER_THAN_CURRENT_DT";

                  return;
                }

                if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
                  local.Default1.Date))
                {
                  var field1 =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field1.Error = true;

                  ExitState = "END_DT_GREATER_THAN_CURRENT_DT";

                  return;
                }

                if (AsChar(export.Display.Item.DisplayCase.Status) == 'C')
                {
                  if (ReadCase1())
                  {
                    if (Lt(entities.Case1.StatusDate,
                      export.Display.Item.DisplayCaseRole.EndDate))
                    {
                      ExitState = "END_DATE_GREATER_CASE_END_DATE";

                      var field1 =
                        GetField(export.Display.Item.DisplayCaseRole, "endDate");
                        

                      field1.Error = true;

                      return;
                    }

                    // ** CQ#2715 changes were backed out because user could not
                    // key in some back dated data
                    // ** for the cases. If the case is closed and the user 
                    // wants to re-open the case he might have problem,
                    // ** then the business users will take care of it by 
                    // writing update sql's
                  }
                }
              }

              // ** CQ#408 Code modified not to allow start date greater than 
              // end date or current date **
              // ****** CQ#408 Changes Begin Here ******
              if (!Equal(export.HiddenDisplay.Item.HiddenDisplayRole.EndDate,
                local.Default1.Date))
              {
                if (Lt(export.Display.Item.DisplayCaseRole.EndDate,
                  export.Display.Item.DisplayCaseRole.StartDate))
                {
                  var field1 =
                    GetField(export.Display.Item.DisplayCaseRole, "endDate");

                  field1.Error = true;

                  ExitState = "ACO_NE0000_END_LESS_THAN_START";

                  return;
                }
              }

              if (Lt(local.Current.Date,
                export.Display.Item.DisplayCaseRole.StartDate))
              {
                var field1 =
                  GetField(export.Display.Item.DisplayCaseRole, "startDate");

                field1.Error = true;

                ExitState = "START_DT_GREATER_THAN_CURRENT_DT";

                return;
              }

              // ****** CQ#408 Changes End   Here ******
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field =
                GetField(export.Display.Item.DisplayCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

        export.Display.CheckIndex();

        if (local.RowCounter.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        UseSiRmadChangeRow();

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (!export.Display.CheckSize())
          {
            break;
          }

          export.HiddenDisplay.Index = export.Display.Index;
          export.HiddenDisplay.CheckSize();

          MoveCaseRole3(export.Display.Item.DisplayCaseRole,
            export.HiddenDisplay.Update.HiddenDisplayRole);
          export.Display.Update.DisplayCommon.SelectChar = "";
        }

        export.Display.CheckIndex();
        ExitState = "SI0000_RMAD_WAS_UPDATED";

        break;
      case "LIST":
        if (AsChar(export.Prompt.SelectChar) == 'S')
        {
          export.Code.CodeName = "CASE_ROLE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          export.Prompt.SelectChar = "";

          return;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;

          return;
        }

        break;
      case "DISPLAY":
        break;
      default:
        break;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.PageNumber.PageNumber = 1;

      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }

      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }

      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;

      switch(TrimEnd(export.CaseRoleText.Text2))
      {
        case "MO":
          break;
        case "FA":
          break;
        case "AR":
          break;
        case "AP":
          break;
        case "CH":
          break;
        case "AC":
          break;
        case "":
          break;
        default:
          ExitState = "SI0000_RMAD_INVALID_ROLE";

          var field = GetField(export.CaseRoleText, "text2");

          field.Error = true;

          return;
      }

      UseSiReadCsePerson1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Display.Count = 0;
        export.Export1.Count = 0;

        return;
      }

      if (export.PageNumber.PageNumber >= 1)
      {
      }
      else
      {
        export.PageNumber.PageNumber = 1;
      }

      local.CsePerson.Number = export.CsePersonsWorkSet.Number;

      switch(TrimEnd(export.CaseRoleText.Text2))
      {
        case "":
          UseSiRmadAllRoles();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          break;
        case "AC":
          UseSiRmadAllChildren();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          break;
        default:
          export.CaseRole.Type1 = export.CaseRoleText.Text2;
          UseSiRmadCaseRole();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          break;
      }

      if (export.Export1.Count == 0)
      {
        ExitState = "SI_RMAD_NO_ROLES";

        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      UseSiRmadDisplayPage();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      for(export.Display.Index = 0; export.Display.Index < export
        .Display.Count; ++export.Display.Index)
      {
        if (!export.Display.CheckSize())
        {
          break;
        }

        if (Equal(export.Display.Item.DisplayCaseRole.StartDate,
          local.Null1.Date))
        {
          var field1 =
            GetField(export.Display.Item.DisplayCaseRole, "startDate");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Display.Item.DisplayCaseRole, "endDate");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Display.Item.DisplayCommon, "selectChar");

          field3.Color = "cyan";
          field3.Protected = true;
        }
        else
        {
          var field1 =
            GetField(export.Display.Item.DisplayCaseRole, "startDate");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;

          if (Equal(export.Display.Item.DisplayCaseRole.EndDate,
            local.Null1.Date))
          {
            var field =
              GetField(export.Display.Item.DisplayCaseRole, "endDate");

            field.Color = "cyan";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = true;
          }
          else
          {
            var field =
              GetField(export.Display.Item.DisplayCaseRole, "endDate");

            field.Color = "green";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
          }

          var field2 =
            GetField(export.Display.Item.DisplayCommon, "selectChar");

          field2.Color = "green";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = false;
        }

        export.HiddenDisplay.Index = export.Display.Index;
        export.HiddenDisplay.CheckSize();

        MoveCaseRole3(export.Display.Item.DisplayCaseRole,
          export.HiddenDisplay.Update.HiddenDisplayRole);
      }

      export.Display.CheckIndex();
      ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
    }

    if (AsChar(local.SucessfullyDeleted.Flag) == 'Y')
    {
      ExitState = "SI0000_ROLE_SUCESSFULLY_DELETED";
    }

    if (AsChar(local.SavingOk.Flag) == 'Y')
    {
      if (AsChar(local.CheckCasesForOtherKid.Flag) == 'Y')
      {
        ExitState = "SI0000_UPD_OK_CHECK_OTHER_CASES";
      }
      else
      {
        ExitState = "ACO_NI0000_SUCCESSFULLY_SAVED";
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCaseRole3(CaseRole source, CaseRole target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDisplay1(Export.DisplayGroup source,
    SiRmadRemoveRow.Import.DisplayGroup target)
  {
    target.DisplayRowIndicator.SelectChar = source.DisplayCommon.SelectChar;
    target.DisplayCaseRole.Assign(source.DisplayCaseRole);
    MoveCsePersonsWorkSet2(source.DisplayCsePersonsWorkSet,
      target.DisplayCsePersonsWorkSet);
    MoveCase1(source.DisplayCase, target.DisplayCase);
    target.DisplayRowNumber.Count = source.DisplayRowNumber.Count;
    target.DisplayRowOperation.OneChar = source.DisplayRowOperation.OneChar;
    target.DisplayOffice.Name = source.DisplayOffice.Name;
    MoveServiceProvider(source.DisplayServiceProvider,
      target.DisplayServiceProvider);
  }

  private static void MoveDisplay2(Export.DisplayGroup source,
    SiRmadChangeRow.Import.DisplayGroup target)
  {
    target.DisplayRowIndicator.SelectChar = source.DisplayCommon.SelectChar;
    target.DisplayCaseRole.Assign(source.DisplayCaseRole);
    MoveCsePersonsWorkSet2(source.DisplayCsePersonsWorkSet,
      target.DisplayCsePersonsWorkSet);
    MoveCase1(source.DisplayCase, target.DisplayCase);
    target.DisplayRowNumber.Count = source.DisplayRowNumber.Count;
    target.DisplayRowOperation.OneChar = source.DisplayRowOperation.OneChar;
    target.DisplayOffice.Name = source.DisplayOffice.Name;
    MoveServiceProvider(source.DisplayServiceProvider,
      target.DisplayServiceProvider);
  }

  private static void MoveDisplay3(SiRmadDisplayPage.Export.DisplayGroup source,
    Export.DisplayGroup target)
  {
    target.DisplayCommon.SelectChar = source.DisplayRowIndicator.SelectChar;
    target.DisplayCaseRole.Assign(source.DisplayCaseRole);
    MoveCsePersonsWorkSet2(source.DisplayCsePersonsWorkSet,
      target.DisplayCsePersonsWorkSet);
    MoveCase1(source.DisplayCase, target.DisplayCase);
    target.DisplayRowNumber.Count = source.DisplayRowNumber.Count;
    target.DisplayRowOperation.OneChar = source.DisplayRowOperation.OneChar;
    target.DisplayOffice.Name = source.DisplayOffice.Name;
    MoveServiceProvider(source.DisplayServiceProvider,
      target.DisplayServiceProvider);
  }

  private static void MoveExport2(SiRmadAllChildren.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.AllExportRowIndicator.SelectChar = source.RowIndicator.SelectChar;
    target.AllCaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet2(source.CsePersonsWorkSet, target.AllCsePersonsWorkSet);
      
    MoveCase1(source.Case1, target.AllCase);
    target.AllExportRowNumber.Count = source.RowNumber.Count;
    target.AllExportRowOperation.OneChar = source.RowOperation.OneChar;
    target.AllOffice.Name = source.Office.Name;
    MoveServiceProvider(source.ServiceProvider, target.AllServiceProvider);
  }

  private static void MoveExport3(SiRmadAllRoles.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.AllExportRowIndicator.SelectChar = source.RowIndicator.SelectChar;
    target.AllCaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet2(source.CsePersonsWorkSet, target.AllCsePersonsWorkSet);
      
    MoveCase1(source.Case1, target.AllCase);
    target.AllExportRowNumber.Count = source.RowNumber.Count;
    target.AllExportRowOperation.OneChar = source.RowOperation.OneChar;
    target.AllOffice.Name = source.Office.Name;
    MoveServiceProvider(source.ServiceProvider, target.AllServiceProvider);
  }

  private static void MoveExport4(SiRmadRemoveRow.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.AllExportRowIndicator.SelectChar = source.RowIndicator.SelectChar;
    target.AllCaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet2(source.CsePersonsWorkSet, target.AllCsePersonsWorkSet);
      
    MoveCase1(source.Case1, target.AllCase);
    target.AllExportRowNumber.Count = source.RowNumber.Count;
    target.AllExportRowOperation.OneChar = source.RowOperation.OneChar;
    target.AllOffice.Name = source.Office.Name;
    MoveServiceProvider(source.ServiceProvider, target.AllServiceProvider);
  }

  private static void MoveExport5(SiRmadCaseRole.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.AllExportRowIndicator.SelectChar = source.RowIndicator.SelectChar;
    target.AllCaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet2(source.CsePersonsWorkSet, target.AllCsePersonsWorkSet);
      
    MoveCase1(source.Case1, target.AllCase);
    target.AllExportRowNumber.Count = source.RowNumber.Count;
    target.AllExportRowOperation.OneChar = source.RowOperation.OneChar;
    target.AllOffice.Name = source.Office.Name;
    MoveServiceProvider(source.ServiceProvider, target.AllServiceProvider);
  }

  private static void MoveExport6(SiRmadChangeRow.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.AllExportRowIndicator.SelectChar = source.RowIndicator.SelectChar;
    target.AllCaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet2(source.CsePersonsWorkSet, target.AllCsePersonsWorkSet);
      
    MoveCase1(source.Case1, target.AllCase);
    target.AllExportRowNumber.Count = source.RowNumber.Count;
    target.AllExportRowOperation.OneChar = source.RowOperation.OneChar;
    target.AllOffice.Name = source.Office.Name;
    MoveServiceProvider(source.ServiceProvider, target.AllServiceProvider);
  }

  private static void MoveExport7(SiRmadAddRow.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.AllExportRowIndicator.SelectChar = source.RowIndicator.SelectChar;
    target.AllCaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet2(source.CsePersonsWorkSet, target.AllCsePersonsWorkSet);
      
    MoveCase1(source.Case1, target.AllCase);
    target.AllExportRowNumber.Count = source.RowNumber.Count;
    target.AllExportRowOperation.OneChar = source.RowOperation.OneChar;
    target.AllOffice.Name = source.Office.Name;
    MoveServiceProvider(source.ServiceProvider, target.AllServiceProvider);
  }

  private static void MoveExport1ToImport2(Export.ExportGroup source,
    SiRmadRemoveRow.Import.ImportGroup target)
  {
    target.RowIndicator.SelectChar = source.AllExportRowIndicator.SelectChar;
    target.CaseRole.Assign(source.AllCaseRole);
    MoveCsePersonsWorkSet2(source.AllCsePersonsWorkSet, target.CsePersonsWorkSet);
      
    MoveCase1(source.AllCase, target.Case1);
    target.RowNumber.Count = source.AllExportRowNumber.Count;
    target.RowOperation.OneChar = source.AllExportRowOperation.OneChar;
    target.Office.Name = source.AllOffice.Name;
    MoveServiceProvider(source.AllServiceProvider, target.ServiceProvider);
  }

  private static void MoveExport1ToImport3(Export.ExportGroup source,
    SiRmadDisplayPage.Import.ImportGroup target)
  {
    target.RowIndicator.SelectChar = source.AllExportRowIndicator.SelectChar;
    target.CaseRole.Assign(source.AllCaseRole);
    MoveCsePersonsWorkSet2(source.AllCsePersonsWorkSet, target.CsePersonsWorkSet);
      
    MoveCase1(source.AllCase, target.Case1);
    target.RowNumber.Count = source.AllExportRowNumber.Count;
    target.RowOperation.OneChar = source.AllExportRowOperation.OneChar;
    target.Office.Name = source.AllOffice.Name;
    MoveServiceProvider(source.AllServiceProvider, target.ServiceProvider);
  }

  private static void MoveExport1ToImport4(Export.ExportGroup source,
    SiRmadChangeRow.Import.ImportGroup target)
  {
    target.RowIndicator.SelectChar = source.AllExportRowIndicator.SelectChar;
    target.CaseRole.Assign(source.AllCaseRole);
    MoveCsePersonsWorkSet2(source.AllCsePersonsWorkSet, target.CsePersonsWorkSet);
      
    MoveCase1(source.AllCase, target.Case1);
    target.RowNumber.Count = source.AllExportRowNumber.Count;
    target.RowOperation.OneChar = source.AllExportRowOperation.OneChar;
    target.Office.Name = source.AllOffice.Name;
    MoveServiceProvider(source.AllServiceProvider, target.ServiceProvider);
  }

  private static void MoveExport1ToImport5(Export.ExportGroup source,
    SiRmadAddRow.Import.ImportGroup target)
  {
    target.RowIndicator.SelectChar = source.AllExportRowIndicator.SelectChar;
    target.CaseRole.Assign(source.AllCaseRole);
    MoveCsePersonsWorkSet2(source.AllCsePersonsWorkSet, target.CsePersonsWorkSet);
      
    MoveCase1(source.AllCase, target.Case1);
    target.RowNumber.Count = source.AllExportRowNumber.Count;
    target.RowOperation.OneChar = source.AllExportRowOperation.OneChar;
    target.Office.Name = source.AllOffice.Name;
    MoveServiceProvider(source.AllServiceProvider, target.ServiceProvider);
  }

  private static void MoveExport1ToImport6(Export.ExportGroup source,
    SiRmadSaveData.Import.ImportGroup target)
  {
    target.RowIndicator.SelectChar = source.AllExportRowIndicator.SelectChar;
    target.CaseRole.Assign(source.AllCaseRole);
    MoveCsePersonsWorkSet2(source.AllCsePersonsWorkSet, target.CsePersonsWorkSet);
      
    MoveCase1(source.AllCase, target.Case1);
    target.RowNumber.Count = source.AllExportRowNumber.Count;
    target.RowOperation.OneChar = source.AllExportRowOperation.OneChar;
    target.Office.Name = source.AllOffice.Name;
    MoveServiceProvider(source.AllServiceProvider, target.ServiceProvider);
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.UserId = source.UserId;
    target.LastName = source.LastName;
  }

  private static void MoveSsnWorkArea(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabReadAdabasPerson1()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = export.NewCsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.Cse.Flag = useExport.Cse.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.NewCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabReadAdabasPerson2()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number =
      export.Display.Item.DisplayCsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.NewSsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    export.NewSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.NewSsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.NewSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCheckCaseRoleCombinations()
  {
    var useImport = new SiCheckCaseRoleCombinations.Import();
    var useExport = new SiCheckCaseRoleCombinations.Export();

    useImport.Case1.Number = local.NewCase.Number;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Verify.Type1 = local.CaseRole.Type1;
    useImport.New1.StartDate = export.NewCaseRole.StartDate;

    Call(SiCheckCaseRoleCombinations.Execute, useImport, useExport);

    local.Common.Flag = useExport.Common.Flag;
  }

  private void UseSiCheckName()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.NewCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.NewCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.NewCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiRmadAddRow()
  {
    var useImport = new SiRmadAddRow.Import();
    var useExport = new SiRmadAddRow.Export();

    useImport.NewOffice.Name = local.NewOffice.Name;
    MoveServiceProvider(local.NewServiceProvider, useImport.NewServiceProvider);
    MoveCaseRole2(export.NewCaseRole, useImport.NewCaseRole);
    MoveCsePersonsWorkSet2(export.NewCsePersonsWorkSet,
      useImport.NewCsePersonsWorkSet);
    MoveCase1(local.NewCase, useImport.NewCase);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport5);

    Call(SiRmadAddRow.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport7);
  }

  private void UseSiRmadAllChildren()
  {
    var useImport = new SiRmadAllChildren.Import();
    var useExport = new SiRmadAllChildren.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiRmadAllChildren.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport2);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiRmadAllRoles()
  {
    var useImport = new SiRmadAllRoles.Import();
    var useExport = new SiRmadAllRoles.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiRmadAllRoles.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport3);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiRmadCaseRole()
  {
    var useImport = new SiRmadCaseRole.Import();
    var useExport = new SiRmadCaseRole.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CaseRole.Type1 = export.CaseRole.Type1;
    useImport.CsePersonsWorkSet.FormattedName =
      export.CsePersonsWorkSet.FormattedName;

    Call(SiRmadCaseRole.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport5);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiRmadChangeRow()
  {
    var useImport = new SiRmadChangeRow.Import();
    var useExport = new SiRmadChangeRow.Export();

    export.Display.CopyTo(useImport.Display, MoveDisplay2);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport4);

    Call(SiRmadChangeRow.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport6);
  }

  private void UseSiRmadDisplayPage()
  {
    var useImport = new SiRmadDisplayPage.Import();
    var useExport = new SiRmadDisplayPage.Export();

    useImport.Standard.PageNumber = export.PageNumber.PageNumber;
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport3);

    Call(SiRmadDisplayPage.Execute, useImport, useExport);

    useExport.Display.CopyTo(export.Display, MoveDisplay3);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSiRmadForChWithDiffAr()
  {
    var useImport = new SiRmadForChWithDiffAr.Import();
    var useExport = new SiRmadForChWithDiffAr.Export();

    useImport.Case1.Number = local.Validated.Number;
    useImport.CsePerson.Number = local.Header.Number;

    Call(SiRmadForChWithDiffAr.Execute, useImport, useExport);

    local.InvalidChCsePerson.Number = useExport.ChCsePerson.Number;
    local.InvalidChCaseRole.Identifier = useExport.ChCaseRole.Identifier;
    local.CheckCasesForOtherKid.Flag = useExport.CheckCasesForOtherKid.Flag;
  }

  private void UseSiRmadRemoveRow()
  {
    var useImport = new SiRmadRemoveRow.Import();
    var useExport = new SiRmadRemoveRow.Export();

    export.Display.CopyTo(useImport.Display, MoveDisplay1);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport2);

    Call(SiRmadRemoveRow.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport4);
  }

  private void UseSiRmadSaveData()
  {
    var useImport = new SiRmadSaveData.Import();
    var useExport = new SiRmadSaveData.Export();

    useImport.ArGap.Number = import.GapCase.Number;
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport6);

    Call(SiRmadSaveData.Execute, useImport, useExport);

    local.ArGapCommon.Flag = useExport.ArGapCommon.Flag;
    local.ArGapCase.Number = useExport.ArGapCase.Number;
    local.ErrorNumber.Count = useExport.ErrorNumber.Count;
    local.ErrorCsePersonsWorkSet.Number =
      useExport.ErrorCsePersonsWorkSet.Number;
    local.ErrorRowNumber.Count = useExport.ErrorRowNumber.Count;
    local.ErrorCase.Number = useExport.ErrorCase.Number;
  }

  private void UseSiVerifyAndCreateCsePerson()
  {
    var useImport = new SiVerifyAndCreateCsePerson.Import();
    var useExport = new SiVerifyAndCreateCsePerson.Export();

    useImport.Cse.Flag = local.Cse.Flag;
    useImport.RetreivePersonProgram.Date = local.RetreivePersonProgram.Date;
    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(SiVerifyAndCreateCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet,
      export.NewCsePersonsWorkSet);
  }

  private void UseSiVerifyChildIsOnACase()
  {
    var useImport = new SiVerifyChildIsOnACase.Import();
    var useExport = new SiVerifyChildIsOnACase.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CaseRole.StartDate = export.NewCaseRole.StartDate;

    Call(SiVerifyChildIsOnACase.Execute, useImport, useExport);

    local.RelToArIsCh.Flag = useExport.RelToArIsCh.Flag;
    MoveCommon(useExport.ActiveCaseCh, local.ActiveCaseCh);
  }

  private void UseSiVerifyOneActiveCaseRole1()
  {
    var useImport = new SiVerifyOneActiveCaseRole.Import();
    var useExport = new SiVerifyOneActiveCaseRole.Export();

    useImport.Case1.Number = local.NewCase.Number;
    MoveCaseRole1(local.CaseRole, useImport.CaseRole);

    Call(SiVerifyOneActiveCaseRole.Execute, useImport, useExport);

    local.ErrorCommon.Flag = useExport.Common.Flag;
  }

  private void UseSiVerifyOneActiveCaseRole2()
  {
    var useImport = new SiVerifyOneActiveCaseRole.Import();
    var useExport = new SiVerifyOneActiveCaseRole.Export();

    useImport.Case1.Number = local.NewCase.Number;
    MoveCaseRole1(export.NewCaseRole, useImport.CaseRole);

    Call(SiVerifyOneActiveCaseRole.Execute, useImport, useExport);

    local.ErrorCommon.Flag = useExport.Common.Flag;
  }

  private void DeleteCaseRole()
  {
    var casNumber = entities.Delete.CasNumber;
    var cspNumber = entities.Delete.CspNumber;
    bool exists;

    exists = Read("DeleteCaseRole#1",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_APPOINTMENT\".",
        "50001");
    }

    Update("DeleteCaseRole#2",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    Update("DeleteCaseRole#3",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    Update("DeleteCaseRole#4",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    Update("DeleteCaseRole#5",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    exists = Read("DeleteCaseRole#6",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_INTERSTAT_RQST\".",
        "50001");
    }

    Update("DeleteCaseRole#7",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    Update("DeleteCaseRole#8",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    Update("DeleteCaseRole#9",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    Update("DeleteCaseRole#10",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    Update("DeleteCaseRole#11",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber);
        db.SetNullableString(command, "cspNumber1", cspNumber);
        db.SetNullableString(command, "croType", entities.Delete.Type1);
        db.SetNullableInt32(command, "croId", entities.Delete.Identifier);
      });

    exists = Read("DeleteCaseRole#12",
      (db, command) =>
      {
        db.SetString(command, "casNumber2", casNumber);
      },
      null);

    if (!exists)
    {
      Update("DeleteCaseRole#13",
        (db, command) =>
        {
          db.SetString(command, "casNumber2", casNumber);
        });
    }

    exists = Read("DeleteCaseRole#14",
      (db, command) =>
      {
        db.SetString(command, "cspNumber2", cspNumber);
      },
      null);

    if (!exists)
    {
      Update("DeleteCaseRole#15",
        (db, command) =>
        {
          db.SetString(command, "cspNumber2", cspNumber);
        });
    }
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Display.Item.DisplayCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.NewCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.NewCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.NewCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.ChCaseRole.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.NewCase.Number);
        db.SetString(command, "cspNumber", export.NewCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);
      });
  }

  private bool ReadCaseRole4()
  {
    entities.Delete.Populated = false;

    return Read("ReadCaseRole4",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId",
          export.Display.Item.DisplayCaseRole.Identifier);
        db.SetString(
          command, "casNumber", export.Display.Item.DisplayCase.Number);
      },
      (db, reader) =>
      {
        entities.Delete.CasNumber = db.GetString(reader, 0);
        entities.Delete.CspNumber = db.GetString(reader, 1);
        entities.Delete.Type1 = db.GetString(reader, 2);
        entities.Delete.Identifier = db.GetInt32(reader, 3);
        entities.Delete.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Delete.Type1);
      });
  }

  private bool ReadCaseRole5()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.NewCase.Number);
        db.SetString(command, "cspNumber", export.NewCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadChild()
  {
    entities.Child.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId",
          export.Display.Item.DisplayCaseRole.Identifier);
        db.SetString(
          command, "cspNumber",
          export.Display.Item.DisplayCsePersonsWorkSet.Number);
        db.SetString(
          command, "casNumber", export.Display.Item.DisplayCase.Number);
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ChildPaternityEstInd.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.NewCase.Number);
      },
      (db, reader) =>
      {
        entities.ChildPaternityEstInd.Number = db.GetString(reader, 0);
        entities.ChildPaternityEstInd.Type1 = db.GetString(reader, 1);
        entities.ChildPaternityEstInd.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.ChildPaternityEstInd.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.ChildPaternityEstInd.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildPaternityEstInd.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb1", entities.CaseRole.CspNumber);
        db.SetString(command, "numb2", export.NewCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadMother()
  {
    entities.Mother.Populated = false;

    return Read("ReadMother",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId",
          export.Display.Item.DisplayCaseRole.Identifier);
        db.SetString(
          command, "cspNumber",
          export.Display.Item.DisplayCsePersonsWorkSet.Number);
        db.SetString(
          command, "casNumber", export.Display.Item.DisplayCase.Number);
      },
      (db, reader) =>
      {
        entities.Mother.CasNumber = db.GetString(reader, 0);
        entities.Mother.CspNumber = db.GetString(reader, 1);
        entities.Mother.Type1 = db.GetString(reader, 2);
        entities.Mother.Identifier = db.GetInt32(reader, 3);
        entities.Mother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
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
    /// <summary>A HiddenDisplayGroup group.</summary>
    [Serializable]
    public class HiddenDisplayGroup
    {
      /// <summary>
      /// A value of HiddenDisplayRole.
      /// </summary>
      [JsonPropertyName("hiddenDisplayRole")]
      public CaseRole HiddenDisplayRole
      {
        get => hiddenDisplayRole ??= new();
        set => hiddenDisplayRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CaseRole hiddenDisplayRole;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of AllImportRowIndicator.
      /// </summary>
      [JsonPropertyName("allImportRowIndicator")]
      public Common AllImportRowIndicator
      {
        get => allImportRowIndicator ??= new();
        set => allImportRowIndicator = value;
      }

      /// <summary>
      /// A value of AllCaseRole.
      /// </summary>
      [JsonPropertyName("allCaseRole")]
      public CaseRole AllCaseRole
      {
        get => allCaseRole ??= new();
        set => allCaseRole = value;
      }

      /// <summary>
      /// A value of AllCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("allCsePersonsWorkSet")]
      public CsePersonsWorkSet AllCsePersonsWorkSet
      {
        get => allCsePersonsWorkSet ??= new();
        set => allCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of AllCase.
      /// </summary>
      [JsonPropertyName("allCase")]
      public Case1 AllCase
      {
        get => allCase ??= new();
        set => allCase = value;
      }

      /// <summary>
      /// A value of AllImportRowNumber.
      /// </summary>
      [JsonPropertyName("allImportRowNumber")]
      public Common AllImportRowNumber
      {
        get => allImportRowNumber ??= new();
        set => allImportRowNumber = value;
      }

      /// <summary>
      /// A value of AllImportRowOperation.
      /// </summary>
      [JsonPropertyName("allImportRowOperation")]
      public Standard AllImportRowOperation
      {
        get => allImportRowOperation ??= new();
        set => allImportRowOperation = value;
      }

      /// <summary>
      /// A value of AllOffice.
      /// </summary>
      [JsonPropertyName("allOffice")]
      public Office AllOffice
      {
        get => allOffice ??= new();
        set => allOffice = value;
      }

      /// <summary>
      /// A value of AllServiceProvider.
      /// </summary>
      [JsonPropertyName("allServiceProvider")]
      public ServiceProvider AllServiceProvider
      {
        get => allServiceProvider ??= new();
        set => allServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common allImportRowIndicator;
      private CaseRole allCaseRole;
      private CsePersonsWorkSet allCsePersonsWorkSet;
      private Case1 allCase;
      private Common allImportRowNumber;
      private Standard allImportRowOperation;
      private Office allOffice;
      private ServiceProvider allServiceProvider;
    }

    /// <summary>A DisplayGroup group.</summary>
    [Serializable]
    public class DisplayGroup
    {
      /// <summary>
      /// A value of DisplayServiceProvider.
      /// </summary>
      [JsonPropertyName("displayServiceProvider")]
      public ServiceProvider DisplayServiceProvider
      {
        get => displayServiceProvider ??= new();
        set => displayServiceProvider = value;
      }

      /// <summary>
      /// A value of DisplayOffice.
      /// </summary>
      [JsonPropertyName("displayOffice")]
      public Office DisplayOffice
      {
        get => displayOffice ??= new();
        set => displayOffice = value;
      }

      /// <summary>
      /// A value of DisplayRowOperation.
      /// </summary>
      [JsonPropertyName("displayRowOperation")]
      public Standard DisplayRowOperation
      {
        get => displayRowOperation ??= new();
        set => displayRowOperation = value;
      }

      /// <summary>
      /// A value of DisplayRowNumber.
      /// </summary>
      [JsonPropertyName("displayRowNumber")]
      public Common DisplayRowNumber
      {
        get => displayRowNumber ??= new();
        set => displayRowNumber = value;
      }

      /// <summary>
      /// A value of DisplayCommon.
      /// </summary>
      [JsonPropertyName("displayCommon")]
      public Common DisplayCommon
      {
        get => displayCommon ??= new();
        set => displayCommon = value;
      }

      /// <summary>
      /// A value of DisplayCase.
      /// </summary>
      [JsonPropertyName("displayCase")]
      public Case1 DisplayCase
      {
        get => displayCase ??= new();
        set => displayCase = value;
      }

      /// <summary>
      /// A value of DisplayCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("displayCsePersonsWorkSet")]
      public CsePersonsWorkSet DisplayCsePersonsWorkSet
      {
        get => displayCsePersonsWorkSet ??= new();
        set => displayCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DisplayCaseRole.
      /// </summary>
      [JsonPropertyName("displayCaseRole")]
      public CaseRole DisplayCaseRole
      {
        get => displayCaseRole ??= new();
        set => displayCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private ServiceProvider displayServiceProvider;
      private Office displayOffice;
      private Standard displayRowOperation;
      private Common displayRowNumber;
      private Common displayCommon;
      private Case1 displayCase;
      private CsePersonsWorkSet displayCsePersonsWorkSet;
      private CaseRole displayCaseRole;
    }

    /// <summary>
    /// A value of GapCase.
    /// </summary>
    [JsonPropertyName("gapCase")]
    public Case1 GapCase
    {
      get => gapCase ??= new();
      set => gapCase = value;
    }

    /// <summary>
    /// A value of GapCommon.
    /// </summary>
    [JsonPropertyName("gapCommon")]
    public Common GapCommon
    {
      get => gapCommon ??= new();
      set => gapCommon = value;
    }

    /// <summary>
    /// A value of Response.
    /// </summary>
    [JsonPropertyName("response")]
    public Common Response
    {
      get => response ??= new();
      set => response = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public WorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of CaseRoleText.
    /// </summary>
    [JsonPropertyName("caseRoleText")]
    public TextWorkArea CaseRoleText
    {
      get => caseRoleText ??= new();
      set => caseRoleText = value;
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
    /// Gets a value of HiddenDisplay.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenDisplayGroup> HiddenDisplay => hiddenDisplay ??= new(
      HiddenDisplayGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenDisplay for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenDisplay")]
    [Computed]
    public IList<HiddenDisplayGroup> HiddenDisplay_Json
    {
      get => hiddenDisplay;
      set => HiddenDisplay.Assign(value);
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
    }

    /// <summary>
    /// A value of ChildSuccessfullyAdded.
    /// </summary>
    [JsonPropertyName("childSuccessfullyAdded")]
    public Common ChildSuccessfullyAdded
    {
      get => childSuccessfullyAdded ??= new();
      set => childSuccessfullyAdded = value;
    }

    /// <summary>
    /// A value of BeenToName.
    /// </summary>
    [JsonPropertyName("beenToName")]
    public Common BeenToName
    {
      get => beenToName ??= new();
      set => beenToName = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of NewCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("newCsePersonsWorkSet")]
    public CsePersonsWorkSet NewCsePersonsWorkSet
    {
      get => newCsePersonsWorkSet ??= new();
      set => newCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NewCaseRole.
    /// </summary>
    [JsonPropertyName("newCaseRole")]
    public CaseRole NewCaseRole
    {
      get => newCaseRole ??= new();
      set => newCaseRole = value;
    }

    /// <summary>
    /// A value of SelectAction.
    /// </summary>
    [JsonPropertyName("selectAction")]
    public Common SelectAction
    {
      get => selectAction ??= new();
      set => selectAction = value;
    }

    /// <summary>
    /// A value of NewSsnWorkArea.
    /// </summary>
    [JsonPropertyName("newSsnWorkArea")]
    public SsnWorkArea NewSsnWorkArea
    {
      get => newSsnWorkArea ??= new();
      set => newSsnWorkArea = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// Gets a value of Display.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayGroup> Display => display ??= new(
      DisplayGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Display for json serialization.
    /// </summary>
    [JsonPropertyName("display")]
    [Computed]
    public IList<DisplayGroup> Display_Json
    {
      get => display;
      set => Display.Assign(value);
    }

    private Case1 gapCase;
    private Common gapCommon;
    private Common response;
    private WorkArea message;
    private TextWorkArea caseRoleText;
    private CodeValue codeValue;
    private Array<HiddenDisplayGroup> hiddenDisplay;
    private Standard pageNumber;
    private Array<ImportGroup> import1;
    private Common prompt;
    private CaseRole caseRole;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common fromRole;
    private Common childSuccessfullyAdded;
    private Common beenToName;
    private Case1 next;
    private Case1 case1;
    private Standard hiddenStandard;
    private CsePersonsWorkSet newCsePersonsWorkSet;
    private CaseRole newCaseRole;
    private Common selectAction;
    private SsnWorkArea newSsnWorkArea;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<DisplayGroup> display;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenDisplayGroup group.</summary>
    [Serializable]
    public class HiddenDisplayGroup
    {
      /// <summary>
      /// A value of HiddenDisplayRole.
      /// </summary>
      [JsonPropertyName("hiddenDisplayRole")]
      public CaseRole HiddenDisplayRole
      {
        get => hiddenDisplayRole ??= new();
        set => hiddenDisplayRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CaseRole hiddenDisplayRole;
    }

    /// <summary>A DisplayGroup group.</summary>
    [Serializable]
    public class DisplayGroup
    {
      /// <summary>
      /// A value of DisplayCommon.
      /// </summary>
      [JsonPropertyName("displayCommon")]
      public Common DisplayCommon
      {
        get => displayCommon ??= new();
        set => displayCommon = value;
      }

      /// <summary>
      /// A value of DisplayCaseRole.
      /// </summary>
      [JsonPropertyName("displayCaseRole")]
      public CaseRole DisplayCaseRole
      {
        get => displayCaseRole ??= new();
        set => displayCaseRole = value;
      }

      /// <summary>
      /// A value of DisplayCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("displayCsePersonsWorkSet")]
      public CsePersonsWorkSet DisplayCsePersonsWorkSet
      {
        get => displayCsePersonsWorkSet ??= new();
        set => displayCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DisplayCase.
      /// </summary>
      [JsonPropertyName("displayCase")]
      public Case1 DisplayCase
      {
        get => displayCase ??= new();
        set => displayCase = value;
      }

      /// <summary>
      /// A value of DisplayRowNumber.
      /// </summary>
      [JsonPropertyName("displayRowNumber")]
      public Common DisplayRowNumber
      {
        get => displayRowNumber ??= new();
        set => displayRowNumber = value;
      }

      /// <summary>
      /// A value of DisplayRowOperation.
      /// </summary>
      [JsonPropertyName("displayRowOperation")]
      public Standard DisplayRowOperation
      {
        get => displayRowOperation ??= new();
        set => displayRowOperation = value;
      }

      /// <summary>
      /// A value of DisplayOffice.
      /// </summary>
      [JsonPropertyName("displayOffice")]
      public Office DisplayOffice
      {
        get => displayOffice ??= new();
        set => displayOffice = value;
      }

      /// <summary>
      /// A value of DisplayServiceProvider.
      /// </summary>
      [JsonPropertyName("displayServiceProvider")]
      public ServiceProvider DisplayServiceProvider
      {
        get => displayServiceProvider ??= new();
        set => displayServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common displayCommon;
      private CaseRole displayCaseRole;
      private CsePersonsWorkSet displayCsePersonsWorkSet;
      private Case1 displayCase;
      private Common displayRowNumber;
      private Standard displayRowOperation;
      private Office displayOffice;
      private ServiceProvider displayServiceProvider;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of AllExportRowIndicator.
      /// </summary>
      [JsonPropertyName("allExportRowIndicator")]
      public Common AllExportRowIndicator
      {
        get => allExportRowIndicator ??= new();
        set => allExportRowIndicator = value;
      }

      /// <summary>
      /// A value of AllCaseRole.
      /// </summary>
      [JsonPropertyName("allCaseRole")]
      public CaseRole AllCaseRole
      {
        get => allCaseRole ??= new();
        set => allCaseRole = value;
      }

      /// <summary>
      /// A value of AllCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("allCsePersonsWorkSet")]
      public CsePersonsWorkSet AllCsePersonsWorkSet
      {
        get => allCsePersonsWorkSet ??= new();
        set => allCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of AllCase.
      /// </summary>
      [JsonPropertyName("allCase")]
      public Case1 AllCase
      {
        get => allCase ??= new();
        set => allCase = value;
      }

      /// <summary>
      /// A value of AllExportRowNumber.
      /// </summary>
      [JsonPropertyName("allExportRowNumber")]
      public Common AllExportRowNumber
      {
        get => allExportRowNumber ??= new();
        set => allExportRowNumber = value;
      }

      /// <summary>
      /// A value of AllExportRowOperation.
      /// </summary>
      [JsonPropertyName("allExportRowOperation")]
      public Standard AllExportRowOperation
      {
        get => allExportRowOperation ??= new();
        set => allExportRowOperation = value;
      }

      /// <summary>
      /// A value of AllOffice.
      /// </summary>
      [JsonPropertyName("allOffice")]
      public Office AllOffice
      {
        get => allOffice ??= new();
        set => allOffice = value;
      }

      /// <summary>
      /// A value of AllServiceProvider.
      /// </summary>
      [JsonPropertyName("allServiceProvider")]
      public ServiceProvider AllServiceProvider
      {
        get => allServiceProvider ??= new();
        set => allServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common allExportRowIndicator;
      private CaseRole allCaseRole;
      private CsePersonsWorkSet allCsePersonsWorkSet;
      private Case1 allCase;
      private Common allExportRowNumber;
      private Standard allExportRowOperation;
      private Office allOffice;
      private ServiceProvider allServiceProvider;
    }

    /// <summary>
    /// A value of GapCase.
    /// </summary>
    [JsonPropertyName("gapCase")]
    public Case1 GapCase
    {
      get => gapCase ??= new();
      set => gapCase = value;
    }

    /// <summary>
    /// A value of GapCommon.
    /// </summary>
    [JsonPropertyName("gapCommon")]
    public Common GapCommon
    {
      get => gapCommon ??= new();
      set => gapCommon = value;
    }

    /// <summary>
    /// A value of Response.
    /// </summary>
    [JsonPropertyName("response")]
    public Common Response
    {
      get => response ??= new();
      set => response = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public WorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of CaseRoleText.
    /// </summary>
    [JsonPropertyName("caseRoleText")]
    public TextWorkArea CaseRoleText
    {
      get => caseRoleText ??= new();
      set => caseRoleText = value;
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
    /// Gets a value of HiddenDisplay.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenDisplayGroup> HiddenDisplay => hiddenDisplay ??= new(
      HiddenDisplayGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenDisplay for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenDisplay")]
    [Computed]
    public IList<HiddenDisplayGroup> HiddenDisplay_Json
    {
      get => hiddenDisplay;
      set => HiddenDisplay.Assign(value);
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
    }

    /// <summary>
    /// A value of BeenToName.
    /// </summary>
    [JsonPropertyName("beenToName")]
    public Common BeenToName
    {
      get => beenToName ??= new();
      set => beenToName = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of NewCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("newCsePersonsWorkSet")]
    public CsePersonsWorkSet NewCsePersonsWorkSet
    {
      get => newCsePersonsWorkSet ??= new();
      set => newCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NewCaseRole.
    /// </summary>
    [JsonPropertyName("newCaseRole")]
    public CaseRole NewCaseRole
    {
      get => newCaseRole ??= new();
      set => newCaseRole = value;
    }

    /// <summary>
    /// A value of SelectAction.
    /// </summary>
    [JsonPropertyName("selectAction")]
    public Common SelectAction
    {
      get => selectAction ??= new();
      set => selectAction = value;
    }

    /// <summary>
    /// A value of NewSsnWorkArea.
    /// </summary>
    [JsonPropertyName("newSsnWorkArea")]
    public SsnWorkArea NewSsnWorkArea
    {
      get => newSsnWorkArea ??= new();
      set => newSsnWorkArea = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// Gets a value of Display.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayGroup> Display => display ??= new(
      DisplayGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Display for json serialization.
    /// </summary>
    [JsonPropertyName("display")]
    [Computed]
    public IList<DisplayGroup> Display_Json
    {
      get => display;
      set => Display.Assign(value);
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private Case1 gapCase;
    private Common gapCommon;
    private Common response;
    private WorkArea message;
    private TextWorkArea caseRoleText;
    private Code code;
    private Array<HiddenDisplayGroup> hiddenDisplay;
    private Standard pageNumber;
    private Common prompt;
    private CaseRole caseRole;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common fromRole;
    private Common beenToName;
    private CsePersonsWorkSet selected;
    private Case1 next;
    private Case1 case1;
    private Standard standard;
    private Standard hiddenStandard;
    private CsePersonsWorkSet newCsePersonsWorkSet;
    private CaseRole newCaseRole;
    private Common selectAction;
    private SsnWorkArea newSsnWorkArea;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<DisplayGroup> display;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InvalidChCaseRole.
    /// </summary>
    [JsonPropertyName("invalidChCaseRole")]
    public CaseRole InvalidChCaseRole
    {
      get => invalidChCaseRole ??= new();
      set => invalidChCaseRole = value;
    }

    /// <summary>
    /// A value of CheckCasesForOtherKid.
    /// </summary>
    [JsonPropertyName("checkCasesForOtherKid")]
    public Common CheckCasesForOtherKid
    {
      get => checkCasesForOtherKid ??= new();
      set => checkCasesForOtherKid = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public CsePerson Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of InvalidChCsePerson.
    /// </summary>
    [JsonPropertyName("invalidChCsePerson")]
    public CsePerson InvalidChCsePerson
    {
      get => invalidChCsePerson ??= new();
      set => invalidChCsePerson = value;
    }

    /// <summary>
    /// A value of InvalidChAr.
    /// </summary>
    [JsonPropertyName("invalidChAr")]
    public CaseUnit InvalidChAr
    {
      get => invalidChAr ??= new();
      set => invalidChAr = value;
    }

    /// <summary>
    /// A value of Validated.
    /// </summary>
    [JsonPropertyName("validated")]
    public Case1 Validated
    {
      get => validated ??= new();
      set => validated = value;
    }

    /// <summary>
    /// A value of SavingOk.
    /// </summary>
    [JsonPropertyName("savingOk")]
    public Common SavingOk
    {
      get => savingOk ??= new();
      set => savingOk = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of RowCounter.
    /// </summary>
    [JsonPropertyName("rowCounter")]
    public Common RowCounter
    {
      get => rowCounter ??= new();
      set => rowCounter = value;
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// A value of ErrorCase.
    /// </summary>
    [JsonPropertyName("errorCase")]
    public Case1 ErrorCase
    {
      get => errorCase ??= new();
      set => errorCase = value;
    }

    /// <summary>
    /// A value of ErrorRowNumber.
    /// </summary>
    [JsonPropertyName("errorRowNumber")]
    public Common ErrorRowNumber
    {
      get => errorRowNumber ??= new();
      set => errorRowNumber = value;
    }

    /// <summary>
    /// A value of ErrorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("errorCsePersonsWorkSet")]
    public CsePersonsWorkSet ErrorCsePersonsWorkSet
    {
      get => errorCsePersonsWorkSet ??= new();
      set => errorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArGapCommon.
    /// </summary>
    [JsonPropertyName("arGapCommon")]
    public Common ArGapCommon
    {
      get => arGapCommon ??= new();
      set => arGapCommon = value;
    }

    /// <summary>
    /// A value of ArGapCase.
    /// </summary>
    [JsonPropertyName("arGapCase")]
    public Case1 ArGapCase
    {
      get => arGapCase ??= new();
      set => arGapCase = value;
    }

    /// <summary>
    /// A value of ErrorNumber.
    /// </summary>
    [JsonPropertyName("errorNumber")]
    public Common ErrorNumber
    {
      get => errorNumber ??= new();
      set => errorNumber = value;
    }

    /// <summary>
    /// A value of SucessfullyDeleted.
    /// </summary>
    [JsonPropertyName("sucessfullyDeleted")]
    public Common SucessfullyDeleted
    {
      get => sucessfullyDeleted ??= new();
      set => sucessfullyDeleted = value;
    }

    /// <summary>
    /// A value of NumberSelectedRows.
    /// </summary>
    [JsonPropertyName("numberSelectedRows")]
    public Common NumberSelectedRows
    {
      get => numberSelectedRows ??= new();
      set => numberSelectedRows = value;
    }

    /// <summary>
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewCase.
    /// </summary>
    [JsonPropertyName("newCase")]
    public Case1 NewCase
    {
      get => newCase ??= new();
      set => newCase = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public DateWorkArea Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of GenerateEvent.
    /// </summary>
    [JsonPropertyName("generateEvent")]
    public Common GenerateEvent
    {
      get => generateEvent ??= new();
      set => generateEvent = value;
    }

    /// <summary>
    /// A value of NewCsePerson.
    /// </summary>
    [JsonPropertyName("newCsePerson")]
    public CsePerson NewCsePerson
    {
      get => newCsePerson ??= new();
      set => newCsePerson = value;
    }

    /// <summary>
    /// A value of FatherOnCase.
    /// </summary>
    [JsonPropertyName("fatherOnCase")]
    public Common FatherOnCase
    {
      get => fatherOnCase ??= new();
      set => fatherOnCase = value;
    }

    /// <summary>
    /// A value of Dup.
    /// </summary>
    [JsonPropertyName("dup")]
    public CsePerson Dup
    {
      get => dup ??= new();
      set => dup = value;
    }

    /// <summary>
    /// A value of ActiveAp.
    /// </summary>
    [JsonPropertyName("activeAp")]
    public CsePersonsWorkSet ActiveAp
    {
      get => activeAp ??= new();
      set => activeAp = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of RetreivePersonProgram.
    /// </summary>
    [JsonPropertyName("retreivePersonProgram")]
    public DateWorkArea RetreivePersonProgram
    {
      get => retreivePersonProgram ??= new();
      set => retreivePersonProgram = value;
    }

    /// <summary>
    /// A value of OfAge.
    /// </summary>
    [JsonPropertyName("ofAge")]
    public DateWorkArea OfAge
    {
      get => ofAge ??= new();
      set => ofAge = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of NewCaseRole.
    /// </summary>
    [JsonPropertyName("newCaseRole")]
    public CaseRole NewCaseRole
    {
      get => newCaseRole ??= new();
      set => newCaseRole = value;
    }

    /// <summary>
    /// A value of SetNewRelToAr.
    /// </summary>
    [JsonPropertyName("setNewRelToAr")]
    public Common SetNewRelToAr
    {
      get => setNewRelToAr ??= new();
      set => setNewRelToAr = value;
    }

    /// <summary>
    /// A value of RelToArIsCh.
    /// </summary>
    [JsonPropertyName("relToArIsCh")]
    public Common RelToArIsCh
    {
      get => relToArIsCh ??= new();
      set => relToArIsCh = value;
    }

    /// <summary>
    /// A value of ActiveCaseCh.
    /// </summary>
    [JsonPropertyName("activeCaseCh")]
    public Common ActiveCaseCh
    {
      get => activeCaseCh ??= new();
      set => activeCaseCh = value;
    }

    /// <summary>
    /// A value of SuccessfulUpdate.
    /// </summary>
    [JsonPropertyName("successfulUpdate")]
    public Common SuccessfulUpdate
    {
      get => successfulUpdate ??= new();
      set => successfulUpdate = value;
    }

    /// <summary>
    /// A value of SuccessfulAdd.
    /// </summary>
    [JsonPropertyName("successfulAdd")]
    public Common SuccessfulAdd
    {
      get => successfulAdd ??= new();
      set => successfulAdd = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of BlankCaseRole.
    /// </summary>
    [JsonPropertyName("blankCaseRole")]
    public CaseRole BlankCaseRole
    {
      get => blankCaseRole ??= new();
      set => blankCaseRole = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of ErrorCommon.
    /// </summary>
    [JsonPropertyName("errorCommon")]
    public Common ErrorCommon
    {
      get => errorCommon ??= new();
      set => errorCommon = value;
    }

    /// <summary>
    /// A value of NextCaseRole.
    /// </summary>
    [JsonPropertyName("nextCaseRole")]
    public CaseRole NextCaseRole
    {
      get => nextCaseRole ??= new();
      set => nextCaseRole = value;
    }

    /// <summary>
    /// A value of NextCsePerson.
    /// </summary>
    [JsonPropertyName("nextCsePerson")]
    public CsePerson NextCsePerson
    {
      get => nextCsePerson ??= new();
      set => nextCsePerson = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of NewSsnWorkArea.
    /// </summary>
    [JsonPropertyName("newSsnWorkArea")]
    public SsnWorkArea NewSsnWorkArea
    {
      get => newSsnWorkArea ??= new();
      set => newSsnWorkArea = value;
    }

    /// <summary>
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    /// <summary>
    /// A value of OtherChOnCase.
    /// </summary>
    [JsonPropertyName("otherChOnCase")]
    public Common OtherChOnCase
    {
      get => otherChOnCase ??= new();
      set => otherChOnCase = value;
    }

    /// <summary>
    /// A value of ChOnCase.
    /// </summary>
    [JsonPropertyName("chOnCase")]
    public Common ChOnCase
    {
      get => chOnCase ??= new();
      set => chOnCase = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    private CaseRole invalidChCaseRole;
    private Common checkCasesForOtherKid;
    private CsePerson header;
    private CsePerson invalidChCsePerson;
    private CaseUnit invalidChAr;
    private Case1 validated;
    private Common savingOk;
    private Case1 prev;
    private Common rowCounter;
    private Standard pageNumber;
    private Case1 errorCase;
    private Common errorRowNumber;
    private CsePersonsWorkSet errorCsePersonsWorkSet;
    private Common arGapCommon;
    private Case1 arGapCase;
    private Common errorNumber;
    private Common sucessfullyDeleted;
    private Common numberSelectedRows;
    private Office newOffice;
    private ServiceProvider newServiceProvider;
    private Case1 newCase;
    private TextWorkArea textWorkArea;
    private DateWorkArea default1;
    private Common generateEvent;
    private CsePerson newCsePerson;
    private Common fatherOnCase;
    private CsePerson dup;
    private CsePersonsWorkSet activeAp;
    private Case1 case1;
    private CsePerson arCsePerson;
    private Common cse;
    private DateWorkArea retreivePersonProgram;
    private DateWorkArea ofAge;
    private Common fromRole;
    private DateWorkArea null1;
    private DateWorkArea current;
    private CaseRole newCaseRole;
    private Common setNewRelToAr;
    private Common relToArIsCh;
    private Common activeCaseCh;
    private Common successfulUpdate;
    private Common successfulAdd;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private DateWorkArea dateWorkArea;
    private CaseRole blankCaseRole;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Common errorCommon;
    private CaseRole nextCaseRole;
    private CsePerson nextCsePerson;
    private AbendData abendData;
    private Common common;
    private SsnWorkArea newSsnWorkArea;
    private Infrastructure lastTran;
    private Common otherChOnCase;
    private Common chOnCase;
    private Common found;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CaseRole Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    /// <summary>
    /// A value of Delete.
    /// </summary>
    [JsonPropertyName("delete")]
    public CaseRole Delete
    {
      get => delete ??= new();
      set => delete = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of ChildPaternityEstInd.
    /// </summary>
    [JsonPropertyName("childPaternityEstInd")]
    public CsePerson ChildPaternityEstInd
    {
      get => childPaternityEstInd ??= new();
      set => childPaternityEstInd = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private GeneticTest geneticTest;
    private CaseRole child;
    private CaseRole mother;
    private CaseRole delete;
    private CsePersonAccount obligee;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson childPaternityEstInd;
    private CsePerson csePerson;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}

// Program: SP_OFCA_OFC_CASELOD_ASGNMT_MAINT, ID: 372558713, model: 746.
// Short name: SWEOFCAP
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
/// A program: SP_OFCA_OFC_CASELOD_ASGNMT_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpOfcaOfcCaselodAsgnmtMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_OFCA_OFC_CASELOD_ASGNMT_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpOfcaOfcCaselodAsgnmtMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpOfcaOfcCaselodAsgnmtMaint.
  /// </summary>
  public SpOfcaOfcCaselodAsgnmtMaint(IContext context, Import import,
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
    // ************************************************************************
    // **   Date    Developer  PR #     Description
    // ** --------  ---------  ----     -----------
    // ** 04/13/95  J. Kemp       Initial development
    // ** 02/28/96  a. hackler    retro fits
    // ** 12/26/96  J. Rookard    Refit for new security and new Service Plan
    // **                         architecture.
    // ** 01/07/96  J. Rookard    Continue refit for new Service Plan 
    // architecture.
    // ** 01/14/97  J. Rookard    Continue refit for new Service Plan 
    // architecture.
    // ** 03/28/97  J. Rookard    Add validation edit to prevent updating or
    // **                         creating Office
    // **                         Caseload Assignment occurrences for 
    // discontinued
    // **                         Offices.
    // **                         Add code to support additional flow to
    // **                         Code Values List for select and return of
    // **                         Office Caseload Assignment Type.
    // ** 04/15/97  J. Rookard    Add validation to prevent creation when no
    // **                         OSP provided.
    // **                         Add validation to prevent creation when no
    // **                         matching criteria provided.
    // **                         Fix sel column to hold S value when returning
    // **                         from various dialog flows.
    // ** 04/21/97  J. Rookard    Enhance and clarify command of Display logic 
    // to
    // **                         provide appropriate exit state messages and to
    // **                         sort on Service Provider last name prior to 
    // sort
    // **                         on alpha. Clarify commands of Copy, Confirm,
    // **                         and List.
    // ** 08/07/97  J. Rookard    modify flow between OFCA and PRGM to allow 
    // only
    // **                         certain values to be selected on PRGM for 
    // return
    // **                         to OFCA.
    // ** 08/14/97  J. Rookard    modify add processing to disallow create with 
    // a
    // **                         discontinued Office Service Provider
    // ** 08/27/97  J. Rookard    add validation routines for Program and
    // **                         Function values on add and update commands.
    // ** 09/17/97  J. Rookard    debug add and update commands for Program.
    // ** 10/14/99  SWSRCHF  H00077184  Highlight the Function field when in 
    // error
    // ** 10/14/99  SWSRCHF  H00077203  Control access to Service Provider 
    // profile
    // **
    // 
    // maintenance
    // ** 12/11/00  SWSRCHF  I00109102  Error message displayed incorrectly 
    // after using
    // **
    // 
    // the county picklist.
    // ** 12/21/00  SWSRCHF    000265   Increase length of Last Name(s) in the 
    // ALPHA THRU
    // **
    // 
    // area of the screen
    // ** 12/21/00  SWSRCHF    000266   Incorrect status for referral after 
    // update function
    // ** 01/29/01  SWSRCHF  I00112351  Add/Update message not displayed after 
    // function
    // PR171236. Change conditions for read for overlap. Put in a condition code
    // becuase the table being read contains old information that invalidates
    // the read. Put a chekc in for not = to a if the assignment type is CA.
    // LBachura 3-28-03
    // ***************************************************************************************
    // *                              
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 12/22/2009  Raj S              CQ14547     Modified to fix READ EACH 
    // Loop problem.  *
    // *
    // 
    // After the upgrade the program started    *
    // *
    // 
    // reading the new records after opening the*
    // *
    // 
    // cursor.  To fix this problem, timestamp  *
    // *
    // 
    // value is used to skip the new records.   *
    // *
    // 
    // *
    // * 08/16/2013  GVandy             CQ38147     Replace assignments by 
    // county with       *
    // *
    // 
    // assignments by tribunal.                 *
    // *
    // 
    // *
    // * 11/04/2013  GVandy             CQ41845     Change display order to 
    // priority,        *
    // *
    // 
    // program, tribunal, function, and alpha.  *
    // *
    // 
    // *
    // * 01/06/2014  GVandy             CQ42390     Correct Update logic 
    // allowing assignments*
    // *
    // 
    // to end dated service provider roles.     *
    // *
    // 
    // *
    // * 09/05/2018  R.Mathews          CQ60203     Allow referral assignment by
    // alpha       *
    // *
    // 
    // and/or program                           *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Office.Assign(import.Office);
    export.HiddenOffice.SystemGeneratedId =
      import.HiddenOffice.SystemGeneratedId;
    export.OfficeAddress.City = import.OfficeAddress.City;
    export.OfficeType.Description = import.OfficeType.Description;
    export.InactiveOffice.Flag = import.InactiveOffice.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "CANCEL") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "REDIST") || Equal
      (global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    export.ServiceProvider.Assign(import.ServiceProvider);
    export.Priority.Priority = import.Priority.Priority;
    export.Search.BeginingAlpha = import.Search.BeginingAlpha;
    export.TypeSearch.AssignmentType = import.TypeSearch.AssignmentType;
    export.ListOffice.Flag = import.ListOffice.Flag;
    export.ListAsgnType.Flag = import.ListAsgnType.Flag;

    // *** Work request 000265
    // *** 12/21/00 swsrchf
    export.OfficeCaseloadAssignment.AssignmentIndicator =
      import.OfficeCaseloadAssignment.AssignmentIndicator;

    if (Equal(global.Command, "PREVIEW"))
    {
      export.Confirm.Flag = import.Confirm.Flag;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.OfficeCaseloadAssignment.Assign(
        import.Import1.Item.OfficeCaseloadAssignment);
      export.Export1.Update.HiddenOfficeCaseloadAssignment.Assign(
        import.Import1.Item.HiddenOfficeCaseloadAssignment);
      MoveProgram(import.Import1.Item.Program, export.Export1.Update.Program);
      MoveProgram(import.Import1.Item.HiddenProgram,
        export.Export1.Update.HiddenProgram);
      export.Export1.Update.Tribunal.Identifier =
        import.Import1.Item.Tribunal.Identifier;
      export.Export1.Update.HiddenTribunal.Identifier =
        import.Import1.Item.HiddenTribunal.Identifier;
      export.Export1.Update.ServiceProvider.Assign(
        import.Import1.Item.ServiceProvider);
      export.Export1.Update.HiddenServiceProvider.Assign(
        import.Import1.Item.HiddenServiceProvider);
      MoveOfficeServiceProvider(import.Import1.Item.OfficeServiceProvider,
        export.Export1.Update.OfficeServiceProvider);
      export.Export1.Update.ListTribunal.Flag =
        import.Import1.Item.ListTribunal.Flag;
      export.Export1.Update.ListProgram.Flag =
        import.Import1.Item.ListProgram.Flag;
      export.Export1.Update.ListServiceProvider.Flag =
        import.Import1.Item.ListServiceProvider.Flag;
      export.Export1.Update.ListFunction.Flag =
        import.Import1.Item.ListFunction.Flag;
      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // **** Validate All Commands  ****
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        if (AsChar(export.InactiveOffice.Flag) == 'Y')
        {
          ExitState = "SP0000_INVALID_COM_INACT_OFFCE";
        }

        break;
      case "CANCEL":
        if (AsChar(export.InactiveOffice.Flag) == 'Y')
        {
          ExitState = "SP0000_INVALID_COM_INACT_OFFCE";

          break;
        }
        else if (IsEmpty(export.InactiveOffice.Flag))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
        }
        else
        {
          var field1 = GetField(export.TypeSearch, "assignmentType");

          field1.Error = true;

          ExitState = "INVALID_WITH_TYPE_CODE";

          break;
        }

        // *********************************************
        // Read each Caseload Assignment occurrence where the Assignment 
        // Indicator is not equal to A and the Assignment Type is equal to CA (
        // for Case).  Delete occurrences with these values.
        // *********************************************
        // *********************************************
        // READ EACH ASSIGNMENT WHERE ASSIGNMENT INDICATOR IS  not = A and 
        // ASSIGNMENT TYPE = CA.
        // *********************************************
        foreach(var item in ReadOfficeCaseloadAssignmentOfficeServiceProvider1())
          
        {
          if (!Equal(entities.OfficeCaseloadAssignment.AssignmentType,
            export.TypeSearch.AssignmentType))
          {
            continue;
          }

          if (!Lt(local.Current.Date,
            entities.OfficeCaseloadAssignment.DiscontinueDate))
          {
            continue;
          }

          if (AsChar(entities.OfficeCaseloadAssignment.AssignmentIndicator) != 'A'
            )
          {
            DeleteOfficeCaseloadAssignment();

            continue;
          }
        }

        global.Command = "DISPLAY";
        ExitState = "SP0000_REDISTRIBUTION_CANCELED";

        return;
      case "COPY":
        if (AsChar(export.InactiveOffice.Flag) == 'Y')
        {
          ExitState = "SP0000_INVALID_COM_INACT_OFFCE";

          break;
        }
        else if (IsEmpty(export.InactiveOffice.Flag))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
        }
        else
        {
          var field1 = GetField(export.TypeSearch, "assignmentType");

          field1.Error = true;

          ExitState = "INVALID_WITH_TYPE_CODE";

          return;
        }

        // *** start
        // *** 10/14/99 SWSRCHF
        // *** H00077203
        // ** Control access to Service Provider profile maintenance
        // *** 10/14/99 SWSRCHF
        // *** H00077203
        // *** end
        // *********************************************
        // Read each Caseload Assignment occurrence where the Assignment 
        // Indicator is not equal to A and the Assignment Type is equal to CA (
        // for Case).  Delete occurrences with these values.
        // *********************************************
        // *********************************************
        // READ EACH ASSIGNMENT WHERE ASSIGNMENT INDICATOR IS = A and ASSIGNMENT
        // TYPE = CA.
        // *********************************************
        // ***************************************************************************************
        // * CQ14547 Changes
        // 
        // *
        // ***************************************************************************************
        local.TsBeforeCreate.CreatedTimestamp = Now();

        foreach(var item in ReadOfficeCaseloadAssignmentOfficeServiceProvider1())
          
        {
          if (!Equal(entities.OfficeCaseloadAssignment.AssignmentType,
            export.TypeSearch.AssignmentType))
          {
            continue;
          }

          if (!Lt(local.Current.Date,
            entities.OfficeCaseloadAssignment.DiscontinueDate))
          {
            continue;
          }

          if (AsChar(entities.OfficeCaseloadAssignment.AssignmentIndicator) != 'A'
            )
          {
            // ***************************************************************************************
            // * CQ14547  Changes
            // 
            // *
            // ***************************************************************************************
            if (Lt(entities.OfficeCaseloadAssignment.CreatedTimestamp,
              local.TsBeforeCreate.CreatedTimestamp))
            {
              DeleteOfficeCaseloadAssignment();
            }

            continue;
          }

          local.OfficeCaseloadAssignment.Assign(
            entities.OfficeCaseloadAssignment);

          // *********************************************
          // SET REDISTRIBUTE INDICATOR TO (P)review
          // *********************************************
          local.OfficeCaseloadAssignment.AssignmentIndicator = "P";

          if (ReadProgram2())
          {
            MoveProgram(entities.Program, local.Program);
          }
          else
          {
            MoveProgram(local.InitProgram, local.Program);
          }

          if (ReadTribunal2())
          {
            local.Tribunal.Identifier = entities.Tribunal.Identifier;
          }
          else
          {
            local.Tribunal.Identifier = local.InitTribunal.Identifier;
          }

          UseSpCreateOfficeCaseloadAssign1();
        }

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          ExitState = "SP0000_COPY_SUCC_A_TO_R";
        }
        else
        {
          break;
        }

        global.Command = "DISPLAY";

        break;
      case "DELETE":
        if (AsChar(export.InactiveOffice.Flag) == 'Y')
        {
          ExitState = "SP0000_INVALID_COM_INACT_OFFCE";
        }

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
            var field1 = GetField(export.Standard, "nextTransaction");

            field1.Error = true;

            break;
          }

          return;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "HELP":
        global.Command = "DISPLAY";

        break;
      case "LIST":
        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "REDIST":
        if (AsChar(export.InactiveOffice.Flag) == 'Y')
        {
          ExitState = "SP0000_INVALID_COM_INACT_OFFCE";

          break;
        }
        else if (IsEmpty(export.InactiveOffice.Flag))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
        }
        else
        {
          var field1 = GetField(export.TypeSearch, "assignmentType");

          field1.Error = true;

          ExitState = "INVALID_WITH_TYPE_CODE";

          break;
        }

        foreach(var item in ReadOfficeCaseloadAssignment1())
        {
          // *******************************************
          // READ EACH CA TYPE ASSIGNMENT NOT EQUAL TO ACTIVE AND UPDATE TO 
          // REDISTRIBUTE.
          // *******************************************
          try
          {
            UpdateOfficeCaseloadAssignment2();
            ExitState = "SP0000_AUD_OK_CSLD_BTCH_MUST_RUN";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_NU";

                continue;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_PV";

                continue;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        global.Command = "DISPLAY";
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "RETCDVL":
        // Returning on a link from CDVL - List Code Values.
        if (AsChar(export.ListAsgnType.Flag) == 'S')
        {
          export.ListAsgnType.Flag = "";

          var field1 = GetField(export.ListAsgnType, "flag");

          field1.Protected = false;
          field1.Focused = true;

          if (!IsEmpty(import.HiddenSelectionCodeValue.Cdvalue))
          {
            export.TypeSearch.AssignmentType =
              import.HiddenSelectionCodeValue.Cdvalue;
          }

          if (export.Office.SystemGeneratedId == 0)
          {
            return;
          }
          else
          {
            global.Command = "DISPLAY";
          }
        }
        else
        {
          // Returning on a link from CDVL - List Code Values - for FUNCTION.
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.ListFunction.Flag) != 'S')
            {
              continue;
            }

            if (IsEmpty(import.HiddenSelectionCodeValue.Cdvalue))
            {
              var field1 = GetField(export.Export1.Item.Common, "selectChar");

              field1.Protected = false;
              field1.Focused = false;

              export.Export1.Update.ListFunction.Flag = "";

              var field2 = GetField(export.Export1.Item.ListFunction, "flag");

              field2.Protected = false;
              field2.Focused = true;

              break;
            }

            export.Export1.Update.OfficeCaseloadAssignment.Function =
              import.HiddenSelectionCodeValue.Cdvalue;

            // **** Validate function and office combination ****
            local.Validate.Flag = "";

            foreach(var item in ReadCountyService4())
            {
              if (Equal(entities.CountyService.Function,
                export.Export1.Item.OfficeCaseloadAssignment.Function))
              {
                local.Validate.Flag = "Y";

                break;
              }
            }

            if (AsChar(local.Validate.Flag) == 'Y')
            {
              var field1 = GetField(export.Export1.Item.Common, "selectChar");

              field1.Protected = false;
              field1.Focused = false;

              export.Export1.Update.ListFunction.Flag = "";

              var field2 = GetField(export.Export1.Item.ListFunction, "flag");

              field2.Protected = false;
              field2.Focused = true;
            }
            else
            {
              var field1 = GetField(export.Export1.Item.Common, "selectChar");

              field1.Color = "red";
              field1.Protected = false;
              field1.Focused = false;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "function");

              field2.Color = "red";
              field2.Protected = false;
              field2.Focused = false;

              var field3 = GetField(export.Export1.Item.ListFunction, "flag");

              field3.Error = true;

              ExitState = "SP0000_INVALID_OFFICE_FUNCTION";
            }

            break;
          }
        }

        break;
      case "RETLTRB":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.ListTribunal.Flag) != 'S')
          {
            continue;
          }

          if (import.FromLtrb.Identifier == 0)
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Protected = false;

            export.Export1.Update.ListTribunal.Flag = "";

            var field2 = GetField(export.Export1.Item.ListTribunal, "flag");

            field2.Protected = false;
            field2.Focused = true;

            break;
          }

          export.Export1.Update.Tribunal.Identifier =
            import.FromLtrb.Identifier;

          // **** Validate Tribunal COUNTY and office combination ****
          local.Validate.Flag = "";

          if (ReadCountyService3())
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Protected = false;

            export.Export1.Update.ListTribunal.Flag = "";

            var field2 = GetField(export.Export1.Item.ListTribunal, "flag");

            field2.Protected = false;
            field2.Focused = true;
          }
          else
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Color = "red";
            field1.Protected = false;

            var field2 = GetField(export.Export1.Item.Tribunal, "identifier");

            field2.Color = "red";
            field2.Protected = false;

            var field3 = GetField(export.Export1.Item.ListTribunal, "flag");

            field3.Error = true;

            ExitState = "SP0000_INVALID_TRIBUNAL_COUNTY";
          }

          break;
        }

        break;
      case "RETOFCL":
        // Returning on a link from OFCL - List Office.
        export.ListOffice.Flag = "";

        var field = GetField(export.ListOffice, "flag");

        field.Protected = false;
        field.Focused = true;

        if (import.SelectedOffice.SystemGeneratedId != 0)
        {
          export.Office.Assign(import.SelectedOffice);
          export.OfficeAddress.City = import.SelectedOfficeAddress.City;
          global.Command = "DISPLAY";
        }

        break;
      case "RETOFPR":
        global.Command = "DISPLAY";

        break;
      case "RETPRGM":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.ListProgram.Flag) != 'S')
          {
            continue;
          }

          if (import.HiddenSelectionProgram.SystemGeneratedIdentifier == 0)
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Protected = false;

            export.Export1.Update.ListProgram.Flag = "";

            var field2 = GetField(export.Export1.Item.ListProgram, "flag");

            field2.Protected = false;
            field2.Focused = true;

            break;
          }

          MoveProgram(import.HiddenSelectionProgram,
            export.Export1.Update.Program);

          // **** Validate Program and office combination ****
          local.Validate.Flag = "";

          foreach(var item in ReadProgram4())
          {
            if (Equal(entities.Program.Code, export.Export1.Item.Program.Code))
            {
              local.Validate.Flag = "Y";

              break;
            }
          }

          if (AsChar(local.Validate.Flag) == 'Y')
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = false;

            export.Export1.Update.ListProgram.Flag = "";

            var field2 = GetField(export.Export1.Item.ListProgram, "flag");

            field2.Protected = false;
            field2.Focused = true;
          }
          else
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Color = "red";
            field1.Protected = false;

            var field2 = GetField(export.Export1.Item.Program, "code");

            field2.Color = "red";
            field2.Protected = false;

            var field3 = GetField(export.Export1.Item.Common, "selectChar");

            field3.Error = true;

            ExitState = "SP0000_INVALID_OFFICE_PROGRAM";
          }

          break;
        }

        break;
      case "RETSVPO":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.ListServiceProvider.Flag) != 'S')
          {
            continue;
          }

          if (IsEmpty(import.HiddenSelectionOfficeServiceProvider.RoleCode))
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = false;

            export.Export1.Update.ListServiceProvider.Flag = "";

            var field2 =
              GetField(export.Export1.Item.ListServiceProvider, "flag");

            field2.Protected = false;
            field2.Focused = true;

            break;
          }

          export.Export1.Update.ServiceProvider.Assign(
            import.HiddenSelectionServiceProvider);
          MoveOfficeServiceProvider(import.HiddenSelectionOfficeServiceProvider,
            export.Export1.Update.OfficeServiceProvider);

          // **** Validate Service Provider and office combination ****
          local.Validate.Flag = "";

          foreach(var item in ReadServiceProviderOfficeServiceProvider())
          {
            if (entities.ServiceProvider.SystemGeneratedId == export
              .Export1.Item.ServiceProvider.SystemGeneratedId)
            {
              local.Validate.Flag = "Y";

              break;
            }
          }

          if (AsChar(local.Validate.Flag) == 'Y')
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Protected = false;

            export.Export1.Update.ListServiceProvider.Flag = "";

            var field2 =
              GetField(export.Export1.Item.ListServiceProvider, "flag");

            field2.Protected = false;
            field2.Focused = true;
          }
          else
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Color = "red";
            field1.Protected = false;

            var field2 =
              GetField(export.Export1.Item.ListServiceProvider, "flag");

            field2.Error = true;

            ExitState = "SP0000_INVALID_OFFICE_SRV_PRVDR";
          }

          break;
        }

        break;
      case "RETURN":
        global.Command = "DISPLAY";
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "UPDATE":
        if (AsChar(export.InactiveOffice.Flag) == 'Y')
        {
          ExitState = "SP0000_INVALID_COM_INACT_OFFCE";
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

    // ****  Security Test **** PR 138744 on 2-21-2002 to add cancel and delete 
    // for the programmer profile project. LBachura
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "CANCEL") || Equal
      (global.Command, "REDIST"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        goto Test1;
      }

      if (Equal(global.Command, "DISPLAY"))
      {
        goto Test1;
      }

      if (!Equal(global.Command, "LIST"))
      {
        if (export.Office.SystemGeneratedId == 0)
        {
          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          return;
        }

        if (IsEmpty(export.TypeSearch.AssignmentType))
        {
          var field = GetField(export.TypeSearch, "assignmentType");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA") || Equal
          (export.TypeSearch.AssignmentType, "RE"))
        {
        }
        else
        {
          export.CodeValue.Cdvalue = export.TypeSearch.AssignmentType;
          export.Code.CodeName = "ASSIGNMENT TYPE";
          UseCabValidateCodeValue();

          switch(local.ReturnCode.Count)
          {
            case 0:
              // Input code value valid.
              break;
            case 1:
              // Invalid code name.
              ExitState = "CO0000_INVALID_CODE";

              return;
            case 2:
              // Invalid code value
              ExitState = "ACO_NE0000_INVALID_CODE";

              var field =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "assignmentType");

              field.Error = true;

              return;
            default:
              break;
          }
        }
      }

      // **** Validate Selections ****
      local.FilterCount.Count = 0;
      local.SelCount.Count = 0;
      local.PromptCount.Count = 0;

      if (!IsEmpty(export.ListAsgnType.Flag))
      {
        var field = GetField(export.ListAsgnType, "flag");

        field.Error = true;

        local.FilterCount.Count = 1;
      }

      if (!IsEmpty(export.ListOffice.Flag))
      {
        var field = GetField(export.ListOffice, "flag");

        field.Error = true;

        ++local.FilterCount.Count;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ++local.SelCount.Count;
        }

        if (!IsEmpty(export.Export1.Item.ListTribunal.Flag))
        {
          var field = GetField(export.Export1.Item.ListTribunal, "flag");

          field.Error = true;

          ++local.PromptCount.Count;
        }

        if (!IsEmpty(export.Export1.Item.ListFunction.Flag))
        {
          var field = GetField(export.Export1.Item.ListFunction, "flag");

          field.Error = true;

          ++local.PromptCount.Count;
        }

        if (!IsEmpty(export.Export1.Item.ListProgram.Flag))
        {
          var field = GetField(export.Export1.Item.ListProgram, "flag");

          field.Error = true;

          ++local.PromptCount.Count;
        }

        if (!IsEmpty(export.Export1.Item.ListServiceProvider.Flag))
        {
          var field = GetField(export.Export1.Item.ListServiceProvider, "flag");

          field.Error = true;

          ++local.PromptCount.Count;
        }
      }

      switch(local.FilterCount.Count)
      {
        case 0:
          break;
        case 1:
          local.FilterSelPrompt.Count = 100;

          break;
        default:
          local.FilterSelPrompt.Count = 200;

          break;
      }

      switch(local.SelCount.Count)
      {
        case 0:
          break;
        case 1:
          local.FilterSelPrompt.Count += 10;

          break;
        default:
          local.FilterSelPrompt.Count += 20;

          break;
      }

      switch(local.PromptCount.Count)
      {
        case 0:
          break;
        case 1:
          ++local.FilterSelPrompt.Count;

          break;
        default:
          local.FilterSelPrompt.Count += 2;

          break;
      }

      switch(local.FilterSelPrompt.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        case 1:
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "SP0000_REQUEST_REQUIRES_SEL";

            return;
          }
          else
          {
            ExitState = "SP0000_PROMPT_NOT_ALLOWED";

            return;
          }

          break;
        case 2:
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        case 10:
          if (Equal(global.Command, "ADD") || Equal
            (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }

          break;
        case 11:
          if (Equal(global.Command, "LIST"))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";

            return;
          }

          break;
        case 20:
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        case 100:
          if (Equal(global.Command, "LIST"))
          {
          }
          else
          {
            ExitState = "INVALID_OPTION_SELECTED";

            return;
          }

          break;
        case 101:
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        default:
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
      }
    }

Test1:

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // ****  Validate data ****
      if (ReadOfficeAssignmentPlan())
      {
        // Ok, currency on Office Assignment Plan acquired.
      }
      else
      {
        ExitState = "OFFICE_ASSIGNMENT_TABLE_NF";

        return;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          continue;
        }

        if (AsChar(export.Export1.Item.Common.SelectChar) != 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          goto Test4;
        }

        switch(TrimEnd(global.Command))
        {
          case "ADD":
            // **** ADDs has to on a blank rec  *****
            if (export.Export1.Item.HiddenServiceProvider.SystemGeneratedId != 0
              )
            {
              ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

              return;
            }

            break;
          case "UPDATE":
            if (export.Export1.Item.HiddenServiceProvider.SystemGeneratedId == 0
              )
            {
              ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

              return;
            }

            // **** Update  - one field must have changed  *****
            if (export.Export1.Item.OfficeCaseloadAssignment.Priority != export
              .Export1.Item.HiddenOfficeCaseloadAssignment.Priority)
            {
            }
            else if (!Equal(export.Export1.Item.OfficeCaseloadAssignment.
              AssignmentType,
              export.Export1.Item.HiddenOfficeCaseloadAssignment.
                AssignmentType))
            {
            }
            else if (!Equal(export.Export1.Item.OfficeCaseloadAssignment.
              BeginingAlpha,
              export.Export1.Item.HiddenOfficeCaseloadAssignment.
                BeginingAlpha))
            {
            }
            else if (!Equal(export.Export1.Item.OfficeCaseloadAssignment.
              EndingAlpha,
              export.Export1.Item.HiddenOfficeCaseloadAssignment.EndingAlpha))
            {
            }
            else if (!Equal(export.Export1.Item.OfficeCaseloadAssignment.
              Function,
              export.Export1.Item.HiddenOfficeCaseloadAssignment.Function))
            {
            }
            else if (export.Export1.Item.ServiceProvider.SystemGeneratedId != export
              .Export1.Item.HiddenServiceProvider.SystemGeneratedId)
            {
            }
            else if (!Equal(export.Export1.Item.OfficeServiceProvider.RoleCode,
              export.Export1.Item.OfficeServiceProvider.RoleCode))
            {
            }
            else if (export.Export1.Item.Tribunal.Identifier != export
              .Export1.Item.HiddenTribunal.Identifier)
            {
            }
            else if (!Equal(export.Export1.Item.Program.Code,
              export.Export1.Item.HiddenProgram.Code))
            {
            }
            else
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "SP0000_DATA_NOT_CHANGED";

              return;
            }

            break;
          default:
            break;
        }

        if (IsEmpty(export.Export1.Item.OfficeServiceProvider.RoleCode) || export
          .Export1.Item.ServiceProvider.SystemGeneratedId == 0)
        {
          var field1 =
            GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

          field1.Error = true;

          var field2 =
            GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");

          field2.Error = true;

          var field3 =
            GetField(export.Export1.Item.ServiceProvider, "lastName");

          field3.Error = true;

          var field4 =
            GetField(export.Export1.Item.ServiceProvider, "firstName");

          field4.Error = true;

          var field5 = GetField(export.Export1.Item.Common, "selectChar");

          field5.Color = "red";
          field5.Protected = false;
          field5.Focused = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }
        else
        {
        }

        if (IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.BeginingAlpha) &&
          IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.EndingAlpha) && IsEmpty
          (export.Export1.Item.Program.Code) && export
          .Export1.Item.Tribunal.Identifier == 0 && IsEmpty
          (export.Export1.Item.OfficeCaseloadAssignment.Function))
        {
          var field1 =
            GetField(export.Export1.Item.OfficeCaseloadAssignment,
            "beginingAlpha");

          field1.Error = true;

          var field2 =
            GetField(export.Export1.Item.OfficeCaseloadAssignment, "endingAlpha");
            

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.Tribunal, "identifier");

          field3.Error = true;

          var field4 =
            GetField(export.Export1.Item.OfficeCaseloadAssignment, "function");

          field4.Error = true;

          var field5 = GetField(export.Export1.Item.Program, "code");

          field5.Error = true;

          var field6 = GetField(export.Export1.Item.Common, "selectChar");

          field6.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        if (ReadServiceProvider())
        {
          // Okay, currency on Service Provider acquired.
          export.Export1.Update.ServiceProvider.
            Assign(entities.ServiceProvider);
        }
        else
        {
          var field =
            GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }

        if (ReadOfficeServiceProvider1())
        {
          // Okay, currency on Office Service Provider acquired.
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.Export1.Update.OfficeServiceProvider);

          if (!Lt(local.Current.Date,
            entities.OfficeServiceProvider.EffectiveDate) && !
            Lt(entities.OfficeServiceProvider.DiscontinueDate,
            local.Current.Date))
          {
          }
          else
          {
          }
        }
        else if (ReadOfficeServiceProvider2())
        {
          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          var field1 = GetField(export.Export1.Item.Common, "selectChar");

          field1.Error = true;

          var field2 =
            GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

          field2.Error = true;

          return;
        }
        else
        {
          var field1 =
            GetField(export.Export1.Item.ServiceProvider, "systemGeneratedId");

          field1.Error = true;

          var field2 =
            GetField(export.Export1.Item.OfficeServiceProvider, "roleCode");

          field2.Error = true;

          ExitState = "SP0000_SP_ROLE_COMB_NF_IN_OFCE";

          return;
        }

        if (export.Export1.Item.Tribunal.Identifier != export
          .Export1.Item.HiddenTribunal.Identifier && export
          .Export1.Item.Tribunal.Identifier != 0)
        {
          if (AsChar(entities.OfficeAssignmentPlan.TribunalInd) != 'Y')
          {
            ExitState = "INVALID_OFFICE_ASSIGNMENT_TRIB";

            var field = GetField(export.Export1.Item.Tribunal, "identifier");

            field.Error = true;

            return;
          }
          else if (ReadTribunal1())
          {
            if (ReadCountyService1())
            {
              local.Priority3.Flag = "Y";
            }
            else
            {
              ExitState = "SP0000_INVALID_TRIBUNAL_COUNTY";

              var field = GetField(export.Export1.Item.Tribunal, "identifier");

              field.Error = true;

              return;
            }
          }
          else
          {
            ExitState = "TRIBUNAL_NF";

            var field = GetField(export.Export1.Item.Tribunal, "identifier");

            field.Error = true;

            return;
          }
        }
        else if (export.Export1.Item.Tribunal.Identifier == 0)
        {
          export.Export1.Update.Tribunal.Identifier =
            local.InitTribunal.Identifier;

          // @@@
        }
        else
        {
          local.Priority3.Flag = "Y";
        }

        if (!Equal(export.Export1.Item.Program.Code,
          export.Export1.Item.HiddenProgram.Code) && !
          IsEmpty(export.Export1.Item.Program.Code))
        {
          if (AsChar(entities.OfficeAssignmentPlan.ProgramAssignmentInd) != 'Y')
          {
            ExitState = "INVALID_OFFICE_ASSIGN_PROGRAM";

            var field = GetField(export.Export1.Item.Program, "code");

            field.Error = true;

            return;
          }
          else
          {
            if (ReadProgram1())
            {
              MoveProgram(entities.Program, export.Export1.Update.Program);
              local.Priority1.Flag = "Y";
            }
            else
            {
              var field = GetField(export.Export1.Item.Program, "code");

              field.Error = true;

              ExitState = "SP0000_PROGRAM_NOT_SERVICED";

              return;
            }

            // **** Verify that program is serviced by office ****
          }
        }
        else if (IsEmpty(export.Export1.Item.Program.Code))
        {
          MoveProgram(local.Program, export.Export1.Update.Program);
        }
        else
        {
          local.Priority1.Flag = "Y";
        }

        if (!Equal(export.Export1.Item.OfficeCaseloadAssignment.Function,
          export.Export1.Item.HiddenOfficeCaseloadAssignment.Function) && !
          IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.Function))
        {
          if (AsChar(entities.OfficeAssignmentPlan.FunctionAssignmentInd) != 'Y'
            )
          {
            ExitState = "INVALID_OFFICE_ASSIGN_TASK";

            var field =
              GetField(export.Export1.Item.OfficeCaseloadAssignment, "function");
              

            field.Error = true;

            return;
          }
          else if (ReadCountyService2())
          {
            local.Priority2.Flag = "Y";
          }
          else
          {
            ExitState = "SP0000_FUNCTION_NOT_PERFORMED";

            var field =
              GetField(export.Export1.Item.OfficeCaseloadAssignment, "function");
              

            field.Error = true;

            return;
          }
        }
        else if (Equal(export.Export1.Item.OfficeCaseloadAssignment.Function,
          export.Export1.Item.OfficeCaseloadAssignment.Function) && !
          IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.Function))
        {
          local.Priority2.Flag = "Y";
        }

        if (!IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.BeginingAlpha) &&
          !IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.EndingAlpha))
        {
          if (AsChar(entities.OfficeAssignmentPlan.AlphaAssignmentInd) != 'Y')
          {
            ExitState = "INVALID_OFFICE_ASSIGN_ALPHA";

            var field1 =
              GetField(export.Export1.Item.OfficeCaseloadAssignment,
              "beginingAlpha");

            field1.Error = true;

            var field2 =
              GetField(export.Export1.Item.OfficeCaseloadAssignment,
              "endingAlpha");

            field2.Error = true;

            return;
          }
          else
          {
            // **** Validate only Alpha characters entered ****
            local.SelCount.Count = 0;
            local.PromptCount.Count = 0;
            local.PromptCount.Count =
              Verify(TrimEnd(
                export.Export1.Item.OfficeCaseloadAssignment.BeginingAlpha),
              "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            if (local.PromptCount.Count != 0)
            {
              local.SelCount.Count = 1;
              ExitState = "ACO_NE0000_MUST_BE_ALPHA";

              var field =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field.Error = true;
            }

            local.PromptCount.Count = 0;
            local.PromptCount.Count =
              Verify(TrimEnd(
                export.Export1.Item.OfficeCaseloadAssignment.EndingAlpha),
              "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            local.PromptCount.Count =
              Verify(TrimEnd(
                export.Export1.Item.OfficeCaseloadAssignment.EndingAlpha),
              "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            if (local.PromptCount.Count != 0)
            {
              local.SelCount.Count = 1;
              ExitState = "ACO_NE0000_MUST_BE_ALPHA";

              var field =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field.Error = true;
            }

            if (local.SelCount.Count != 0)
            {
              return;
            }

            if (Lt(export.Export1.Item.OfficeCaseloadAssignment.EndingAlpha,
              export.Export1.Item.OfficeCaseloadAssignment.BeginingAlpha))
            {
              ExitState = "BEGNG_ALPHA_MUST_BE_LESS_THN_END";

              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field2.Error = true;

              return;
            }
          }

          local.Priority4.Flag = "Y";
        }

        if (Equal(export.Export1.Item.OfficeCaseloadAssignment.EffectiveDate,
          local.Null1.Date))
        {
          export.Export1.Update.OfficeCaseloadAssignment.EffectiveDate =
            local.Current.Date;
        }

        if (Equal(export.Export1.Item.OfficeCaseloadAssignment.DiscontinueDate,
          local.Null1.Date))
        {
          export.Export1.Update.OfficeCaseloadAssignment.DiscontinueDate =
            UseCabSetMaximumDiscontinueDate();
        }

        if (!Lt(export.Export1.Item.OfficeCaseloadAssignment.EffectiveDate,
          export.Export1.Item.OfficeCaseloadAssignment.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_END_LESS_THAN_EFF";

          return;
        }

        // **** Verify correct priorities
        // 
        // ****
        // **** When Beg. Alpha = A and ending Alpha = ZZZZZZ then            **
        // **
        // **** Priority can = 4 or a 5 - at least one priority 5 is required **
        // **
        // **** per office - this is the bit bucket                           **
        // **
        // **** If Program exists then priority = 1                           **
        // **
        // **** If Function exists then priority = 2                          **
        // **
        // **** If County exists then priority = 3                            **
        // **
        // **** If Alpha only exists then priority = 4                        **
        // **
        // **** Always use the highest priority Hi = 1 low = 5                **
        // **
        switch(export.Export1.Item.OfficeCaseloadAssignment.Priority)
        {
          case 1:
            if (AsChar(local.Priority1.Flag) != 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Program, "code");

              field2.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }

            break;
          case 2:
            if (AsChar(local.Priority1.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Program, "code");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "function");

              field3.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority2.Flag) != 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "function");

              field2.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }

            break;
          case 3:
            if (AsChar(local.Priority1.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Program, "code");

              field2.Error = true;

              var field3 = GetField(export.Export1.Item.Tribunal, "identifier");

              field3.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority2.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "function");

              field2.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority3.Flag) != 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Tribunal, "identifier");

              field2.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }

            break;
          case 4:
            if (AsChar(local.Priority1.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Program, "code");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field4.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority2.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "function");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field4.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority3.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Tribunal, "identifier");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field4.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority4.Flag) != 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field3.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }

            break;
          case 5:
            if (AsChar(local.Priority1.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Program, "code");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field4.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority2.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "function");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field4.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority3.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Tribunal, "identifier");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field4.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }
            else if (AsChar(local.Priority4.Flag) != 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field3.Error = true;

              ExitState = "SP0000_PRIORITY_MISMATCH";

              return;
            }

            if (!Equal(export.Export1.Item.OfficeCaseloadAssignment.
              BeginingAlpha, "A") && !
              Equal(export.Export1.Item.OfficeCaseloadAssignment.EndingAlpha,
              "ZZZZZZ"))
            {
              var field1 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "priority");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "beginingAlpha");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.OfficeCaseloadAssignment,
                "endingAlpha");

              field3.Error = true;

              ExitState = "SP0000_PRIORITY_5";

              return;
            }

            break;
          default:
            ExitState = "PRIORITY_MUST_BE_BETWEEN_1_AND_5";

            var field =
              GetField(export.Export1.Item.OfficeCaseloadAssignment, "priority");
              

            field.Error = true;

            return;
        }

        // ****  Validate no assignment overlap ****
        // *** Work request 000265
        // *** 12/21/00 swsrchf
        // *** start
        local.WorkAlphaBeg.Text30 =
          export.Export1.Item.OfficeCaseloadAssignment.BeginingAlpha + "";
        local.WorkAlphaEnd.Text30 =
          export.Export1.Item.OfficeCaseloadAssignment.EndingAlpha + "";

        // *** end
        // *** 12/21/00 swsrchf
        // *** Work request 000265
        // PR158293. Removed line of code from the following read checking for 
        // case load assignment indicator not = to "A" after discussion win Sana
        // Beall. Done 12-9-02 by Lbachura
        // PR171236. Change conditions for read for overlap. Put in a condition 
        // code becuase the table being read contains old information that
        // invalidates the read. Put a chekc in for not = to a if the assignment
        // type is CA.  LBachura 3-28-03
        local.Overlap.Flag = "N";
        local.OfficeCaseloadAssignment.AssignmentType =
          export.TypeSearch.AssignmentType;

        if (Equal(local.OfficeCaseloadAssignment.AssignmentType, "RE"))
        {
          foreach(var item in ReadOfficeCaseloadAssignment3())
          {
            if (Equal(global.Command, "UPDATE") && entities
              .OverlapOfficeCaseloadAssignment.SystemGeneratedIdentifier == export
              .Export1.Item.OfficeCaseloadAssignment.SystemGeneratedIdentifier)
            {
              continue;
            }

            local.Tribunal.Identifier = 0;

            if (ReadTribunal3())
            {
              local.Tribunal.Identifier = entities.OverlapTribunal.Identifier;
            }

            local.Program.Code = "";

            if (ReadProgram3())
            {
              local.Program.Code = entities.OverlapProgram.Code;
            }

            if (Equal(local.Program.Code, export.Export1.Item.Program.Code) && local
              .Tribunal.Identifier == export
              .Export1.Item.Tribunal.Identifier && Equal
              (entities.OverlapOfficeCaseloadAssignment.Function,
              export.Export1.Item.OfficeCaseloadAssignment.Function))
            {
              // *** Work request 000265
              // *** 12/21/00 swsrchf
              // *** start
              local.OverlapAlphaBeg.Text30 =
                entities.OverlapOfficeCaseloadAssignment.BeginingAlpha + "";
              local.OverlapAlphaEnd.Text30 =
                entities.OverlapOfficeCaseloadAssignment.EndingAlpha + "";

              if (!Lt(local.WorkAlphaBeg.Text30, local.OverlapAlphaBeg.Text30) &&
                !
                Lt(local.OverlapAlphaEnd.Text30, local.WorkAlphaBeg.Text30) || !
                Lt(local.WorkAlphaEnd.Text30, local.OverlapAlphaBeg.Text30) && !
                Lt(local.OverlapAlphaEnd.Text30, local.WorkAlphaEnd.Text30))
              {
                local.Overlap.Flag = "Y";

                goto Test2;
              }

              if (!Lt(local.OverlapAlphaBeg.Text30, local.WorkAlphaBeg.Text30) &&
                !Lt(local.WorkAlphaEnd.Text30, local.OverlapAlphaEnd.Text30))
              {
                local.Overlap.Flag = "Y";

                goto Test2;
              }
            }
          }
        }

Test2:

        if (Equal(local.OfficeCaseloadAssignment.AssignmentType, "CA"))
        {
          foreach(var item in ReadOfficeCaseloadAssignment2())
          {
            if (Equal(global.Command, "UPDATE") && entities
              .OverlapOfficeCaseloadAssignment.SystemGeneratedIdentifier == export
              .Export1.Item.OfficeCaseloadAssignment.SystemGeneratedIdentifier)
            {
              continue;
            }

            local.Tribunal.Identifier = 0;

            if (ReadTribunal3())
            {
              local.Tribunal.Identifier = entities.OverlapTribunal.Identifier;
            }

            local.Program.Code = "";

            if (ReadProgram3())
            {
              local.Program.Code = entities.OverlapProgram.Code;
            }

            if (Equal(local.Program.Code, export.Export1.Item.Program.Code) && local
              .Tribunal.Identifier == export
              .Export1.Item.Tribunal.Identifier && Equal
              (entities.OverlapOfficeCaseloadAssignment.Function,
              export.Export1.Item.OfficeCaseloadAssignment.Function))
            {
              // *** Work request 000265
              // *** 12/21/00 swsrchf
              // *** start
              local.OverlapAlphaBeg.Text30 =
                entities.OverlapOfficeCaseloadAssignment.BeginingAlpha + "";
              local.OverlapAlphaEnd.Text30 =
                entities.OverlapOfficeCaseloadAssignment.EndingAlpha + "";

              if (!Lt(local.WorkAlphaBeg.Text30, local.OverlapAlphaBeg.Text30) &&
                !
                Lt(local.OverlapAlphaEnd.Text30, local.WorkAlphaBeg.Text30) || !
                Lt(local.WorkAlphaEnd.Text30, local.OverlapAlphaBeg.Text30) && !
                Lt(local.OverlapAlphaEnd.Text30, local.WorkAlphaEnd.Text30))
              {
                local.Overlap.Flag = "Y";

                goto Test3;
              }

              if (!Lt(local.OverlapAlphaBeg.Text30, local.WorkAlphaBeg.Text30) &&
                !Lt(local.WorkAlphaEnd.Text30, local.OverlapAlphaEnd.Text30))
              {
                local.Overlap.Flag = "Y";

                goto Test3;
              }
            }
          }
        }

Test3:

        if (AsChar(local.Overlap.Flag) == 'Y')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          if (!IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.Function))
          {
            var field1 =
              GetField(export.Export1.Item.OfficeCaseloadAssignment, "function");
              

            field1.Error = true;
          }

          if (!IsEmpty(export.Export1.Item.Program.Code))
          {
            var field1 = GetField(export.Export1.Item.Program, "code");

            field1.Error = true;
          }

          if (export.Export1.Item.Tribunal.Identifier != 0)
          {
            var field1 = GetField(export.Export1.Item.Tribunal, "identifier");

            field1.Error = true;
          }

          if (!IsEmpty(export.Export1.Item.OfficeCaseloadAssignment.
            BeginingAlpha))
          {
            var field1 =
              GetField(export.Export1.Item.OfficeCaseloadAssignment,
              "beginingAlpha");

            field1.Error = true;

            var field2 =
              GetField(export.Export1.Item.OfficeCaseloadAssignment,
              "endingAlpha");

            field2.Error = true;
          }

          ExitState = "SP0000_ASSIGNMENT_OVERLAP";

          return;
        }

        break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

Test4:

    switch(TrimEnd(global.Command))
    {
      case "ADD":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            continue;
          }

          if (AsChar(export.Export1.Item.Common.SelectChar) != 'S')
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test5;
          }

          export.Export1.Update.Common.SelectChar = "";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          export.Export1.Update.OfficeCaseloadAssignment.AssignmentType =
            export.TypeSearch.AssignmentType;

          if (Equal(export.Export1.Item.OfficeCaseloadAssignment.AssignmentType,
            "CA"))
          {
            export.Export1.Update.OfficeCaseloadAssignment.AssignmentIndicator =
              "P";
          }

          if (Equal(export.Export1.Item.OfficeCaseloadAssignment.AssignmentType,
            "RE"))
          {
            export.Export1.Update.OfficeCaseloadAssignment.AssignmentIndicator =
              "A";
          }

          UseSpCreateOfficeCaseloadAssign2();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            global.Command = "DISPLAY";
          }

          break;
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
          foreach(var item in ReadOfficeCaseloadAssignment1())
          {
            // *******************************************
            // READ EACH CA TYPE ASSIGNMENT NOT EQUAL TO ACTIVE AND UPDATE TO 
            // REDISTRIBUTE.
            // *******************************************
            if (AsChar(entities.OfficeCaseloadAssignment.AssignmentIndicator) ==
              'P')
            {
              local.ResetP.AssignmentIndicator = "P";
            }
            else if (AsChar(entities.OfficeCaseloadAssignment.
              AssignmentIndicator) == 'R')
            {
              local.ResetR.AssignmentIndicator = "R";
            }
            else if (AsChar(entities.OfficeCaseloadAssignment.
              AssignmentIndicator) == 'S')
            {
              local.ResetS.AssignmentIndicator = "S";
            }

            if (AsChar(local.ResetP.AssignmentIndicator) == 'P' && AsChar
              (local.ResetR.AssignmentIndicator) == 'R')
            {
              local.Reset.Flag = "Y";

              break;
            }
            else if (AsChar(local.ResetP.AssignmentIndicator) == 'P' && AsChar
              (local.ResetS.AssignmentIndicator) == 'S')
            {
              local.Reset.Flag = "Y";

              break;
            }
            else if (AsChar(local.ResetR.AssignmentIndicator) == 'R' && AsChar
              (local.ResetS.AssignmentIndicator) == 'S')
            {
              local.Reset.Flag = "Y";

              break;
            }
          }

          if (AsChar(local.Reset.Flag) == 'Y')
          {
            foreach(var item in ReadOfficeCaseloadAssignment1())
            {
              // *******************************************
              // READ EACH CA TYPE ASSIGNMENT NOT EQUAL TO ACTIVE AND UPDATE TO 
              // REDISTRIBUTE.
              // *******************************************
              try
              {
                UpdateOfficeCaseloadAssignment1();

                // *** Problem report I00112351
                // *** 01/29/01 swsrchf
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_NU";

                    continue;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_PV";

                    continue;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }

        global.Command = "DISPLAY";

        break;
      case "DELETE":
        if (Equal(global.Command, "DELETE"))
        {
          // to validate action level security
          UseScCabTestSecurity();
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            continue;
          }

          if (AsChar(export.Export1.Item.Common.SelectChar) != 'S')
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test5;
          }

          if (export.Export1.Item.OfficeCaseloadAssignment.
            SystemGeneratedIdentifier != export
            .Export1.Item.HiddenOfficeCaseloadAssignment.
              SystemGeneratedIdentifier)
          {
            ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

            goto Test5;
          }

          if (export.Export1.Item.HiddenServiceProvider.SystemGeneratedId == 0)
          {
            ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

            goto Test5;
          }

          export.Export1.Update.Common.SelectChar = "";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          UseSpDeleteOfficeCaseloadAssign();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
          {
            global.Command = "DISPLAY";
          }

          break;
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
          foreach(var item in ReadOfficeCaseloadAssignment1())
          {
            // *******************************************
            // READ EACH CA TYPE ASSIGNMENT NOT EQUAL TO ACTIVE AND UPDATE TO 
            // REDISTRIBUTE.
            // *******************************************
            try
            {
              UpdateOfficeCaseloadAssignment1();

              // *** Problem report I00112351
              // *** 01/29/01 swsrchf
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_NU";

                  continue;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_PV";

                  continue;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        break;
      case "LIST":
        if (AsChar(import.ListAsgnType.Flag) == 'S')
        {
          export.Code.CodeName = "ASSIGNMENT TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }
        else if (!IsEmpty(import.ListAsgnType.Flag))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListAsgnType, "flag");

          field.Error = true;

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            continue;
          }

          if (AsChar(export.Export1.Item.Common.SelectChar) != 'S')
          {
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
          }

          if (!IsEmpty(export.Export1.Item.ListServiceProvider.Flag))
          {
            if (AsChar(export.Export1.Item.ListServiceProvider.Flag) != 'S')
            {
              var field =
                GetField(export.Export1.Item.ListServiceProvider, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
            }

            export.HiddenSelectionServiceProvider.Assign(
              export.Export1.Item.ServiceProvider);
            ExitState = "ECO_LNK_TO_OFFICE_SERVICE_PROVDR";

            return;
          }

          if (!IsEmpty(export.Export1.Item.ListTribunal.Flag))
          {
            if (AsChar(export.Export1.Item.ListTribunal.Flag) != 'S')
            {
              var field = GetField(export.Export1.Item.ListTribunal, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
            }

            if (Equal(export.Export1.Item.OfficeCaseloadAssignment.
              AssignmentType, "RE"))
            {
              var field = GetField(export.Export1.Item.ListTribunal, "flag");

              field.Error = true;

              ExitState = "INVALID_OPTION_SELECTED";

              return;
            }

            // -- Default LTRB to Kansas tribunals.
            export.ToLtrb.StateAbbreviation = "KS";
            ExitState = "ECO_LNK_TO_LTRB";

            return;
          }

          if (!IsEmpty(export.Export1.Item.ListProgram.Flag))
          {
            if (AsChar(export.Export1.Item.ListProgram.Flag) != 'S')
            {
              var field = GetField(export.Export1.Item.ListProgram, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
            }

            if (Equal(export.Export1.Item.OfficeCaseloadAssignment.
              AssignmentType, "RE"))
            {
              var field = GetField(export.Export1.Item.ListProgram, "flag");

              field.Error = true;

              ExitState = "INVALID_OPTION_SELECTED";

              return;
            }

            MoveProgram(export.Export1.Item.Program,
              export.HiddenSelectionProgram);
            export.FromOfca.Flag = "Y";
            ExitState = "ECO_LNK_TO_PROGRAM_MAINTENANCE";

            return;
          }

          if (!IsEmpty(export.Export1.Item.ListFunction.Flag))
          {
            if (AsChar(export.Export1.Item.ListFunction.Flag) != 'S')
            {
              var field = GetField(export.Export1.Item.ListFunction, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
            }

            if (Equal(export.Export1.Item.OfficeCaseloadAssignment.
              AssignmentType, "RE"))
            {
              var field = GetField(export.Export1.Item.ListFunction, "flag");

              field.Error = true;

              ExitState = "INVALID_OPTION_SELECTED";

              return;
            }

            export.HiddenSelectionOfficeCaseloadAssignment.Function =
              export.Export1.Item.OfficeCaseloadAssignment.Function ?? "";
            export.Code.CodeName = "FUNCTION";
            export.CodeValue.Cdvalue =
              export.Export1.Item.OfficeCaseloadAssignment.Function ?? Spaces
              (10);
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }

          break;
        }

        break;
      case "UPDATE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            continue;
          }

          if (AsChar(export.Export1.Item.Common.SelectChar) != 'S')
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test5;
          }

          if (export.Export1.Item.OfficeCaseloadAssignment.
            SystemGeneratedIdentifier != export
            .Export1.Item.HiddenOfficeCaseloadAssignment.
              SystemGeneratedIdentifier)
          {
            ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

            goto Test5;
          }

          if (!Equal(export.Export1.Item.OfficeCaseloadAssignment.
            AssignmentType,
            export.Export1.Item.HiddenOfficeCaseloadAssignment.AssignmentType))
          {
            ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";

            var field1 =
              GetField(export.Export1.Item.OfficeCaseloadAssignment,
              "assignmentType");

            field1.Error = true;

            return;
          }

          // *** Work request 000266
          // *** 12/21/00 swsrchf
          // *** start
          if (Equal(export.Export1.Item.OfficeCaseloadAssignment.AssignmentType,
            "RE"))
          {
            export.Export1.Update.OfficeCaseloadAssignment.AssignmentIndicator =
              "A";
          }
          else
          {
            export.Export1.Update.OfficeCaseloadAssignment.AssignmentIndicator =
              "P";
          }

          // *** end
          // *** 12/21/00 swsrchf
          // *** Work request 000266
          export.Export1.Update.Common.SelectChar = "";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          UseSpUpdateOfficeCaseloadAssign();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
            export.Export1.Update.HiddenTribunal.Identifier =
              export.Export1.Item.Tribunal.Identifier;
            MoveProgram(export.Export1.Item.Program,
              export.Export1.Update.HiddenProgram);
            export.Export1.Update.HiddenServiceProvider.Assign(
              export.Export1.Item.ServiceProvider);
            export.Export1.Update.HiddenOfficeCaseloadAssignment.Assign(
              export.Export1.Item.OfficeCaseloadAssignment);
            global.Command = "DISPLAY";
          }

          break;
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
          foreach(var item in ReadOfficeCaseloadAssignment1())
          {
            // *******************************************
            // READ EACH CA TYPE ASSIGNMENT NOT EQUAL TO ACTIVE AND UPDATE TO 
            // REDISTRIBUTE.
            // *******************************************
            if (AsChar(entities.OfficeCaseloadAssignment.AssignmentIndicator) ==
              'P')
            {
              local.ResetP.AssignmentIndicator = "P";
            }
            else if (AsChar(entities.OfficeCaseloadAssignment.
              AssignmentIndicator) == 'R')
            {
              local.ResetR.AssignmentIndicator = "R";
            }
            else if (AsChar(entities.OfficeCaseloadAssignment.
              AssignmentIndicator) == 'S')
            {
              local.ResetS.AssignmentIndicator = "S";
            }

            if (AsChar(local.ResetP.AssignmentIndicator) == 'P' && AsChar
              (local.ResetR.AssignmentIndicator) == 'R')
            {
              local.Reset.Flag = "Y";

              break;
            }
            else if (AsChar(local.ResetP.AssignmentIndicator) == 'P' && AsChar
              (local.ResetS.AssignmentIndicator) == 'S')
            {
              local.Reset.Flag = "Y";

              break;
            }
            else if (AsChar(local.ResetR.AssignmentIndicator) == 'R' && AsChar
              (local.ResetS.AssignmentIndicator) == 'S')
            {
              local.Reset.Flag = "Y";

              break;
            }
          }

          if (AsChar(local.Reset.Flag) == 'Y')
          {
            foreach(var item in ReadOfficeCaseloadAssignment1())
            {
              // *******************************************
              // READ EACH CA TYPE ASSIGNMENT NOT EQUAL TO ACTIVE AND UPDATE TO 
              // REDISTRIBUTE.
              // *******************************************
              try
              {
                UpdateOfficeCaseloadAssignment1();

                // *** Problem report I00112351
                // *** 01/29/01 swsrchf
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_NU";

                    continue;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_PV";

                    continue;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }

        break;
      default:
        break;
    }

Test5:

    if (Equal(global.Command, "DISPLAY"))
    {
      export.ListOffice.Flag = "";
      export.ListAsgnType.Flag = "";

      if (export.Office.SystemGeneratedId == 0)
      {
        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        return;
      }

      if (export.Office.SystemGeneratedId != export
        .HiddenOffice.SystemGeneratedId && export
        .HiddenOffice.SystemGeneratedId != 0)
      {
        export.ServiceProvider.LastName = "";
        export.ServiceProvider.SystemGeneratedId = 0;
        export.TypeSearch.AssignmentType = "";
      }

      if (Equal(export.TypeSearch.AssignmentType, "CA") || Equal
        (export.TypeSearch.AssignmentType, "RE"))
      {
      }
      else if (IsEmpty(export.TypeSearch.AssignmentType))
      {
        export.TypeSearch.AssignmentType = "CA";
      }
      else
      {
        var field = GetField(export.TypeSearch, "assignmentType");

        field.Error = true;

        ExitState = "INVALID_TYPE_CODE";

        return;
      }

      if (ReadOffice())
      {
        // Currency on Office acquired.
        export.Office.Assign(entities.Office);
        export.HiddenOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;

        if (Lt(entities.Office.DiscontinueDate, local.Current.Date))
        {
          export.InactiveOffice.Flag = "Y";
        }
        else
        {
          export.InactiveOffice.Flag = "N";
        }

        if (ReadCodeValue())
        {
          export.OfficeType.Description = entities.CodeValue.Description;
        }
        else
        {
          ExitState = "CODE_VALUE_NF";
        }
      }
      else
      {
        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        ExitState = "OFFICE_NF";

        return;
      }

      if (ReadOfficeAddress())
      {
        export.OfficeAddress.City = entities.OfficeAddress.City;
      }
      else
      {
        var field = GetField(export.OfficeAddress, "city");

        field.Error = true;

        ExitState = "OFFICE_ADDRESS_NF";
      }

      // **** Blank out group view ****
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        export.Export1.Update.OfficeCaseloadAssignment.Assign(
          local.InitOfficeCaseloadAssignment);
        export.Export1.Update.HiddenOfficeCaseloadAssignment.Assign(
          local.InitOfficeCaseloadAssignment);
        MoveProgram(local.InitProgram, export.Export1.Update.Program);
        MoveProgram(local.InitProgram, export.Export1.Update.HiddenProgram);
        export.Export1.Update.Tribunal.Identifier =
          local.InitTribunal.Identifier;
        export.Export1.Update.HiddenTribunal.Identifier =
          local.InitTribunal.Identifier;
      }

      export.OfficeCaseloadAssignment.AssignmentIndicator = "";
      local.SetAssign.Flag = "N";
      local.StartSpNbr.SystemGeneratedId = 0;
      local.EndSpNbr.SystemGeneratedId = 99999;

      if (export.ServiceProvider.SystemGeneratedId != 0)
      {
        local.StartSpNbr.SystemGeneratedId =
          export.ServiceProvider.SystemGeneratedId;
        local.EndSpNbr.SystemGeneratedId =
          export.ServiceProvider.SystemGeneratedId;
      }

      // @@@
      // -- 11/04/13 GVandy CQ41845 Change display order to priority, program, 
      // tribunal,
      //    function, and alpha.  Original code commented out below.
      local.Local1.Index = -1;

      foreach(var item in ReadOfficeCaseloadAssignmentOfficeServiceProvider2())
      {
        // *********************************************
        // READ EACH ASSIGNMENT WHERE ASSIGNMENT INDICATOR IS NOT = A.
        // *********************************************
        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
          if (AsChar(entities.OfficeCaseloadAssignment.AssignmentIndicator) == 'A'
            )
          {
            // *** Problem report I00112351
            // *** 01/29/01 swsrchf
            // *** changed the Ending Alpha check
            if (entities.OfficeCaseloadAssignment.Priority == 5 && Equal
              (entities.OfficeCaseloadAssignment.BeginingAlpha, "A") && !
              Lt(entities.OfficeCaseloadAssignment.EndingAlpha, "Z"))
            {
              local.SetAssign.Flag = "Y";
            }

            continue;
          }
        }

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
        MoveOfficeServiceProvider(entities.OfficeServiceProvider,
          local.Local1.Update.OfficeServiceProvider);

        if (Equal(export.TypeSearch.AssignmentType, "CA") || Equal
          (export.TypeSearch.AssignmentType, "RE"))
        {
          if (ReadProgram2())
          {
            MoveProgram(entities.Program, local.Local1.Update.Program);
          }
        }

        if (Equal(export.TypeSearch.AssignmentType, "CA"))
        {
          if (ReadTribunal2())
          {
            local.Local1.Update.Tribunal.Identifier =
              entities.Tribunal.Identifier;
          }
        }

        // *** Work request 000265
        // *** 12/21/00 swsrchf
        // *** start
        if (IsEmpty(export.OfficeCaseloadAssignment.AssignmentIndicator))
        {
          export.OfficeCaseloadAssignment.AssignmentIndicator =
            entities.OfficeCaseloadAssignment.AssignmentIndicator;
        }

        // *** end
        // *** 12/21/00 swsrchf
        // *** Work request 000265
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
        MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
          local.Compare.CompareOfficeServiceProvider);
        MoveProgram(local.Local1.Item.Program, local.Compare.CompareProgram);
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

              goto Test6;
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

              goto Test6;
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

              goto Test6;
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

              goto Test6;
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

              goto Test6;
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

Test6:

          if (AsChar(local.Swap1.Flag) == 'N')
          {
            continue;
          }

          local.Swap.SwapOfficeCaseloadAssignment.Assign(
            local.Local1.Item.OfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
            local.Swap.SwapOfficeServiceProvider);
          MoveProgram(local.Local1.Item.Program, local.Swap.SwapProgram);
          local.Swap.SwapServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          local.Swap.SwapTribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
          local.Local1.Update.OfficeCaseloadAssignment.Assign(
            local.Compare.CompareOfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Compare.CompareOfficeServiceProvider,
            local.Local1.Update.OfficeServiceProvider);
          MoveProgram(local.Compare.CompareProgram, local.Local1.Update.Program);
            
          local.Local1.Update.ServiceProvider.Assign(
            local.Compare.CompareServiceProvider);
          local.Local1.Update.Tribunal.Identifier =
            local.Compare.CompareTribunal.Identifier;

          local.Local1.Index = local.I.Count - 1;
          local.Local1.CheckSize();

          local.Local1.Update.OfficeCaseloadAssignment.Assign(
            local.Swap.SwapOfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Swap.SwapOfficeServiceProvider,
            local.Local1.Update.OfficeServiceProvider);
          MoveProgram(local.Swap.SwapProgram, local.Local1.Update.Program);
          local.Local1.Update.ServiceProvider.Assign(
            local.Swap.SwapServiceProvider);
          local.Local1.Update.Tribunal.Identifier =
            local.Swap.SwapTribunal.Identifier;
          local.Compare.CompareOfficeCaseloadAssignment.Assign(
            local.Local1.Item.OfficeCaseloadAssignment);
          MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
            local.Compare.CompareOfficeServiceProvider);
          MoveProgram(local.Local1.Item.Program, local.Compare.CompareProgram);
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
          MoveOfficeServiceProvider(local.Local1.Item.OfficeServiceProvider,
            export.Export1.Update.OfficeServiceProvider);
          MoveProgram(local.Local1.Item.Program, export.Export1.Update.Program);
          export.Export1.Update.ServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          export.Export1.Update.Tribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
          MoveOfficeCaseloadAssignment2(local.Local1.Item.
            OfficeCaseloadAssignment,
            export.Export1.Update.HiddenOfficeCaseloadAssignment);
          MoveProgram(local.Local1.Item.Program,
            export.Export1.Update.HiddenProgram);
          export.Export1.Update.HiddenServiceProvider.Assign(
            local.Local1.Item.ServiceProvider);
          export.Export1.Update.HiddenTribunal.Identifier =
            local.Local1.Item.Tribunal.Identifier;
          export.Export1.Update.Common.SelectChar = "";
          export.Export1.Update.ListTribunal.Flag = "";
          export.Export1.Update.ListServiceProvider.Flag = "";
          export.Export1.Update.ListProgram.Flag = "";
          export.Export1.Update.ListFunction.Flag = "";
          export.Export1.Next();
        }
        while(local.Local1.Index + 1 != local.Local1.Count);
      }

      if (AsChar(local.SetAssign.Flag) != 'Y')
      {
        // *** Problem report I00112351
        // *** 01/29/01 swsrchf
        // *** start
        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPD_A_Z";
        }
        else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD_A_Z";
        }
        else if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DEL_A_Z";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DIS_A_Z";
        }

        // *** end
        // *** 01/29/01 swsrchf
        // *** Problem report I00112351
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_OK";
      }

      export.HiddenOffice.SystemGeneratedId = export.Office.SystemGeneratedId;
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
    target.BeginingAlpha = source.BeginingAlpha;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
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
    target.BeginingAlpha = source.BeginingAlpha;
    target.Priority = source.Priority;
    target.Function = source.Function;
    target.AssignmentType = source.AssignmentType;
  }

  private static void MoveOfficeCaseloadAssignment3(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EndingAlpha = source.EndingAlpha;
    target.BeginingAlpha = source.BeginingAlpha;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Priority = source.Priority;
  }

  private static void MoveOfficeCaseloadAssignment4(
    OfficeCaseloadAssignment source, OfficeCaseloadAssignment target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = export.CodeValue.Cdvalue;
    useImport.Code.CodeName = export.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
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

  private void UseSpCreateOfficeCaseloadAssign1()
  {
    var useImport = new SpCreateOfficeCaseloadAssign.Import();
    var useExport = new SpCreateOfficeCaseloadAssign.Export();

    useImport.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
    MoveProgram(local.Program, useImport.Program);
    useImport.OfficeCaseloadAssignment.Assign(local.OfficeCaseloadAssignment);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.Tribunal.Identifier = local.Tribunal.Identifier;

    Call(SpCreateOfficeCaseloadAssign.Execute, useImport, useExport);
  }

  private void UseSpCreateOfficeCaseloadAssign2()
  {
    var useImport = new SpCreateOfficeCaseloadAssign.Import();
    var useExport = new SpCreateOfficeCaseloadAssign.Export();

    MoveOfficeServiceProvider(export.Export1.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    MoveProgram(export.Export1.Item.Program, useImport.Program);
    useImport.OfficeCaseloadAssignment.Assign(
      export.Export1.Item.OfficeCaseloadAssignment);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.Tribunal.Identifier = export.Export1.Item.Tribunal.Identifier;

    Call(SpCreateOfficeCaseloadAssign.Execute, useImport, useExport);

    export.Export1.Update.OfficeCaseloadAssignment.Assign(
      useExport.OfficeCaseloadAssignment);
  }

  private void UseSpDeleteOfficeCaseloadAssign()
  {
    var useImport = new SpDeleteOfficeCaseloadAssign.Import();
    var useExport = new SpDeleteOfficeCaseloadAssign.Export();

    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveOfficeCaseloadAssignment4(export.Export1.Item.OfficeCaseloadAssignment,
      useImport.OfficeCaseloadAssignment);

    Call(SpDeleteOfficeCaseloadAssign.Execute, useImport, useExport);

    MoveOfficeCaseloadAssignment3(useExport.OfficeCaseloadAssignment,
      export.Export1.Update.OfficeCaseloadAssignment);
  }

  private void UseSpUpdateOfficeCaseloadAssign()
  {
    var useImport = new SpUpdateOfficeCaseloadAssign.Import();
    var useExport = new SpUpdateOfficeCaseloadAssign.Export();

    MoveOfficeServiceProvider(export.Export1.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.OldServiceProvider.SystemGeneratedId =
      export.Export1.Item.HiddenServiceProvider.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Export1.Item.ServiceProvider.SystemGeneratedId;
    MoveProgram(export.Export1.Item.HiddenProgram, useImport.OldProgram);
    MoveProgram(export.Export1.Item.Program, useImport.Program);
    useImport.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.OfficeCaseloadAssignment.Assign(
      export.Export1.Item.OfficeCaseloadAssignment);
    useImport.OldTribunal.Identifier =
      export.Export1.Item.HiddenTribunal.Identifier;
    useImport.Tribunal.Identifier = export.Export1.Item.Tribunal.Identifier;

    Call(SpUpdateOfficeCaseloadAssign.Execute, useImport, useExport);

    export.Export1.Update.OfficeCaseloadAssignment.Assign(
      useExport.OfficeCaseloadAssignment);
  }

  private void DeleteOfficeCaseloadAssignment()
  {
    Update("DeleteOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });
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

  private bool ReadCountyService1()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.CountyService.Populated = false;

    return Read("ReadCountyService1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 1);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 2);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 3);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 4);
        entities.CountyService.CogCode = db.GetNullableString(reader, 5);
        entities.CountyService.Function = db.GetNullableString(reader, 6);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 7);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadCountyService2()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "function",
          export.Export1.Item.OfficeCaseloadAssignment.Function ?? "");
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 1);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 2);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 3);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 4);
        entities.CountyService.CogCode = db.GetNullableString(reader, 5);
        entities.CountyService.Function = db.GetNullableString(reader, 6);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 7);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadCountyService3()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "identifier", export.Export1.Item.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 1);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 2);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 3);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 4);
        entities.CountyService.CogCode = db.GetNullableString(reader, 5);
        entities.CountyService.Function = db.GetNullableString(reader, 6);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 7);
        entities.CountyService.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCountyService4()
  {
    entities.CountyService.Populated = false;

    return ReadEach("ReadCountyService4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 1);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 2);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 3);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 4);
        entities.CountyService.CogCode = db.GetNullableString(reader, 5);
        entities.CountyService.Function = db.GetNullableString(reader, 6);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 7);
        entities.CountyService.Populated = true;

        return true;
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

  private bool ReadOfficeAssignmentPlan()
  {
    entities.OfficeAssignmentPlan.Populated = false;

    return Read("ReadOfficeAssignmentPlan",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.
          SetString(command, "assignmentType", export.TypeSearch.AssignmentType);
          
      },
      (db, reader) =>
      {
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
        entities.OfficeAssignmentPlan.TribunalInd = db.GetString(reader, 8);
        entities.OfficeAssignmentPlan.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment1()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
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
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment2()
  {
    entities.OverlapOfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offDGeneratedId", export.Office.SystemGeneratedId);
        db.
          SetString(command, "assignmentType", export.TypeSearch.AssignmentType);
          
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "priority",
          export.Export1.Item.OfficeCaseloadAssignment.Priority);
      },
      (db, reader) =>
      {
        entities.OverlapOfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OverlapOfficeCaseloadAssignment.EndingAlpha =
          db.GetString(reader, 1);
        entities.OverlapOfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OverlapOfficeCaseloadAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OverlapOfficeCaseloadAssignment.Priority =
          db.GetInt32(reader, 4);
        entities.OverlapOfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OverlapOfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 6);
        entities.OverlapOfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 7);
        entities.OverlapOfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 8);
        entities.OverlapOfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 9);
        entities.OverlapOfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 10);
        entities.OverlapOfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.OverlapOfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.OverlapOfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 13);
        entities.OverlapOfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 14);
        entities.OverlapOfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignment3()
  {
    entities.OverlapOfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignment3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offDGeneratedId", export.Office.SystemGeneratedId);
        db.
          SetString(command, "assignmentType", export.TypeSearch.AssignmentType);
          
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "priority",
          export.Export1.Item.OfficeCaseloadAssignment.Priority);
      },
      (db, reader) =>
      {
        entities.OverlapOfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OverlapOfficeCaseloadAssignment.EndingAlpha =
          db.GetString(reader, 1);
        entities.OverlapOfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OverlapOfficeCaseloadAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.OverlapOfficeCaseloadAssignment.Priority =
          db.GetInt32(reader, 4);
        entities.OverlapOfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OverlapOfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 6);
        entities.OverlapOfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 7);
        entities.OverlapOfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 8);
        entities.OverlapOfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 9);
        entities.OverlapOfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 10);
        entities.OverlapOfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.OverlapOfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.OverlapOfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 13);
        entities.OverlapOfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 14);
        entities.OverlapOfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignmentOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignmentOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
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
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 17);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 18);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 18);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 20);
        entities.ServiceProvider.UserId = db.GetString(reader, 21);
        entities.ServiceProvider.LastName = db.GetString(reader, 22);
        entities.ServiceProvider.FirstName = db.GetString(reader, 23);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeCaseloadAssignmentOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeCaseloadAssignment.Populated = false;

    return ReadEach("ReadOfficeCaseloadAssignmentOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.
          SetString(command, "assignmentType", export.TypeSearch.AssignmentType);
          
        db.SetInt32(command, "priority", export.Priority.Priority);
        db.SetString(command, "lastName", import.ServiceProvider.LastName);
        db.SetInt32(
          command, "systemGeneratedId1", local.StartSpNbr.SystemGeneratedId);
        db.SetInt32(
          command, "systemGeneratedId2", local.EndSpNbr.SystemGeneratedId);
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
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 17);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 18);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 18);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 19);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 20);
        entities.ServiceProvider.UserId = db.GetString(reader, 21);
        entities.ServiceProvider.LastName = db.GetString(reader, 22);
        entities.ServiceProvider.FirstName = db.GetString(reader, 23);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeCaseloadAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          export.Export1.Item.OfficeServiceProvider.RoleCode);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          export.Export1.Item.OfficeServiceProvider.RoleCode);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(command, "code", export.Export1.Item.Program.Code);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.EffectiveDate = db.GetDate(reader, 2);
        entities.Program.DiscontinueDate = db.GetDate(reader, 3);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Program.Populated = false;

    return Read("ReadProgram2",
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
        entities.Program.EffectiveDate = db.GetDate(reader, 2);
        entities.Program.DiscontinueDate = db.GetDate(reader, 3);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram3()
  {
    System.Diagnostics.Debug.Assert(
      entities.OverlapOfficeCaseloadAssignment.Populated);
    entities.OverlapProgram.Populated = false;

    return Read("ReadProgram3",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId",
          entities.OverlapOfficeCaseloadAssignment.PrgGeneratedId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OverlapProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OverlapProgram.Code = db.GetString(reader, 1);
        entities.OverlapProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram4()
  {
    entities.Program.Populated = false;

    return ReadEach("ReadProgram4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.EffectiveDate = db.GetDate(reader, 2);
        entities.Program.DiscontinueDate = db.GetDate(reader, 3);
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.Export1.Item.ServiceProvider.SystemGeneratedId);
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

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetString(
          command, "roleCode",
          import.HiddenSelectionOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", export.Export1.Item.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.OfficeCaseloadAssignment.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal3()
  {
    System.Diagnostics.Debug.Assert(
      entities.OverlapOfficeCaseloadAssignment.Populated);
    entities.OverlapTribunal.Populated = false;

    return Read("ReadTribunal3",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.OverlapOfficeCaseloadAssignment.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OverlapTribunal.Identifier = db.GetInt32(reader, 0);
        entities.OverlapTribunal.Populated = true;
      });
  }

  private void UpdateOfficeCaseloadAssignment1()
  {
    var assignmentIndicator = "P";

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("UpdateOfficeCaseloadAssignment1",
      (db, command) =>
      {
        db.SetString(command, "assignmentInd", assignmentIndicator);
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.AssignmentIndicator = assignmentIndicator;
    entities.OfficeCaseloadAssignment.Populated = true;
  }

  private void UpdateOfficeCaseloadAssignment2()
  {
    var assignmentIndicator = "R";

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("UpdateOfficeCaseloadAssignment2",
      (db, command) =>
      {
        db.SetString(command, "assignmentInd", assignmentIndicator);
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.AssignmentIndicator = assignmentIndicator;
    entities.OfficeCaseloadAssignment.Populated = true;
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
      /// A value of HiddenOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("hiddenOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment HiddenOfficeCaseloadAssignment
      {
        get => hiddenOfficeCaseloadAssignment ??= new();
        set => hiddenOfficeCaseloadAssignment = value;
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
      /// A value of ListTribunal.
      /// </summary>
      [JsonPropertyName("listTribunal")]
      public Common ListTribunal
      {
        get => listTribunal ??= new();
        set => listTribunal = value;
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
      /// A value of HiddenProgram.
      /// </summary>
      [JsonPropertyName("hiddenProgram")]
      public Program HiddenProgram
      {
        get => hiddenProgram ??= new();
        set => hiddenProgram = value;
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
      /// A value of ListFunction.
      /// </summary>
      [JsonPropertyName("listFunction")]
      public Common ListFunction
      {
        get => listFunction ??= new();
        set => listFunction = value;
      }

      /// <summary>
      /// A value of ListProgram.
      /// </summary>
      [JsonPropertyName("listProgram")]
      public Common ListProgram
      {
        get => listProgram ??= new();
        set => listProgram = value;
      }

      /// <summary>
      /// A value of ListServiceProvider.
      /// </summary>
      [JsonPropertyName("listServiceProvider")]
      public Common ListServiceProvider
      {
        get => listServiceProvider ??= new();
        set => listServiceProvider = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      public const int Capacity = 44;

      private OfficeCaseloadAssignment hiddenOfficeCaseloadAssignment;
      private OfficeServiceProvider officeServiceProvider;
      private Common listTribunal;
      private ServiceProvider hiddenServiceProvider;
      private Program hiddenProgram;
      private Tribunal hiddenTribunal;
      private Common listFunction;
      private Common listProgram;
      private Common listServiceProvider;
      private Program program;
      private Tribunal tribunal;
      private ServiceProvider serviceProvider;
      private Common common;
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
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of Confirm.
    /// </summary>
    [JsonPropertyName("confirm")]
    public Common Confirm
    {
      get => confirm ??= new();
      set => confirm = value;
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
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of ListAsgnType.
    /// </summary>
    [JsonPropertyName("listAsgnType")]
    public Common ListAsgnType
    {
      get => listAsgnType ??= new();
      set => listAsgnType = value;
    }

    /// <summary>
    /// A value of TypeSearch.
    /// </summary>
    [JsonPropertyName("typeSearch")]
    public OfficeCaseloadAssignment TypeSearch
    {
      get => typeSearch ??= new();
      set => typeSearch = value;
    }

    /// <summary>
    /// A value of HiddenSelectionCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenSelectionCodeValue")]
    public CodeValue HiddenSelectionCodeValue
    {
      get => hiddenSelectionCodeValue ??= new();
      set => hiddenSelectionCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenSelectionOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("hiddenSelectionOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment HiddenSelectionOfficeCaseloadAssignment
    {
      get => hiddenSelectionOfficeCaseloadAssignment ??= new();
      set => hiddenSelectionOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of HiddenSelectionOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionOfficeServiceProvider")]
    public OfficeServiceProvider HiddenSelectionOfficeServiceProvider
    {
      get => hiddenSelectionOfficeServiceProvider ??= new();
      set => hiddenSelectionOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenSelectionProgram.
    /// </summary>
    [JsonPropertyName("hiddenSelectionProgram")]
    public Program HiddenSelectionProgram
    {
      get => hiddenSelectionProgram ??= new();
      set => hiddenSelectionProgram = value;
    }

    /// <summary>
    /// A value of FromLtrb.
    /// </summary>
    [JsonPropertyName("fromLtrb")]
    public Tribunal FromLtrb
    {
      get => fromLtrb ??= new();
      set => fromLtrb = value;
    }

    /// <summary>
    /// A value of HiddenSelectionServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionServiceProvider")]
    public ServiceProvider HiddenSelectionServiceProvider
    {
      get => hiddenSelectionServiceProvider ??= new();
      set => hiddenSelectionServiceProvider = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public OfficeCaseloadAssignment Search
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

    /// <summary>
    /// A value of InactiveOffice.
    /// </summary>
    [JsonPropertyName("inactiveOffice")]
    public Common InactiveOffice
    {
      get => inactiveOffice ??= new();
      set => inactiveOffice = value;
    }

    /// <summary>
    /// A value of SelectedOfficeAddress.
    /// </summary>
    [JsonPropertyName("selectedOfficeAddress")]
    public OfficeAddress SelectedOfficeAddress
    {
      get => selectedOfficeAddress ??= new();
      set => selectedOfficeAddress = value;
    }

    private OfficeCaseloadAssignment priority;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private Common confirm;
    private Office selectedOffice;
    private Office hiddenOffice;
    private Common listAsgnType;
    private OfficeCaseloadAssignment typeSearch;
    private CodeValue hiddenSelectionCodeValue;
    private OfficeCaseloadAssignment hiddenSelectionOfficeCaseloadAssignment;
    private OfficeServiceProvider hiddenSelectionOfficeServiceProvider;
    private Program hiddenSelectionProgram;
    private Tribunal fromLtrb;
    private ServiceProvider hiddenSelectionServiceProvider;
    private CodeValue officeType;
    private Common listOffice;
    private ServiceProvider serviceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private OfficeCaseloadAssignment search;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common inactiveOffice;
    private OfficeAddress selectedOfficeAddress;
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
      /// A value of HiddenOfficeCaseloadAssignment.
      /// </summary>
      [JsonPropertyName("hiddenOfficeCaseloadAssignment")]
      public OfficeCaseloadAssignment HiddenOfficeCaseloadAssignment
      {
        get => hiddenOfficeCaseloadAssignment ??= new();
        set => hiddenOfficeCaseloadAssignment = value;
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
      /// A value of ListTribunal.
      /// </summary>
      [JsonPropertyName("listTribunal")]
      public Common ListTribunal
      {
        get => listTribunal ??= new();
        set => listTribunal = value;
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
      /// A value of HiddenProgram.
      /// </summary>
      [JsonPropertyName("hiddenProgram")]
      public Program HiddenProgram
      {
        get => hiddenProgram ??= new();
        set => hiddenProgram = value;
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
      /// A value of ListFunction.
      /// </summary>
      [JsonPropertyName("listFunction")]
      public Common ListFunction
      {
        get => listFunction ??= new();
        set => listFunction = value;
      }

      /// <summary>
      /// A value of ListProgram.
      /// </summary>
      [JsonPropertyName("listProgram")]
      public Common ListProgram
      {
        get => listProgram ??= new();
        set => listProgram = value;
      }

      /// <summary>
      /// A value of ListServiceProvider.
      /// </summary>
      [JsonPropertyName("listServiceProvider")]
      public Common ListServiceProvider
      {
        get => listServiceProvider ??= new();
        set => listServiceProvider = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      public const int Capacity = 44;

      private OfficeCaseloadAssignment hiddenOfficeCaseloadAssignment;
      private OfficeServiceProvider officeServiceProvider;
      private Common listTribunal;
      private ServiceProvider hiddenServiceProvider;
      private Program hiddenProgram;
      private Tribunal hiddenTribunal;
      private Common listFunction;
      private Common listProgram;
      private Common listServiceProvider;
      private Program program;
      private Tribunal tribunal;
      private ServiceProvider serviceProvider;
      private Common common;
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
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of Confirm.
    /// </summary>
    [JsonPropertyName("confirm")]
    public Common Confirm
    {
      get => confirm ??= new();
      set => confirm = value;
    }

    /// <summary>
    /// A value of FromOfca.
    /// </summary>
    [JsonPropertyName("fromOfca")]
    public Common FromOfca
    {
      get => fromOfca ??= new();
      set => fromOfca = value;
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
    /// A value of ListAsgnType.
    /// </summary>
    [JsonPropertyName("listAsgnType")]
    public Common ListAsgnType
    {
      get => listAsgnType ??= new();
      set => listAsgnType = value;
    }

    /// <summary>
    /// A value of TypeSearch.
    /// </summary>
    [JsonPropertyName("typeSearch")]
    public OfficeCaseloadAssignment TypeSearch
    {
      get => typeSearch ??= new();
      set => typeSearch = value;
    }

    /// <summary>
    /// A value of HiddenSelectionCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenSelectionCodeValue")]
    public CodeValue HiddenSelectionCodeValue
    {
      get => hiddenSelectionCodeValue ??= new();
      set => hiddenSelectionCodeValue = value;
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
    /// A value of HiddenSelectionProgram.
    /// </summary>
    [JsonPropertyName("hiddenSelectionProgram")]
    public Program HiddenSelectionProgram
    {
      get => hiddenSelectionProgram ??= new();
      set => hiddenSelectionProgram = value;
    }

    /// <summary>
    /// A value of HiddenSelectionServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenSelectionServiceProvider")]
    public ServiceProvider HiddenSelectionServiceProvider
    {
      get => hiddenSelectionServiceProvider ??= new();
      set => hiddenSelectionServiceProvider = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public OfficeCaseloadAssignment Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenSelectionOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("hiddenSelectionOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment HiddenSelectionOfficeCaseloadAssignment
    {
      get => hiddenSelectionOfficeCaseloadAssignment ??= new();
      set => hiddenSelectionOfficeCaseloadAssignment = value;
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
    /// A value of InactiveOffice.
    /// </summary>
    [JsonPropertyName("inactiveOffice")]
    public Common InactiveOffice
    {
      get => inactiveOffice ??= new();
      set => inactiveOffice = value;
    }

    /// <summary>
    /// A value of ToLtrb.
    /// </summary>
    [JsonPropertyName("toLtrb")]
    public Fips ToLtrb
    {
      get => toLtrb ??= new();
      set => toLtrb = value;
    }

    private OfficeCaseloadAssignment priority;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private Common confirm;
    private Common fromOfca;
    private Office hiddenOffice;
    private Common listAsgnType;
    private OfficeCaseloadAssignment typeSearch;
    private CodeValue hiddenSelectionCodeValue;
    private CodeValue codeValue;
    private Code code;
    private Program hiddenSelectionProgram;
    private ServiceProvider hiddenSelectionServiceProvider;
    private CodeValue officeType;
    private Common listOffice;
    private ServiceProvider serviceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private OfficeCaseloadAssignment search;
    private Array<ExportGroup> export1;
    private OfficeCaseloadAssignment hiddenSelectionOfficeCaseloadAssignment;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common inactiveOffice;
    private Fips toLtrb;
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
    /// A value of Overlap.
    /// </summary>
    [JsonPropertyName("overlap")]
    public Common Overlap
    {
      get => overlap ??= new();
      set => overlap = value;
    }

    /// <summary>
    /// A value of Reset.
    /// </summary>
    [JsonPropertyName("reset")]
    public Common Reset
    {
      get => reset ??= new();
      set => reset = value;
    }

    /// <summary>
    /// A value of ResetP.
    /// </summary>
    [JsonPropertyName("resetP")]
    public OfficeCaseloadAssignment ResetP
    {
      get => resetP ??= new();
      set => resetP = value;
    }

    /// <summary>
    /// A value of ResetS.
    /// </summary>
    [JsonPropertyName("resetS")]
    public OfficeCaseloadAssignment ResetS
    {
      get => resetS ??= new();
      set => resetS = value;
    }

    /// <summary>
    /// A value of ResetR.
    /// </summary>
    [JsonPropertyName("resetR")]
    public OfficeCaseloadAssignment ResetR
    {
      get => resetR ??= new();
      set => resetR = value;
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
    /// A value of EndSpNbr.
    /// </summary>
    [JsonPropertyName("endSpNbr")]
    public ServiceProvider EndSpNbr
    {
      get => endSpNbr ??= new();
      set => endSpNbr = value;
    }

    /// <summary>
    /// A value of StartSpNbr.
    /// </summary>
    [JsonPropertyName("startSpNbr")]
    public ServiceProvider StartSpNbr
    {
      get => startSpNbr ??= new();
      set => startSpNbr = value;
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
    /// A value of WorkAlphaEnd.
    /// </summary>
    [JsonPropertyName("workAlphaEnd")]
    public TextWorkArea WorkAlphaEnd
    {
      get => workAlphaEnd ??= new();
      set => workAlphaEnd = value;
    }

    /// <summary>
    /// A value of WorkAlphaBeg.
    /// </summary>
    [JsonPropertyName("workAlphaBeg")]
    public TextWorkArea WorkAlphaBeg
    {
      get => workAlphaBeg ??= new();
      set => workAlphaBeg = value;
    }

    /// <summary>
    /// A value of OverlapAlphaEnd.
    /// </summary>
    [JsonPropertyName("overlapAlphaEnd")]
    public TextWorkArea OverlapAlphaEnd
    {
      get => overlapAlphaEnd ??= new();
      set => overlapAlphaEnd = value;
    }

    /// <summary>
    /// A value of OverlapAlphaBeg.
    /// </summary>
    [JsonPropertyName("overlapAlphaBeg")]
    public TextWorkArea OverlapAlphaBeg
    {
      get => overlapAlphaBeg ??= new();
      set => overlapAlphaBeg = value;
    }

    /// <summary>
    /// A value of Priority1.
    /// </summary>
    [JsonPropertyName("priority1")]
    public Common Priority1
    {
      get => priority1 ??= new();
      set => priority1 = value;
    }

    /// <summary>
    /// A value of Priority2.
    /// </summary>
    [JsonPropertyName("priority2")]
    public Common Priority2
    {
      get => priority2 ??= new();
      set => priority2 = value;
    }

    /// <summary>
    /// A value of Priority3.
    /// </summary>
    [JsonPropertyName("priority3")]
    public Common Priority3
    {
      get => priority3 ??= new();
      set => priority3 = value;
    }

    /// <summary>
    /// A value of Priority4.
    /// </summary>
    [JsonPropertyName("priority4")]
    public Common Priority4
    {
      get => priority4 ??= new();
      set => priority4 = value;
    }

    /// <summary>
    /// A value of FilterSelPrompt.
    /// </summary>
    [JsonPropertyName("filterSelPrompt")]
    public Common FilterSelPrompt
    {
      get => filterSelPrompt ??= new();
      set => filterSelPrompt = value;
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
    /// A value of FilterCount.
    /// </summary>
    [JsonPropertyName("filterCount")]
    public Common FilterCount
    {
      get => filterCount ??= new();
      set => filterCount = value;
    }

    /// <summary>
    /// A value of Validate.
    /// </summary>
    [JsonPropertyName("validate")]
    public Common Validate
    {
      get => validate ??= new();
      set => validate = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of SetAssign.
    /// </summary>
    [JsonPropertyName("setAssign")]
    public Common SetAssign
    {
      get => setAssign ??= new();
      set => setAssign = value;
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
    /// A value of TsBeforeCreate.
    /// </summary>
    [JsonPropertyName("tsBeforeCreate")]
    public OfficeCaseloadAssignment TsBeforeCreate
    {
      get => tsBeforeCreate ??= new();
      set => tsBeforeCreate = value;
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
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
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
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public Common J
    {
      get => j ??= new();
      set => j = value;
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

    private Array<InitializedGroup> initialized;
    private Common swap1;
    private Tribunal tribunal;
    private Tribunal initTribunal;
    private Common overlap;
    private Common reset;
    private OfficeCaseloadAssignment resetP;
    private OfficeCaseloadAssignment resetS;
    private OfficeCaseloadAssignment resetR;
    private OfficeCaseloadAssignment initOfficeCaseloadAssignment;
    private ServiceProvider endSpNbr;
    private ServiceProvider startSpNbr;
    private Program initProgram;
    private TextWorkArea workAlphaEnd;
    private TextWorkArea workAlphaBeg;
    private TextWorkArea overlapAlphaEnd;
    private TextWorkArea overlapAlphaBeg;
    private Common priority1;
    private Common priority2;
    private Common priority3;
    private Common priority4;
    private Common filterSelPrompt;
    private Common selCount;
    private Common filterCount;
    private Common validate;
    private DateWorkArea current;
    private Common returnCode;
    private DateWorkArea null1;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Program program;
    private ServiceProvider serviceProvider;
    private Common setAssign;
    private Common promptCount;
    private OfficeCaseloadAssignment tsBeforeCreate;
    private Array<LocalGroup> local1;
    private Common i;
    private CompareGroup compare;
    private Common j;
    private SwapGroup swap;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of OverlapTribunal.
    /// </summary>
    [JsonPropertyName("overlapTribunal")]
    public Tribunal OverlapTribunal
    {
      get => overlapTribunal ??= new();
      set => overlapTribunal = value;
    }

    /// <summary>
    /// A value of OverlapProgram.
    /// </summary>
    [JsonPropertyName("overlapProgram")]
    public Program OverlapProgram
    {
      get => overlapProgram ??= new();
      set => overlapProgram = value;
    }

    /// <summary>
    /// A value of OverlapOfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("overlapOfficeCaseloadAssignment")]
    public OfficeCaseloadAssignment OverlapOfficeCaseloadAssignment
    {
      get => overlapOfficeCaseloadAssignment ??= new();
      set => overlapOfficeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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

    private Fips fips;
    private Tribunal tribunal;
    private Tribunal overlapTribunal;
    private Program overlapProgram;
    private OfficeCaseloadAssignment overlapOfficeCaseloadAssignment;
    private CountyService countyService;
    private OfficeAssignmentPlan officeAssignmentPlan;
    private Code code;
    private CodeValue codeValue;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private OfficeAddress officeAddress;
    private Office office;
    private Program program;
    private CseOrganization cseOrganization;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }
#endregion
}

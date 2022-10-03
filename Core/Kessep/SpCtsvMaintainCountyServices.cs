// Program: SP_CTSV_MAINTAIN_COUNTY_SERVICES, ID: 372328097, model: 746.
// Short name: SWECTSVP
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
/// A program: SP_CTSV_MAINTAIN_COUNTY_SERVICES.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCtsvMaintainCountyServices: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CTSV_MAINTAIN_COUNTY_SERVICES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCtsvMaintainCountyServices(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCtsvMaintainCountyServices.
  /// </summary>
  public SpCtsvMaintainCountyServices(IContext context, Import import,
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
    // *********************************************
    // **   M A I N T E N A N C E    L O G
    // **
    // ** DATE      *  DESCRIPTION
    // ** 04/13/95     J. Kemp
    // ** 05/19/95     R. Grey
    // ** 02/15/96     a. hackler    retrofits
    // ** 04/25/96     S. Konkader   begin pre accepance test review
    // ** 04/26/96     J. Rookard    continue test review and debug.
    // ** 04/30/96	R. Grey       complete for acceptance tst
    // ** 01-03-97 	R. Marchman   Add new security/next tran.
    // ** 02-00-97	R. Grey	      Add Function to CTSV create
    // ** 06-18-97	R. Grey	      Fix view match for Read CTSV
    // ** 10-31-98     Anita Massey   fixes per screen assess
    // **
    // 
    // document
    // ** 6/11/99  Anita Massey    changed read property to select
    //                   only.   Deleted all disabled statements.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    export.HiddenSelect.SystemGeneratedId =
      import.HiddenSelect.SystemGeneratedId;
    export.PrevCommand.Command = import.PrevCommand.Command;
    export.Cdvl.Code.CodeName = import.Cdvl.Code.CodeName;
    MoveCodeValue(import.Cdvl.CodeValue, export.Cdvl.CodeValue);
    export.Cdvl.ReturnVal.Count = import.Cdvl.ReturnVal.Count;
    export.Select1.Code = import.Select1.Code;
    MoveCseOrganization2(import.SelectCounty, export.SelectCounty);
    export.PromptOffice.Flag = import.PromptOffice.Flag;
    export.FilterSearch.SearchCnty.Code = import.FilterSearch.SearchCnty.Code;
    export.FilterSearch.PromptScnty.SelectChar =
      import.FilterSearch.PromptScnty.SelectChar;
    export.FilterSearch.SearchPgm.Code = import.FilterSearch.SearchPgm.Code;
    export.FilterSearch.PromptSpgm.SelectChar =
      import.FilterSearch.PromptSpgm.SelectChar;
    export.FilterSearch.SearchFunc.Function =
      import.FilterSearch.SearchFunc.Function;
    export.FilterSearch.PromptSfunction.SelectChar =
      import.FilterSearch.PromptSfunction.SelectChar;
    export.FilterSearch.SearchFuncOnly.Flag =
      import.FilterSearch.SearchFuncOnly.Flag;

    if (AsChar(export.FilterSearch.SearchFuncOnly.Flag) != 'Y')
    {
      export.FilterSearch.SearchFuncOnly.Flag = "N";
    }

    export.Select.OldRec.Flag = import.Select.OldRec.Flag;

    if (AsChar(export.Select.OldRec.Flag) != 'Y')
    {
      export.Select.OldRec.Flag = "N";
    }

    export.Select.SelectOffice.Assign(import.Select.SelectOffice);
    export.Select.SelectOfficeAddress.City =
      import.Select.SelectOfficeAddress.City;

    export.List.Index = 0;
    export.List.Clear();

    for(import.List.Index = 0; import.List.Index < import.List.Count; ++
      import.List.Index)
    {
      if (export.List.IsFull)
      {
        break;
      }

      export.List.Update.ListSel.SelectChar =
        import.List.Item.ListSel.SelectChar;
      export.List.Update.ListCnty.Flag = import.List.Item.ListCnty.Flag;
      export.List.Update.ListPgm.Flag = import.List.Item.ListPgm.Flag;
      MoveCseOrganization1(import.List.Item.ListCseOrganization,
        export.List.Update.ListCseOrganization);
      export.List.Update.ListCountyService.Assign(
        import.List.Item.ListCountyService);
      MoveProgram(import.List.Item.ListProgram, export.List.Update.ListProgram);
      export.List.Update.ListFunction.Flag = import.List.Item.ListFunction.Flag;
      export.List.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // *** if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        UseScCabNextTranPut();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;

          for(export.List.Index = 0; export.List.Index < export.List.Count; ++
            export.List.Index)
          {
            var field1 = GetField(export.List.Item.ListCseOrganization, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.List.Item.ListProgram, "code");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.List.Item.ListCountyService, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            if (!Lt(local.Current.Date,
              export.List.Item.ListCountyService.DiscontinueDate))
            {
              var field4 =
                GetField(export.List.Item.ListCountyService, "discontinueDate");
                

              field4.Color = "cyan";
              field4.Protected = true;
            }

            if (AsChar(export.Select.OldRec.Flag) == 'Y')
            {
              var field4 =
                GetField(export.List.Item.ListCountyService, "discontinueDate");
                

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }
        }

        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is coming into this procedure from another screen.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
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

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    if (Equal(global.Command, "RTLIST") || Equal(global.Command, "RLCVAL"))
    {
    }
    else
    {
      // **** begin group C ****
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ********************************************
    // * End Security
    // ********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      // *******************************************
      // *  Add, Update, Delete require processing Group-View.
      // *  Do not process Group if no Office-ID.
      // *******************************************
      if (export.Select.SelectOffice.SystemGeneratedId == 0)
      {
        var field = GetField(export.Select.SelectOffice, "systemGeneratedId");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        goto Test1;
      }

      for(export.List.Index = 0; export.List.Index < export.List.Count; ++
        export.List.Index)
      {
        switch(AsChar(export.List.Item.ListSel.SelectChar))
        {
          case '*':
            export.List.Update.ListSel.SelectChar = "";

            break;
          case ' ':
            break;
          case 'S':
            ++local.Count1.Count;

            break;
          default:
            var field = GetField(export.List.Item.ListSel, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test1;
        }
      }

      if (local.Count1.Count == 0)
      {
        ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";
      }
    }

Test1:

    if (Equal(global.Command, "RLCVAL"))
    {
      // *********************************************
      // * Return from selecting a County Service Function.
      // *********************************************
      if (AsChar(export.FilterSearch.PromptSfunction.SelectChar) == 'S')
      {
        if (!IsEmpty(export.Cdvl.CodeValue.Cdvalue))
        {
          export.FilterSearch.SearchFunc.Function =
            export.Cdvl.CodeValue.Cdvalue;
        }

        local.PrevCommand.Command = global.Command;
        global.Command = "DISPLAY";
      }
      else
      {
        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (AsChar(export.List.Item.ListFunction.Flag) == 'S')
          {
            export.List.Update.ListCountyService.Function =
              export.Cdvl.CodeValue.Cdvalue;
            local.PrevCommand.Command = global.Command;
            global.Command = "DISPLAY";

            goto Test2;
          }
        }
      }
    }

Test2:

    if (Equal(global.Command, "RTLIST"))
    {
      // ********************************************
      // * Evaluate for prompt out to CSE Organization
      // * for search parameter value
      // ********************************************
      if (AsChar(export.FilterSearch.PromptScnty.SelectChar) == 'S')
      {
        export.FilterSearch.PromptScnty.SelectChar = "";

        var field = GetField(export.FilterSearch.PromptScnty, "selectChar");

        field.Protected = false;
        field.Focused = true;

        if (!IsEmpty(import.SelectCounty.Code))
        {
          export.FilterSearch.SearchCnty.Code = import.SelectCounty.Code;
        }
      }

      if (AsChar(export.FilterSearch.PromptSpgm.SelectChar) == 'S')
      {
        // ********************************************
        // * Evaluate for prompt out to Program for search
        // * parameter value
        // ********************************************
        export.FilterSearch.PromptSpgm.SelectChar = "";

        var field = GetField(export.FilterSearch.PromptSpgm, "selectChar");

        field.Protected = false;
        field.Focused = true;

        if (!IsEmpty(import.Select1.Code))
        {
          export.FilterSearch.SearchPgm.Code = import.Select1.Code;
        }
      }

      // *********************************************
      // * Return from selecting an Office
      // *********************************************
      if (AsChar(export.PromptOffice.Flag) == 'S')
      {
        export.PromptOffice.Flag = "";

        var field = GetField(export.PromptOffice, "flag");

        field.Protected = false;
        field.Focused = true;

        if (import.HiddenSaved.SystemGeneratedId > 0)
        {
          export.Select.SelectOffice.Assign(import.HiddenSaved);
          export.Select.SelectOfficeAddress.City =
            import.Select.SelectOfficeAddress.City;
          global.Command = "DISPLAY";
        }
      }
    }

    // ********************************************
    // * Main Case of Command
    // ********************************************
    switch(TrimEnd(global.Command))
    {
      case "RTLIST":
        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          // *********************************************
          // * Return from selecting a County organization
          // *********************************************
          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
            if (AsChar(export.List.Item.ListCnty.Flag) == 'S')
            {
              export.List.Update.ListCnty.Flag = "";

              var field = GetField(export.List.Item.ListCnty, "flag");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.SelectCounty.Code))
              {
                export.List.Update.ListCseOrganization.Code =
                  import.SelectCounty.Code;
              }
            }

            // *********************************************
            // * Return from selecting a Program code.
            // *********************************************
            if (AsChar(export.List.Item.ListPgm.Flag) == 'S')
            {
              export.List.Update.ListPgm.Flag = "";

              var field = GetField(export.List.Item.ListPgm, "flag");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.Select1.Code))
              {
                export.List.Update.ListProgram.Code = import.Select1.Code;
              }
            }
          }

          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
          }
          else
          {
            var field1 = GetField(export.List.Item.ListCseOrganization, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.List.Item.ListCnty, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.List.Item.ListProgram, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.List.Item.ListPgm, "flag");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.List.Item.ListCountyService, "function");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.List.Item.ListCountyService, "effectiveDate");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.List.Item.ListFunction, "flag");

            field7.Color = "cyan";
            field7.Protected = true;

            if (Lt(local.Null1.Date,
              export.List.Item.ListCountyService.DiscontinueDate))
            {
              local.DateWorkArea.Date =
                export.List.Item.ListCountyService.DiscontinueDate;
              UseCabSetMaximumDiscontinueDate1();
              export.List.Update.ListCountyService.DiscontinueDate =
                local.DateWorkArea.Date;

              if (!Lt(local.Current.Date,
                export.List.Item.ListCountyService.DiscontinueDate))
              {
                var field =
                  GetField(export.List.Item.ListCountyService, "discontinueDate");
                  

                field.Color = "cyan";
                field.Protected = true;
              }

              if (AsChar(export.Select.OldRec.Flag) == 'Y')
              {
                var field =
                  GetField(export.List.Item.ListCountyService, "discontinueDate");
                  

                field.Color = "cyan";
                field.Protected = true;
              }
            }
          }
        }

        break;
      case "DISPLAY":
        if (export.Select.SelectOffice.SystemGeneratedId == 0)
        {
          var field = GetField(export.Select.SelectOffice, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          break;
        }

        if (AsChar(export.FilterSearch.PromptSfunction.SelectChar) == 'S')
        {
          var field =
            GetField(export.FilterSearch.PromptSfunction, "selectChar");

          field.Protected = false;
          field.Focused = true;

          export.FilterSearch.PromptSfunction.SelectChar = "";
        }
        else
        {
          for(export.List.Index = 0; export.List.Index < export.List.Count; ++
            export.List.Index)
          {
            if (AsChar(export.List.Item.ListFunction.Flag) == 'S')
            {
              var field = GetField(export.List.Item.ListFunction, "flag");

              field.Protected = false;
              field.Focused = true;

              export.List.Update.ListFunction.Flag = "";
            }
          }
        }

        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          export.FilterSearch.PromptScnty.SelectChar = "";
          export.FilterSearch.PromptSpgm.SelectChar = "";
        }

        if (AsChar(export.FilterSearch.SearchFuncOnly.Flag) == 'Y')
        {
          if (!IsEmpty(export.FilterSearch.SearchPgm.Code))
          {
            var field1 = GetField(export.FilterSearch.SearchFuncOnly, "flag");

            field1.Error = true;

            var field2 = GetField(export.FilterSearch.SearchPgm, "code");

            field2.Error = true;

            ExitState = "SP0000_INVALID_SEARCH_CRITRA_W_F";

            break;
          }
        }

        // *** new
        if (export.Select.SelectOffice.SystemGeneratedId != import
          .HiddenSelect.SystemGeneratedId)
        {
          export.Select.OldRec.Flag = "N";
          export.FilterSearch.SearchFuncOnly.Flag = "N";
          export.FilterSearch.SearchCnty.Code = "";
          export.FilterSearch.SearchPgm.Code = "";
          export.FilterSearch.SearchFunc.Function = "";
        }

        if (Equal(local.PrevCommand.Command, "RLCVAL"))
        {
          // **********************************************
          // * Fall through and format Office header.
          // **********************************************
        }
        else
        {
          UseSpReadCountyServices();

          if (IsExitState("FN0000_OFFICE_NF"))
          {
            var field =
              GetField(export.Select.SelectOffice, "systemGeneratedId");

            field.Error = true;

            break;
          }
        }

        // ********************************************
        // * Maintain Protection for record identifiers and Effective Date.
        // ********************************************
        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
          }
          else
          {
            var field1 = GetField(export.List.Item.ListCseOrganization, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.List.Item.ListProgram, "code");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.List.Item.ListCountyService, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.List.Item.ListCountyService, "function");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.List.Item.ListPgm, "flag");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.List.Item.ListCnty, "flag");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.List.Item.ListFunction, "flag");

            field7.Color = "cyan";
            field7.Protected = true;

            if (Lt(local.Null1.Date,
              export.List.Item.ListCountyService.DiscontinueDate))
            {
              local.DateWorkArea.Date =
                export.List.Item.ListCountyService.DiscontinueDate;
              UseCabSetMaximumDiscontinueDate1();
              export.List.Update.ListCountyService.DiscontinueDate =
                local.DateWorkArea.Date;

              if (AsChar(export.Select.OldRec.Flag) == 'Y')
              {
                var field =
                  GetField(export.List.Item.ListCountyService, "discontinueDate");
                  

                field.Color = "cyan";
                field.Protected = true;
              }
            }
          }
        }

        // **************************************
        // * validate function entered is valid
        // **************************************
        if (!IsEmpty(export.FilterSearch.SearchFunc.Function))
        {
          export.Cdvl.Code.CodeName = "FUNCTION";
          export.Cdvl.CodeValue.Cdvalue =
            export.FilterSearch.SearchFunc.Function ?? Spaces(10);
          export.Cdvl.ReturnVal.Count = 0;
          UseCabValidateCodeValue();

          if (export.Cdvl.ReturnVal.Count != 0)
          {
            var field = GetField(export.FilterSearch.SearchFunc, "function");

            field.Error = true;

            ExitState = "CODE_VALUE_NF";
          }
        }
        else
        {
          for(export.List.Index = 0; export.List.Index < export.List.Count; ++
            export.List.Index)
          {
            if (!IsEmpty(export.List.Item.ListCountyService.Function))
            {
              export.Cdvl.Code.CodeName = "FUNCTION";
              export.Cdvl.CodeValue.Cdvalue =
                export.List.Item.ListCountyService.Function ?? Spaces(10);
              export.Cdvl.ReturnVal.Count = 0;
              UseCabValidateCodeValue();

              if (export.Cdvl.ReturnVal.Count != 0)
              {
                var field =
                  GetField(export.List.Item.ListCountyService, "function");

                field.Error = true;

                ExitState = "CODE_VALUE_NF";
              }
            }
          }
        }

        break;
      case "LIST":
        // *******************************************
        // * Determine if prompt for Office.
        // *******************************************
        switch(AsChar(export.PromptOffice.Flag))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.FilterSearch.PromptScnty.SelectChar))
            {
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              var field1 = GetField(export.PromptOffice, "flag");

              field1.Error = true;

              var field2 =
                GetField(export.FilterSearch.PromptScnty, "selectChar");

              field2.Error = true;

              goto Test4;
            }
            else if (!IsEmpty(export.FilterSearch.PromptSfunction.SelectChar))
            {
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              var field1 = GetField(export.PromptOffice, "flag");

              field1.Error = true;

              var field2 =
                GetField(export.FilterSearch.PromptSfunction, "selectChar");

              field2.Error = true;

              goto Test4;
            }
            else if (!IsEmpty(export.FilterSearch.PromptSpgm.SelectChar))
            {
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              var field1 = GetField(export.PromptOffice, "flag");

              field1.Error = true;

              var field2 =
                GetField(export.FilterSearch.PromptSpgm, "selectChar");

              field2.Error = true;

              goto Test4;
            }
            else
            {
              export.HiddenSaved.Assign(import.Select.SelectOffice);
              ExitState = "ECO_LNK_TO_LIST_OFFICE";

              return;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.PromptOffice, "flag");

            field.Error = true;

            goto Test4;
        }

        // *******************************************
        // * Determine if prompt for County search parameter.
        // *******************************************
        if (AsChar(export.FilterSearch.SearchFuncOnly.Flag) == 'Y')
        {
          if (AsChar(export.FilterSearch.PromptSpgm.SelectChar) == 'S')
          {
            var field1 = GetField(export.FilterSearch.SearchFuncOnly, "flag");

            field1.Error = true;

            var field2 = GetField(export.FilterSearch.SearchPgm, "code");

            field2.Error = true;

            ExitState = "SP0000_INVALID_SEARCH_CRITRA_W_F";

            break;
          }
        }

        if (AsChar(export.FilterSearch.PromptScnty.SelectChar) == 'S')
        {
          if (!IsEmpty(export.FilterSearch.PromptSfunction.SelectChar) || !
            IsEmpty(export.FilterSearch.PromptSpgm.SelectChar))
          {
            var field = GetField(export.FilterSearch.PromptSpgm, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
          else
          {
            export.SelectCounty.Type1 = "C";
            ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";

            return;
          }
        }

        // *******************************************
        // * Determine if prompt for Program search parameter.
        // *******************************************
        if (!IsEmpty(export.FilterSearch.SearchFunc.Function) && AsChar
          (export.FilterSearch.SearchFuncOnly.Flag) == 'Y')
        {
          if (!IsEmpty(export.FilterSearch.SearchPgm.Code))
          {
            var field = GetField(export.FilterSearch.SearchPgm, "code");

            field.Error = true;

            ExitState = "SP0000_INVALID_SEARCH_CRITRA_W_F";

            break;
          }
        }
        else if (AsChar(export.FilterSearch.PromptSpgm.SelectChar) == 'S')
        {
          if (!IsEmpty(export.FilterSearch.PromptSfunction.SelectChar) || !
            IsEmpty(export.FilterSearch.PromptScnty.SelectChar))
          {
            var field = GetField(export.FilterSearch.PromptSpgm, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
          else
          {
            ExitState = "ECO_LNK_TO_PROGRAM_MAINTENANCE";

            return;
          }
        }

        // *******************************************
        // * Determine if prompt for Function search parameter.
        // *******************************************
        if (AsChar(export.FilterSearch.PromptSfunction.SelectChar) == 'S')
        {
          if (!IsEmpty(export.FilterSearch.PromptSpgm.SelectChar) || !
            IsEmpty(export.FilterSearch.PromptScnty.SelectChar))
          {
            var field = GetField(export.FilterSearch.PromptSpgm, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
          else
          {
            export.Cdvl.Code.CodeName = "FUNCTION";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
        }

        // *******************************************
        // * Determine if prompt for RGV County organization.
        // *******************************************
        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          switch(AsChar(export.List.Item.ListCnty.Flag))
          {
            case ' ':
              break;
            case 'S':
              if (!IsEmpty(export.PromptOffice.Flag))
              {
                ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

                goto Test4;
              }

              export.SelectCounty.Type1 = "C";
              ++local.Count1.Count;

              break;
            default:
              var field = GetField(export.List.Item.ListCnty, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              goto Test4;
          }

          // *******************************************
          // * Determine if prompt for RGV Program code.
          // *******************************************
          switch(AsChar(export.List.Item.ListPgm.Flag))
          {
            case ' ':
              break;
            case 'S':
              if (!IsEmpty(export.PromptOffice.Flag))
              {
                ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

                goto Test4;
              }

              ++local.Count2.Count;

              break;
            default:
              var field = GetField(export.List.Item.ListPgm, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              goto Test4;
          }

          // *******************************************
          // * Determine if prompt for RGV County Service Function Code.
          // *******************************************
          switch(AsChar(export.List.Item.ListFunction.Flag))
          {
            case ' ':
              break;
            case 'S':
              if (!IsEmpty(export.PromptOffice.Flag))
              {
                ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

                goto Test4;
              }

              ++local.Count3.Count;

              break;
            default:
              var field = GetField(export.List.Item.ListFunction, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              goto Test4;
          }

          if (local.Count1.Count > 0 || local.Count2.Count > 0 || local
            .Count3.Count > 0)
          {
            if (AsChar(export.List.Item.ListSel.SelectChar) != 'S')
            {
              var field = GetField(export.List.Item.ListSel, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              goto Test4;
            }
          }
        }

        // ***  SEE IF THEY HAVE MADE MULT SELECTIONS FOR  PROMPT IN LOWER
        // SECTION OF SCREEN
        if (local.Count1.Count > 0)
        {
          if (local.Count2.Count > 0 || local.Count3.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
        }

        if (local.Count2.Count > 0)
        {
          if (local.Count1.Count > 0 || local.Count3.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
        }

        if (local.Count3.Count > 0)
        {
          if (local.Count1.Count > 0 || local.Count2.Count > 0)
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
        }

        // ***  See which choice they made and flow out
        if (local.Count1.Count > 0)
        {
          switch(local.Count1.Count)
          {
            case 0:
              break;
            case 1:
              ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";

              return;
            default:
              break;
          }
        }

        if (local.Count2.Count > 0)
        {
          switch(local.Count2.Count)
          {
            case 0:
              break;
            case 1:
              ExitState = "ECO_LNK_TO_PROGRAM_MAINTENANCE";

              return;
            default:
              break;
          }
        }

        if (local.Count3.Count > 0)
        {
          switch(local.Count3.Count)
          {
            case 0:
              break;
            case 1:
              export.Cdvl.Code.CodeName = "FUNCTION";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            default:
              break;
          }
        }

        if (IsEmpty(export.PromptOffice.Flag) && local.Count1.Count == 0 && local
          .Count2.Count == 0 && local.Count3.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "ADD":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
            if (IsEmpty(export.List.Item.ListCseOrganization.Code))
            {
              var field =
                GetField(export.List.Item.ListCseOrganization, "code");

              field.Error = true;

              export.List.Update.ListSel.SelectChar = "S";
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test4;
            }

            if (IsEmpty(export.List.Item.ListProgram.Code) && IsEmpty
              (export.List.Item.ListCountyService.Function))
            {
              var field1 = GetField(export.List.Item.ListProgram, "code");

              field1.Error = true;

              var field2 =
                GetField(export.List.Item.ListCountyService, "function");

              field2.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test4;
            }

            // *******************************************
            // * Only valid Office relationship to CSE
            // * Organization is for County.
            // *******************************************
            export.List.Update.ListCseOrganization.Type1 = "C";

            // *******************************************
            // * Determine appropriate program type and
            // * validate that Program and Function not created
            // * for same County Service occurance.  Each one
            // * needs its own separate row.
            // *******************************************
            if (!IsEmpty(export.List.Item.ListProgram.Code))
            {
              if (!IsEmpty(export.List.Item.ListCountyService.Function))
              {
                ExitState = "SP0000_INVALID_COUNTY_SERVICE_TY";

                var field1 = GetField(export.List.Item.ListProgram, "code");

                field1.Error = true;

                var field2 =
                  GetField(export.List.Item.ListCountyService, "function");

                field2.Error = true;

                goto Test4;
              }
              else
              {
                export.List.Update.ListCountyService.Type1 = "P";
              }
            }
            else if (!IsEmpty(export.List.Item.ListCountyService.Function))
            {
              if (!IsEmpty(export.List.Item.ListProgram.Code))
              {
                ExitState = "SP0000_INVALID_COUNTY_SERVICE_TY";

                var field1 = GetField(export.List.Item.ListProgram, "code");

                field1.Error = true;

                var field2 =
                  GetField(export.List.Item.ListCountyService, "function");

                field2.Error = true;

                goto Test4;
              }
            }

            if (AsChar(export.List.Item.ListCountyService.Type1) != 'P')
            {
              export.List.Update.ListCountyService.Type1 = "F";
            }

            // ******************************************
            // * Default Effective Date to Current Date
            // * If no value entered, and don't let the
            // * user enter anything less than current date
            // ******************************************
            if (Lt(export.List.Item.ListCountyService.EffectiveDate, Now().Date) ||
              Equal
              (export.List.Item.ListCountyService.EffectiveDate,
              local.Null1.Date))
            {
              export.List.Update.ListCountyService.EffectiveDate = Now().Date;
            }

            // ******************************************
            // * Default Discontinue Date to Maximum Date
            // * if no value entered.
            // ******************************************
            if (Equal(export.List.Item.ListCountyService.DiscontinueDate,
              local.Null1.Date))
            {
              export.DateWorkArea.Date = UseCabSetMaximumDiscontinueDate2();
              export.List.Update.ListCountyService.DiscontinueDate =
                export.DateWorkArea.Date;
            }

            if (!Lt(export.List.Item.ListCountyService.EffectiveDate,
              export.List.Item.ListCountyService.DiscontinueDate))
            {
              ExitState = "INVALID_EFF_END_DATE_COMBINATION";

              var field =
                GetField(export.List.Item.ListCountyService, "discontinueDate");
                

              field.Error = true;

              goto Test3;
            }

            // **************************************
            // * validate function entered is valid
            // **************************************
            if (!IsEmpty(export.List.Item.ListCountyService.Function) && !
              Equal(export.List.Item.ListCountyService.Function, "*"))
            {
              export.Cdvl.Code.CodeName = "FUNCTION";
              export.Cdvl.CodeValue.Cdvalue =
                export.List.Item.ListCountyService.Function ?? Spaces(10);
              export.Cdvl.ReturnVal.Count = 0;
              UseCabValidateCodeValue();

              if (export.Cdvl.ReturnVal.Count != 0)
              {
                var field =
                  GetField(export.List.Item.ListCountyService, "function");

                field.Error = true;

                ExitState = "CODE_VALUE_NF";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK") && !
              IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              goto Test3;
            }

            if (AsChar(export.List.Item.ListCountyService.Type1) == 'P' && IsEmpty
              (export.List.Item.ListProgram.Code))
            {
              export.List.Update.ListCountyService.Type1 = "F";
            }

            export.List.Update.ListCountyService.SystemGeneratedIdentifier = 0;
            UseSpCreateCountyServiceAssign();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              export.List.Update.ListSel.SelectChar = "*";
              export.List.Update.ListCnty.Flag = "";
              export.List.Update.ListPgm.Flag = "";
              export.List.Update.ListFunction.Flag = "";
            }
          }

Test3:

          // ********************************************
          // * Maintain Protection for record identifiers and Effective Date.
          // ********************************************
          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
          }
          else
          {
            var field1 = GetField(export.List.Item.ListCseOrganization, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.List.Item.ListProgram, "code");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.List.Item.ListCountyService, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.List.Item.ListCountyService, "function");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.List.Item.ListPgm, "flag");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.List.Item.ListCnty, "flag");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.List.Item.ListFunction, "flag");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          if (Lt(local.Null1.Date,
            export.List.Item.ListCountyService.DiscontinueDate))
          {
            local.DateWorkArea.Date =
              export.List.Item.ListCountyService.DiscontinueDate;
            UseCabSetMaximumDiscontinueDate1();
            export.List.Update.ListCountyService.DiscontinueDate =
              local.DateWorkArea.Date;
          }

          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
            if (IsExitState("INVALID_EFF_END_DATE_COMBINATION"))
            {
              var field =
                GetField(export.List.Item.ListCountyService, "effectiveDate");

              field.Protected = false;
            }
            else if (IsExitState("FN0000_OFFICE_NF"))
            {
              var field =
                GetField(export.Select.SelectOffice, "systemGeneratedId");

              field.Error = true;
            }
            else if (IsExitState("COUNTY_SERVICES_AE"))
            {
              var field1 =
                GetField(export.List.Item.ListCseOrganization, "code");

              field1.Protected = false;

              var field2 = GetField(export.List.Item.ListProgram, "code");

              field2.Protected = false;

              var field3 =
                GetField(export.List.Item.ListCountyService, "effectiveDate");

              field3.Protected = false;

              var field4 = GetField(export.List.Item.ListSel, "selectChar");

              field4.Error = true;
            }
            else if (IsExitState("INVALID_PROGRAM"))
            {
              var field1 = GetField(export.List.Item.ListProgram, "code");

              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.List.Item.ListProgram, "code");

              field2.Error = true;
            }
            else if (IsExitState("INVALID_CSE_ORG"))
            {
              var field1 =
                GetField(export.List.Item.ListCseOrganization, "code");

              field1.Protected = false;
              field1.Focused = true;

              var field2 =
                GetField(export.List.Item.ListCseOrganization, "code");

              field2.Error = true;
            }
            else if (IsExitState("CODE_VALUE_NF"))
            {
              var field =
                GetField(export.List.Item.ListCountyService, "function");

              field.Error = true;
            }
            else
            {
            }
          }
        }

        break;
      case "UPDATE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (AsChar(export.Select.OldRec.Flag) == 'N' && AsChar
            (export.List.Item.ListSel.SelectChar) == 'S')
          {
            if (!Lt(export.List.Item.ListCountyService.EffectiveDate,
              export.List.Item.ListCountyService.DiscontinueDate) && !
              Equal(export.List.Item.ListCountyService.DiscontinueDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.List.Item.ListCountyService, "discontinueDate");
                

              field.Error = true;

              ExitState = "INVALID_EFF_END_DATE_COMBINATION";

              break;
            }

            // *** Anita
            if (Equal(export.List.Item.ListCountyService.DiscontinueDate,
              local.Null1.Date))
            {
              export.DateWorkArea.Date = UseCabSetMaximumDiscontinueDate2();
              export.List.Update.ListCountyService.DiscontinueDate =
                export.DateWorkArea.Date;
            }

            UseSpUpdtDelCntyServAssignmt();
          }

          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
            if (IsExitState("ACO_NI0000_UPDATE_SUCCESSFUL"))
            {
              export.List.Update.ListSel.SelectChar = "*";
              export.List.Update.ListCnty.Flag = "";
              export.List.Update.ListPgm.Flag = "";
            }
            else
            {
              var field1 =
                GetField(export.List.Item.ListCountyService, "discontinueDate");
                

              field1.Color = "";
              field1.Protected = false;

              var field2 =
                GetField(export.List.Item.ListCountyService, "discontinueDate");
                

              field2.Error = true;
            }
          }

          // ********************************************
          // * Maintain Protection for record identifiers and Effective Date.
          // ********************************************
          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
          }
          else
          {
            var field1 = GetField(export.List.Item.ListCseOrganization, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.List.Item.ListProgram, "code");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.List.Item.ListCountyService, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.List.Item.ListPgm, "flag");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.List.Item.ListCnty, "flag");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.List.Item.ListFunction, "flag");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          if (Lt(local.Null1.Date,
            export.List.Item.ListCountyService.DiscontinueDate))
          {
            local.DateWorkArea.Date =
              export.List.Item.ListCountyService.DiscontinueDate;
            UseCabSetMaximumDiscontinueDate1();
            export.List.Update.ListCountyService.DiscontinueDate =
              local.DateWorkArea.Date;

            if (Lt(export.List.Item.ListCountyService.DiscontinueDate,
              Now().Date))
            {
              var field =
                GetField(export.List.Item.ListCountyService, "discontinueDate");
                

              field.Color = "cyan";
              field.Protected = true;
            }
          }

          if (AsChar(export.Select.OldRec.Flag) == 'Y')
          {
            ExitState = "ACO_NE0000_INVALID_ACTION";

            if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
            {
              export.List.Update.ListSel.SelectChar = "*";
              export.List.Update.ListCnty.Flag = "";
              export.List.Update.ListPgm.Flag = "";

              var field1 = GetField(export.List.Item.ListSel, "selectChar");

              field1.Error = true;
            }

            var field =
              GetField(export.List.Item.ListCountyService, "discontinueDate");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DELETE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (AsChar(export.List.Item.ListSel.SelectChar) == 'S')
          {
            local.DeleteFlag.Flag = "";

            if (ReadOfficeCaseloadAssignment())
            {
              local.DeleteFlag.Flag = "N";
            }

            if (AsChar(local.DeleteFlag.Flag) == 'N')
            {
              var field = GetField(export.List.Item.ListSel, "selectChar");

              field.Error = true;

              ExitState = "SP0000_DELETE_NOT_ALLOWED";

              goto Test4;
            }

            export.PrevCommand.Command = "DELETE";
            UseSpUpdtDelCntyServAssignmt();
            export.List.Update.ListSel.SelectChar = "*";
            export.List.Update.ListCnty.Flag = "";
            export.List.Update.ListPgm.Flag = "";

            if (!IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
              export.List.Update.ListSel.SelectChar = "";

              var field = GetField(export.List.Item.ListSel, "selectChar");

              field.Error = true;
            }
          }

          // ********************************************
          // * Maintain Protection for record identifiers and Effective Date.
          // ********************************************
          var field1 = GetField(export.List.Item.ListCseOrganization, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.List.Item.ListProgram, "code");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.List.Item.ListCountyService, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.List.Item.ListPgm, "flag");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.List.Item.ListCnty, "flag");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.List.Item.ListFunction, "flag");

          field6.Color = "cyan";
          field6.Protected = true;

          if (Lt(local.Null1.Date,
            export.List.Item.ListCountyService.DiscontinueDate))
          {
            local.DateWorkArea.Date =
              export.List.Item.ListCountyService.DiscontinueDate;
            UseCabSetMaximumDiscontinueDate1();
            export.List.Update.ListCountyService.DiscontinueDate =
              local.DateWorkArea.Date;
          }
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test4:

    for(export.List.Index = 0; export.List.Index < export.List.Count; ++
      export.List.Index)
    {
      if (Lt(local.Null1.Date, export.List.Item.ListCountyService.EffectiveDate))
        
      {
        if (!Lt(local.Current.Date,
          export.List.Item.ListCountyService.EffectiveDate))
        {
          var field1 = GetField(export.List.Item.ListCseOrganization, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.List.Item.ListProgram, "code");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.List.Item.ListCountyService, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.List.Item.ListCountyService, "function");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.List.Item.ListPgm, "flag");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.List.Item.ListCnty, "flag");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.List.Item.ListFunction, "flag");

          field7.Color = "cyan";
          field7.Protected = true;

          if (AsChar(export.Select.OldRec.Flag) == 'Y')
          {
            var field =
              GetField(export.List.Item.ListCountyService, "discontinueDate");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
      }
    }

    export.PrevCommand.Command = global.Command;
    export.HiddenSelect.SystemGeneratedId =
      export.Select.SelectOffice.SystemGeneratedId;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCountyService(CountyService source,
    CountyService target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveCseOrganization1(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private static void MoveCseOrganization2(CseOrganization source,
    CseOrganization target)
  {
    target.Type1 = source.Type1;
    target.Name = source.Name;
  }

  private static void MoveList(SpReadCountyServices.Export.ListGroup source,
    Export.ListGroup target)
  {
    target.ListSel.SelectChar = source.ListSel.SelectChar;
    target.ListCnty.Flag = source.ListCnty.Flag;
    target.ListPgm.Flag = source.ListPgm.Flag;
    target.ListFunction.Flag = source.ListFunction.Flag;
    target.ListCseOrganization.Assign(source.ListCseOrganization);
    target.ListCountyService.Assign(source.ListCountyService);
    target.ListProgram.Assign(source.ListProgram);
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

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
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

    useImport.Code.CodeName = export.Cdvl.Code.CodeName;
    useImport.CodeValue.Cdvalue = export.Cdvl.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    export.Cdvl.ReturnVal.Count = useExport.ReturnCode.Count;
    MoveCodeValue(useExport.CodeValue, export.Cdvl.CodeValue);
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

  private void UseSpCreateCountyServiceAssign()
  {
    var useImport = new SpCreateCountyServiceAssign.Import();
    var useExport = new SpCreateCountyServiceAssign.Export();

    useImport.Office.SystemGeneratedId =
      export.Select.SelectOffice.SystemGeneratedId;
    MoveCseOrganization1(export.List.Item.ListCseOrganization,
      useImport.Import1.CseOrganization);
    useImport.Import1.CountyService.Assign(export.List.Item.ListCountyService);
    useImport.Import1.Program.Assign(export.List.Item.ListProgram);

    Call(SpCreateCountyServiceAssign.Execute, useImport, useExport);

    export.SelectCounty.Type1 = useExport.Export1.CseOrganization.Type1;
    export.Select.SelectOffice.SystemGeneratedId =
      useExport.Office.SystemGeneratedId;
    export.List.Update.ListCountyService.
      Assign(useExport.Export1.CountyService);
    export.List.Update.ListProgram.Assign(useExport.Export1.Program);
  }

  private void UseSpReadCountyServices()
  {
    var useImport = new SpReadCountyServices.Import();
    var useExport = new SpReadCountyServices.Export();

    useImport.SearchCntyPgm.SearchProgram.Code =
      export.FilterSearch.SearchPgm.Code;
    useImport.SearchCntyPgm.SearchCseOrganization.Code =
      export.FilterSearch.SearchCnty.Code;
    useImport.SearchCntyPgm.SearchFunction.Function =
      export.FilterSearch.SearchFunc.Function;
    useImport.SearchCntyPgm.SearchFuncOnly.Flag =
      export.FilterSearch.SearchFuncOnly.Flag;
    useImport.SearchOffice.OldRec.Flag = export.Select.OldRec.Flag;
    useImport.SearchOffice.SearchOffice1.Assign(export.Select.SelectOffice);

    Call(SpReadCountyServices.Execute, useImport, useExport);

    useExport.List.CopyTo(export.List, MoveList);
    export.Select.SelectOffice.Assign(useExport.Search.SearchOffice1);
  }

  private void UseSpUpdtDelCntyServAssignmt()
  {
    var useImport = new SpUpdtDelCntyServAssignmt.Import();
    var useExport = new SpUpdtDelCntyServAssignmt.Export();

    useImport.Command.Command = export.PrevCommand.Command;
    useImport.Office.SystemGeneratedId =
      export.Select.SelectOffice.SystemGeneratedId;
    MoveCseOrganization1(export.List.Item.ListCseOrganization,
      useImport.CseOrganization);
    useImport.CountyService.Assign(export.List.Item.ListCountyService);

    Call(SpUpdtDelCntyServAssignmt.Execute, useImport, useExport);

    MoveCountyService(useExport.CountyService,
      export.List.Update.ListCountyService);
  }

  private bool ReadOfficeCaseloadAssignment()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId",
          export.Select.SelectOffice.SystemGeneratedId);
        db.SetNullableString(
          command, "cogCode", export.List.Item.ListCseOrganization.Code);
        db.SetInt32(
          command, "programId",
          export.List.Item.ListProgram.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "function", export.List.Item.ListCountyService.Function ?? ""
          );
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.OfficeCaseloadAssignment.CogTypeCode =
          db.GetNullableString(reader, 3);
        entities.OfficeCaseloadAssignment.CogCode =
          db.GetNullableString(reader, 4);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 5);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.OfficeCaseloadAssignment.Populated = true;
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
    /// <summary>A FilterSearchGroup group.</summary>
    [Serializable]
    public class FilterSearchGroup
    {
      /// <summary>
      /// A value of SearchCnty.
      /// </summary>
      [JsonPropertyName("searchCnty")]
      public CseOrganization SearchCnty
      {
        get => searchCnty ??= new();
        set => searchCnty = value;
      }

      /// <summary>
      /// A value of PromptScnty.
      /// </summary>
      [JsonPropertyName("promptScnty")]
      public Common PromptScnty
      {
        get => promptScnty ??= new();
        set => promptScnty = value;
      }

      /// <summary>
      /// A value of SearchPgm.
      /// </summary>
      [JsonPropertyName("searchPgm")]
      public Program SearchPgm
      {
        get => searchPgm ??= new();
        set => searchPgm = value;
      }

      /// <summary>
      /// A value of PromptSpgm.
      /// </summary>
      [JsonPropertyName("promptSpgm")]
      public Common PromptSpgm
      {
        get => promptSpgm ??= new();
        set => promptSpgm = value;
      }

      /// <summary>
      /// A value of SearchFunc.
      /// </summary>
      [JsonPropertyName("searchFunc")]
      public CountyService SearchFunc
      {
        get => searchFunc ??= new();
        set => searchFunc = value;
      }

      /// <summary>
      /// A value of PromptSfunction.
      /// </summary>
      [JsonPropertyName("promptSfunction")]
      public Common PromptSfunction
      {
        get => promptSfunction ??= new();
        set => promptSfunction = value;
      }

      /// <summary>
      /// A value of SearchFuncOnly.
      /// </summary>
      [JsonPropertyName("searchFuncOnly")]
      public Common SearchFuncOnly
      {
        get => searchFuncOnly ??= new();
        set => searchFuncOnly = value;
      }

      private CseOrganization searchCnty;
      private Common promptScnty;
      private Program searchPgm;
      private Common promptSpgm;
      private CountyService searchFunc;
      private Common promptSfunction;
      private Common searchFuncOnly;
    }

    /// <summary>A SelectGroup group.</summary>
    [Serializable]
    public class SelectGroup
    {
      /// <summary>
      /// A value of OldRec.
      /// </summary>
      [JsonPropertyName("oldRec")]
      public Common OldRec
      {
        get => oldRec ??= new();
        set => oldRec = value;
      }

      /// <summary>
      /// A value of SelectOfficeAddress.
      /// </summary>
      [JsonPropertyName("selectOfficeAddress")]
      public OfficeAddress SelectOfficeAddress
      {
        get => selectOfficeAddress ??= new();
        set => selectOfficeAddress = value;
      }

      /// <summary>
      /// A value of SelectOffice.
      /// </summary>
      [JsonPropertyName("selectOffice")]
      public Office SelectOffice
      {
        get => selectOffice ??= new();
        set => selectOffice = value;
      }

      private Common oldRec;
      private OfficeAddress selectOfficeAddress;
      private Office selectOffice;
    }

    /// <summary>A CdvlGroup group.</summary>
    [Serializable]
    public class CdvlGroup
    {
      /// <summary>
      /// A value of ReturnVal.
      /// </summary>
      [JsonPropertyName("returnVal")]
      public Common ReturnVal
      {
        get => returnVal ??= new();
        set => returnVal = value;
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

      private Common returnVal;
      private Code code;
      private CodeValue codeValue;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of ListSel.
      /// </summary>
      [JsonPropertyName("listSel")]
      public Common ListSel
      {
        get => listSel ??= new();
        set => listSel = value;
      }

      /// <summary>
      /// A value of ListCnty.
      /// </summary>
      [JsonPropertyName("listCnty")]
      public Common ListCnty
      {
        get => listCnty ??= new();
        set => listCnty = value;
      }

      /// <summary>
      /// A value of ListPgm.
      /// </summary>
      [JsonPropertyName("listPgm")]
      public Common ListPgm
      {
        get => listPgm ??= new();
        set => listPgm = value;
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
      /// A value of ListCseOrganization.
      /// </summary>
      [JsonPropertyName("listCseOrganization")]
      public CseOrganization ListCseOrganization
      {
        get => listCseOrganization ??= new();
        set => listCseOrganization = value;
      }

      /// <summary>
      /// A value of ListCountyService.
      /// </summary>
      [JsonPropertyName("listCountyService")]
      public CountyService ListCountyService
      {
        get => listCountyService ??= new();
        set => listCountyService = value;
      }

      /// <summary>
      /// A value of ListProgram.
      /// </summary>
      [JsonPropertyName("listProgram")]
      public Program ListProgram
      {
        get => listProgram ??= new();
        set => listProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common listSel;
      private Common listCnty;
      private Common listPgm;
      private Common listFunction;
      private CseOrganization listCseOrganization;
      private CountyService listCountyService;
      private Program listProgram;
    }

    /// <summary>
    /// A value of HiddenSaved.
    /// </summary>
    [JsonPropertyName("hiddenSaved")]
    public Office HiddenSaved
    {
      get => hiddenSaved ??= new();
      set => hiddenSaved = value;
    }

    /// <summary>
    /// A value of HiddenSelect.
    /// </summary>
    [JsonPropertyName("hiddenSelect")]
    public Office HiddenSelect
    {
      get => hiddenSelect ??= new();
      set => hiddenSelect = value;
    }

    /// <summary>
    /// Gets a value of FilterSearch.
    /// </summary>
    [JsonPropertyName("filterSearch")]
    public FilterSearchGroup FilterSearch
    {
      get => filterSearch ?? (filterSearch = new());
      set => filterSearch = value;
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
    /// A value of Select1.
    /// </summary>
    [JsonPropertyName("select1")]
    public Program Select1
    {
      get => select1 ??= new();
      set => select1 = value;
    }

    /// <summary>
    /// A value of SelectCounty.
    /// </summary>
    [JsonPropertyName("selectCounty")]
    public CseOrganization SelectCounty
    {
      get => selectCounty ??= new();
      set => selectCounty = value;
    }

    /// <summary>
    /// Gets a value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public SelectGroup Select
    {
      get => select ?? (select = new());
      set => select = value;
    }

    /// <summary>
    /// A value of PrevCommand.
    /// </summary>
    [JsonPropertyName("prevCommand")]
    public Common PrevCommand
    {
      get => prevCommand ??= new();
      set => prevCommand = value;
    }

    /// <summary>
    /// A value of PromptOffice.
    /// </summary>
    [JsonPropertyName("promptOffice")]
    public Common PromptOffice
    {
      get => promptOffice ??= new();
      set => promptOffice = value;
    }

    /// <summary>
    /// Gets a value of Cdvl.
    /// </summary>
    [JsonPropertyName("cdvl")]
    public CdvlGroup Cdvl
    {
      get => cdvl ?? (cdvl = new());
      set => cdvl = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
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

    private Office hiddenSaved;
    private Office hiddenSelect;
    private FilterSearchGroup filterSearch;
    private DateWorkArea dateWorkArea;
    private Program select1;
    private CseOrganization selectCounty;
    private SelectGroup select;
    private Common prevCommand;
    private Common promptOffice;
    private CdvlGroup cdvl;
    private Array<ListGroup> list;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FilterSearchGroup group.</summary>
    [Serializable]
    public class FilterSearchGroup
    {
      /// <summary>
      /// A value of SearchCnty.
      /// </summary>
      [JsonPropertyName("searchCnty")]
      public CseOrganization SearchCnty
      {
        get => searchCnty ??= new();
        set => searchCnty = value;
      }

      /// <summary>
      /// A value of PromptScnty.
      /// </summary>
      [JsonPropertyName("promptScnty")]
      public Common PromptScnty
      {
        get => promptScnty ??= new();
        set => promptScnty = value;
      }

      /// <summary>
      /// A value of SearchPgm.
      /// </summary>
      [JsonPropertyName("searchPgm")]
      public Program SearchPgm
      {
        get => searchPgm ??= new();
        set => searchPgm = value;
      }

      /// <summary>
      /// A value of PromptSpgm.
      /// </summary>
      [JsonPropertyName("promptSpgm")]
      public Common PromptSpgm
      {
        get => promptSpgm ??= new();
        set => promptSpgm = value;
      }

      /// <summary>
      /// A value of SearchFunc.
      /// </summary>
      [JsonPropertyName("searchFunc")]
      public CountyService SearchFunc
      {
        get => searchFunc ??= new();
        set => searchFunc = value;
      }

      /// <summary>
      /// A value of PromptSfunction.
      /// </summary>
      [JsonPropertyName("promptSfunction")]
      public Common PromptSfunction
      {
        get => promptSfunction ??= new();
        set => promptSfunction = value;
      }

      /// <summary>
      /// A value of SearchFuncOnly.
      /// </summary>
      [JsonPropertyName("searchFuncOnly")]
      public Common SearchFuncOnly
      {
        get => searchFuncOnly ??= new();
        set => searchFuncOnly = value;
      }

      private CseOrganization searchCnty;
      private Common promptScnty;
      private Program searchPgm;
      private Common promptSpgm;
      private CountyService searchFunc;
      private Common promptSfunction;
      private Common searchFuncOnly;
    }

    /// <summary>A SelectGroup group.</summary>
    [Serializable]
    public class SelectGroup
    {
      /// <summary>
      /// A value of OldRec.
      /// </summary>
      [JsonPropertyName("oldRec")]
      public Common OldRec
      {
        get => oldRec ??= new();
        set => oldRec = value;
      }

      /// <summary>
      /// A value of SelectOffice.
      /// </summary>
      [JsonPropertyName("selectOffice")]
      public Office SelectOffice
      {
        get => selectOffice ??= new();
        set => selectOffice = value;
      }

      /// <summary>
      /// A value of SelectOfficeAddress.
      /// </summary>
      [JsonPropertyName("selectOfficeAddress")]
      public OfficeAddress SelectOfficeAddress
      {
        get => selectOfficeAddress ??= new();
        set => selectOfficeAddress = value;
      }

      private Common oldRec;
      private Office selectOffice;
      private OfficeAddress selectOfficeAddress;
    }

    /// <summary>A CdvlGroup group.</summary>
    [Serializable]
    public class CdvlGroup
    {
      /// <summary>
      /// A value of ReturnVal.
      /// </summary>
      [JsonPropertyName("returnVal")]
      public Common ReturnVal
      {
        get => returnVal ??= new();
        set => returnVal = value;
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

      private Common returnVal;
      private Code code;
      private CodeValue codeValue;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of ListSel.
      /// </summary>
      [JsonPropertyName("listSel")]
      public Common ListSel
      {
        get => listSel ??= new();
        set => listSel = value;
      }

      /// <summary>
      /// A value of ListCnty.
      /// </summary>
      [JsonPropertyName("listCnty")]
      public Common ListCnty
      {
        get => listCnty ??= new();
        set => listCnty = value;
      }

      /// <summary>
      /// A value of ListPgm.
      /// </summary>
      [JsonPropertyName("listPgm")]
      public Common ListPgm
      {
        get => listPgm ??= new();
        set => listPgm = value;
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
      /// A value of ListCseOrganization.
      /// </summary>
      [JsonPropertyName("listCseOrganization")]
      public CseOrganization ListCseOrganization
      {
        get => listCseOrganization ??= new();
        set => listCseOrganization = value;
      }

      /// <summary>
      /// A value of ListCountyService.
      /// </summary>
      [JsonPropertyName("listCountyService")]
      public CountyService ListCountyService
      {
        get => listCountyService ??= new();
        set => listCountyService = value;
      }

      /// <summary>
      /// A value of ListProgram.
      /// </summary>
      [JsonPropertyName("listProgram")]
      public Program ListProgram
      {
        get => listProgram ??= new();
        set => listProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common listSel;
      private Common listCnty;
      private Common listPgm;
      private Common listFunction;
      private CseOrganization listCseOrganization;
      private CountyService listCountyService;
      private Program listProgram;
    }

    /// <summary>
    /// A value of HiddenSaved.
    /// </summary>
    [JsonPropertyName("hiddenSaved")]
    public Office HiddenSaved
    {
      get => hiddenSaved ??= new();
      set => hiddenSaved = value;
    }

    /// <summary>
    /// A value of HiddenSelect.
    /// </summary>
    [JsonPropertyName("hiddenSelect")]
    public Office HiddenSelect
    {
      get => hiddenSelect ??= new();
      set => hiddenSelect = value;
    }

    /// <summary>
    /// Gets a value of FilterSearch.
    /// </summary>
    [JsonPropertyName("filterSearch")]
    public FilterSearchGroup FilterSearch
    {
      get => filterSearch ?? (filterSearch = new());
      set => filterSearch = value;
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
    /// A value of Select1.
    /// </summary>
    [JsonPropertyName("select1")]
    public Program Select1
    {
      get => select1 ??= new();
      set => select1 = value;
    }

    /// <summary>
    /// A value of SelectCounty.
    /// </summary>
    [JsonPropertyName("selectCounty")]
    public CseOrganization SelectCounty
    {
      get => selectCounty ??= new();
      set => selectCounty = value;
    }

    /// <summary>
    /// A value of PrevCommand.
    /// </summary>
    [JsonPropertyName("prevCommand")]
    public Common PrevCommand
    {
      get => prevCommand ??= new();
      set => prevCommand = value;
    }

    /// <summary>
    /// Gets a value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public SelectGroup Select
    {
      get => select ?? (select = new());
      set => select = value;
    }

    /// <summary>
    /// Gets a value of Cdvl.
    /// </summary>
    [JsonPropertyName("cdvl")]
    public CdvlGroup Cdvl
    {
      get => cdvl ?? (cdvl = new());
      set => cdvl = value;
    }

    /// <summary>
    /// A value of PromptOffice.
    /// </summary>
    [JsonPropertyName("promptOffice")]
    public Common PromptOffice
    {
      get => promptOffice ??= new();
      set => promptOffice = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
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

    private Office hiddenSaved;
    private Office hiddenSelect;
    private FilterSearchGroup filterSearch;
    private DateWorkArea dateWorkArea;
    private Program select1;
    private CseOrganization selectCounty;
    private Common prevCommand;
    private SelectGroup select;
    private CdvlGroup cdvl;
    private Common promptOffice;
    private Array<ListGroup> list;
    private NextTranInfo hidden;
    private Standard standard;
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
    /// A value of DeleteFlag.
    /// </summary>
    [JsonPropertyName("deleteFlag")]
    public Common DeleteFlag
    {
      get => deleteFlag ??= new();
      set => deleteFlag = value;
    }

    /// <summary>
    /// A value of PrevCommand.
    /// </summary>
    [JsonPropertyName("prevCommand")]
    public Common PrevCommand
    {
      get => prevCommand ??= new();
      set => prevCommand = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Count1.
    /// </summary>
    [JsonPropertyName("count1")]
    public Common Count1
    {
      get => count1 ??= new();
      set => count1 = value;
    }

    /// <summary>
    /// A value of Count2.
    /// </summary>
    [JsonPropertyName("count2")]
    public Common Count2
    {
      get => count2 ??= new();
      set => count2 = value;
    }

    /// <summary>
    /// A value of Count3.
    /// </summary>
    [JsonPropertyName("count3")]
    public Common Count3
    {
      get => count3 ??= new();
      set => count3 = value;
    }

    private DateWorkArea current;
    private Common deleteFlag;
    private Common prevCommand;
    private DateWorkArea dateWorkArea;
    private DateWorkArea null1;
    private Common count1;
    private Common count2;
    private Common count3;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    private Office office;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private CseOrganization cseOrganization;
    private Program program;
    private CountyService countyService;
  }
#endregion
}

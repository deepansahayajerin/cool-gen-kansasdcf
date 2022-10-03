// Program: OE_FACL_KS_CORRECTIONAL_FACILITY, ID: 374582417, model: 746.
// Short name: SWEFACLP
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
/// A program: OE_FACL_KS_CORRECTIONAL_FACILITY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeFaclKsCorrectionalFacility: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FACL_KS_CORRECTIONAL_FACILITY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFaclKsCorrectionalFacility(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFaclKsCorrectionalFacility.
  /// </summary>
  public OeFaclKsCorrectionalFacility(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // Date		Developer	Description
    // 02/03/96 	Sid Chowdhary	Initial Code
    // 11/08/96                 R. Marchman   	Add new security and next tran.
    // 11/13/00 	SWSRPRM	PR # 107375 - Added Stockton facility
    // 03/04/2002             Vithal Madhira                   PR# 139194
    // Changed ' Area codes'  for following facilities:
    // Ellsworth Corretiona Facility            785
    // Hutchinson Correctional Facility         620
    // Larned Correctional Mental Health        620
    // Norton Correctional Facility             785
    // Winfield Correctional Facility           620
    // Labette Corr Conservation Camp           620
    // Labette Women's Corr Camp                620.
    // ------------------------------------------------------------------------
    // --------------------------------------------------------------------------
    // 04/04/2002          Vithal Madhira           PR# 140102,140103
    // Redesigned the screen to implement following changes:
    // 1. Change the name of screen from 'Kansas Correctional Facility' to '
    // Correctional Facilities'
    // 2. Add  new PF keys PF4 (List), PF5 (Add), PF6 (Update), PF10 (Delete), 
    // PF11 (Clear) and add code implement the functionality on the screen.
    // 3. Validate the 'Security' for the above functions.
    // 4. Add two filters 'City', 'State' to use while displaying the records. '
    // State' will be defaulted to 'KS' initially.
    // --------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------
    // 10/17/2003          Bonnie Lee               PR# 189075
    // Added the command of DISPLAY to security check.
    // -----------------------------------------------------------------------------------------------
    // 08/20/10  RMathews  CQ553  Add ability to NEXT tran to FACL screen and 
    // PF3 to exit
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // **** end   group A ****
    export.Work.SelectChar = import.Work.SelectChar;
    export.H.Number = import.H.Number;
    MoveJailAddresses2(import.Filter, export.Filter);
    export.FilterStatePrompt.Flag = import.FilterStatePrompt.Flag;
    MoveJailAddresses2(import.FilterHidden, export.FilterHidden);
    export.ScrollIndicator.Text9 = import.ScrollIndicator.Text9;
    export.HiddenInitialIndex.Count = import.HiddenInitialIndex.Count;
    export.HiddenFinalIndex.Count = import.HiddenFinalIndex.Count;
    export.HiddenGroupFull.Flag = import.HiddenGroupFull.Flag;
    export.HiddenCode.CodeName = import.HiddenCode.CodeName;
    export.HiddenCodeValue.Cdvalue = import.HiddenCodeValue.Cdvalue;

    for(import.Import1.Index = 0; import.Import1.Index < Import
      .ImportGroup.Capacity; ++import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.SelChar.SelectChar =
        import.Import1.Item.SelChar.SelectChar;
      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Update.DetailHidden.
        Assign(import.Import1.Item.DetailHidden);
      export.Export1.Update.DetailStatePrompt.Flag =
        import.Import1.Item.DetailStatePrompt.Flag;
    }

    import.Import1.CheckIndex();

    for(import.Import2Hidden.Index = 0; import.Import2Hidden.Index < import
      .Import2Hidden.Count; ++import.Import2Hidden.Index)
    {
      if (!import.Import2Hidden.CheckSize())
      {
        break;
      }

      export.Export2Hidden.Index = import.Import2Hidden.Index;
      export.Export2Hidden.CheckSize();

      export.Export2Hidden.Update.Export2HiddenDetail.Assign(
        import.Import2Hidden.Item.Import2HiddenDetail);
    }

    import.Import2Hidden.CheckIndex();

    if (Equal(global.Command, "NEXT1"))
    {
      global.Command = "NEXT";
    }

    // **** begin group B ****
    export.H.Number = import.H.Number;

    if (!import.Imp.IsEmpty)
    {
      for(import.Imp.Index = 0; import.Imp.Index < import.Imp.Count; ++
        import.Imp.Index)
      {
        if (!import.Imp.CheckSize())
        {
          break;
        }

        export.Exp.Index = import.Imp.Index;
        export.Exp.CheckSize();

        export.Exp.Update.SelectChar.SelectChar =
          import.Imp.Item.SectChar.SelectChar;
        export.Exp.Update.Datail.Assign(import.Imp.Item.Deatail);
        export.Exp.Update.Deatail.Assign(import.Imp.Item.Detail);
      }

      import.Imp.CheckIndex();
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      export.HiddenNextTranInfo.CsePersonNumber = import.H.Number;
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

    // CQ553  Allow NEXT tran into FACL screen
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.H.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
        global.Command = "DISPLAY";
      }
    }

    // **** end   group B ****
    if (Equal(global.Command, "DISPPAGE"))
    {
      global.Command = "DISPLAY";
    }

    // -----------------------------------------------------------------------------------
    // 08/20/10  RMathews  Added PF3 to allow exit when screen entered through 
    // NEXT tran.
    // -----------------------------------------------------------------------------------
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // -------------------------------------------------------------------------------
    // Per PR# 140102, 140103  check the security  if COMMAND is 'ADD', 'UPDATE'
    // or 'DELETE'.
    // ------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------
    // 10/17/2003          Bonnie Lee               PR# 189075
    // Added the command of DISPLAY to security check.
    // -----------------------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "";
      }
    }

    // -------------------------------------------------------------------------------
    // Scrolling is invalid with a selection on the screen.
    // ------------------------------------------------------------------------------
    if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
        {
          ExitState = "OE0000_NO_SCROLLING_WITH_SELECT";
          global.Command = "";
        }
      }

      export.Export1.CheckIndex();
    }

    // -------------------------------------------------------------------------------
    //  PR# 140102,140103:  new edits for  'ADD' ,  'UPDATE' and  'DELETE'  .
    // ------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      local.CountSelChar.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Export1.Item.SelChar.SelectChar))
        {
          case 'S':
            ++local.CountSelChar.Count;

            break;
          case ' ':
            continue;
          default:
            var field = GetField(export.Export1.Item.SelChar, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            continue;
        }
      }

      export.Export1.CheckIndex();

      if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
      {
        goto Test1;
      }

      if (local.CountSelChar.Count == 0)
      {
        // ---------------------------------------------------------------
        //          No Selection Made. Display error message.
        // ---------------------------------------------------------------
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";
      }
      else if (local.CountSelChar.Count > 1)
      {
        if (Equal(global.Command, "DELETE"))
        {
          for(export.Export1.Index = 0; export.Export1.Index < Export
            .ExportGroup.Capacity; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
            {
              if (export.Export1.Item.DetailHidden.Identifier == 0)
              {
                var field = GetField(export.Export1.Item.SelChar, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
              }
            }
          }

          export.Export1.CheckIndex();

          goto Test1;
        }

        // ---------------------------------------------------------------
        //       More than one record selected. Display error message.
        // ---------------------------------------------------------------
        ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            var field = GetField(export.Export1.Item.SelChar, "selectChar");

            field.Error = true;
          }
        }

        export.Export1.CheckIndex();
      }
      else if (local.CountSelChar.Count == 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            if (Equal(global.Command, "UPDATE"))
            {
              if (export.Export1.Item.Detail.Identifier == 0 && IsEmpty
                (export.Export1.Item.Detail.JailName) && IsEmpty
                (export.Export1.Item.Detail.Street1) && IsEmpty
                (export.Export1.Item.Detail.Street2) && IsEmpty
                (export.Export1.Item.Detail.City) && IsEmpty
                (export.Export1.Item.Detail.State) && export
                .Export1.Item.Detail.PhoneAreaCode.GetValueOrDefault() == 0 && export
                .Export1.Item.Detail.Phone.GetValueOrDefault() == 0 && IsEmpty
                (export.Export1.Item.Detail.PhoneExtension))
              {
                ExitState = "OE0000_CAN_NOT_PROCESS_BLANK_ROW";

                goto Test1;
              }

              if (IsEmpty(export.Export1.Item.Detail.JailName) && IsEmpty
                (export.Export1.Item.Detail.Street1) && IsEmpty
                (export.Export1.Item.Detail.City) && IsEmpty
                (export.Export1.Item.Detail.State) && IsEmpty
                (export.Export1.Item.Detail.ZipCode5))
              {
                var field1 = GetField(export.Export1.Item.Detail, "jailName");

                field1.Error = true;

                var field2 = GetField(export.Export1.Item.Detail, "street1");

                field2.Error = true;

                var field3 = GetField(export.Export1.Item.Detail, "city");

                field3.Error = true;

                var field4 = GetField(export.Export1.Item.Detail, "state");

                field4.Error = true;

                var field5 = GetField(export.Export1.Item.Detail, "zipCode5");

                field5.Error = true;

                ExitState = "OE0014_MANDATORY_FIELD_MISSING";

                goto Test1;
              }

              if (Equal(export.Export1.Item.Detail.JailName,
                export.Export1.Item.DetailHidden.JailName) && Equal
                (export.Export1.Item.Detail.Street1,
                export.Export1.Item.DetailHidden.Street1) && Equal
                (export.Export1.Item.Detail.Street2,
                export.Export1.Item.DetailHidden.Street2) && Equal
                (export.Export1.Item.Detail.City,
                export.Export1.Item.DetailHidden.City) && Equal
                (export.Export1.Item.Detail.State,
                export.Export1.Item.DetailHidden.State) && Equal
                (export.Export1.Item.Detail.ZipCode5,
                export.Export1.Item.DetailHidden.ZipCode5) && Equal
                (export.Export1.Item.Detail.ZipCode4,
                export.Export1.Item.DetailHidden.ZipCode4) && export
                .Export1.Item.Detail.PhoneAreaCode.GetValueOrDefault() == export
                .Export1.Item.DetailHidden.PhoneAreaCode.GetValueOrDefault() &&
                export.Export1.Item.Detail.Phone.GetValueOrDefault() == export
                .Export1.Item.DetailHidden.Phone.GetValueOrDefault() && Equal
                (export.Export1.Item.Detail.PhoneExtension,
                export.Export1.Item.DetailHidden.PhoneExtension))
              {
                var field = GetField(export.Export1.Item.SelChar, "selectChar");

                field.Error = true;

                ExitState = "FN0000_NO_CHANGE_TO_UPDATE";

                goto Test1;
              }

              if (export.Export1.Item.DetailHidden.Identifier == 0)
              {
                var field = GetField(export.Export1.Item.SelChar, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_ADD_THE_RECORD_FIRST";

                goto Test1;
              }
            }

            if (Equal(global.Command, "DELETE"))
            {
              if (export.Export1.Item.Detail.Identifier == 0 && IsEmpty
                (export.Export1.Item.Detail.JailName) && IsEmpty
                (export.Export1.Item.Detail.Street1) && IsEmpty
                (export.Export1.Item.Detail.Street2) && IsEmpty
                (export.Export1.Item.Detail.City) && IsEmpty
                (export.Export1.Item.Detail.State) && export
                .Export1.Item.Detail.PhoneAreaCode.GetValueOrDefault() == 0 && export
                .Export1.Item.Detail.Phone.GetValueOrDefault() == 0 && IsEmpty
                (export.Export1.Item.Detail.PhoneExtension))
              {
                ExitState = "OE0000_CAN_NOT_PROCESS_BLANK_ROW";

                goto Test1;
              }

              if (export.Export1.Item.DetailHidden.Identifier == 0)
              {
                var field = GetField(export.Export1.Item.SelChar, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_ADD_THE_RECORD_FIRST";

                goto Test1;
              }
            }

            if (Equal(global.Command, "ADD"))
            {
              if (export.Export1.Item.DetailHidden.Identifier > 0)
              {
                var field = GetField(export.Export1.Item.SelChar, "selectChar");

                field.Error = true;

                ExitState = "OE0000_RECORD_ALREADY_ADDED";

                goto Test1;
              }

              if (IsEmpty(export.Export1.Item.Detail.JailName) && IsEmpty
                (export.Export1.Item.Detail.Street1) && IsEmpty
                (export.Export1.Item.Detail.City) && IsEmpty
                (export.Export1.Item.Detail.State) && IsEmpty
                (export.Export1.Item.Detail.ZipCode5))
              {
                var field1 = GetField(export.Export1.Item.Detail, "jailName");

                field1.Error = true;

                var field2 = GetField(export.Export1.Item.Detail, "street1");

                field2.Error = true;

                var field3 = GetField(export.Export1.Item.Detail, "city");

                field3.Error = true;

                var field4 = GetField(export.Export1.Item.Detail, "state");

                field4.Error = true;

                var field5 = GetField(export.Export1.Item.Detail, "zipCode5");

                field5.Error = true;

                ExitState = "OE0014_MANDATORY_FIELD_MISSING";

                goto Test1;
              }
            }

            if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
            {
              if (IsEmpty(export.Export1.Item.Detail.JailName))
              {
                var field = GetField(export.Export1.Item.Detail, "jailName");

                field.Error = true;

                ExitState = "OE0000_JAIL_NAME_MANDATORY";

                goto Test1;
              }

              if (IsEmpty(export.Export1.Item.Detail.Street1))
              {
                var field = GetField(export.Export1.Item.Detail, "street1");

                field.Error = true;

                ExitState = "OE0000_STREET1_MANDATORY";

                goto Test1;
              }

              if (IsEmpty(export.Export1.Item.Detail.City))
              {
                var field = GetField(export.Export1.Item.Detail, "city");

                field.Error = true;

                ExitState = "OE0000_CITY_MANDATORY";

                goto Test1;
              }

              if (IsEmpty(export.Export1.Item.Detail.State))
              {
                var field = GetField(export.Export1.Item.Detail, "state");

                field.Error = true;

                ExitState = "OE0000_STATE_MANDATORY";

                goto Test1;
              }

              // -----------------------------------------------
              //      Validate  State code value.
              // ----------------------------------------------
              if (!IsEmpty(export.Export1.Item.Detail.State))
              {
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue = export.Export1.Item.Detail.State ?? Spaces
                  (10);
                UseCabValidateCodeValue();

                if (AsChar(local.Rtn.Flag) != 'Y')
                {
                  var field = GetField(export.Export1.Item.Detail, "state");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_STATE_CODE";

                  goto Test1;
                }
              }

              if (IsEmpty(export.Export1.Item.Detail.ZipCode5))
              {
                var field = GetField(export.Export1.Item.Detail, "zipCode5");

                field.Error = true;

                ExitState = "OE0000_ZIPCODE5_MANDATORY";

                goto Test1;
              }

              // -----------------------------------------------
              //      Validate  Zip Code5. This must be numeric and must have 
              // five digits.
              // ----------------------------------------------
              if (Length(TrimEnd(export.Export1.Item.Detail.ZipCode5)) > 0 && Length
                (TrimEnd(export.Export1.Item.Detail.ZipCode5)) < 5)
              {
                var field = GetField(export.Export1.Item.Detail, "zipCode5");

                field.Error = true;

                ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

                goto Test1;
              }

              if (Length(TrimEnd(export.Export1.Item.Detail.ZipCode5)) > 0 && Verify
                (export.Export1.Item.Detail.ZipCode5, "0123456789") != 0)
              {
                var field = GetField(export.Export1.Item.Detail, "zipCode5");

                field.Error = true;

                ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                goto Test1;
              }

              // -----------------------------------------------
              //      Validate  Zip Code4. This must be numeric and must have 
              // five digits.
              // ----------------------------------------------
              if (Length(TrimEnd(export.Export1.Item.Detail.ZipCode4)) > 0 && Length
                (TrimEnd(export.Export1.Item.Detail.ZipCode4)) < 4)
              {
                var field = GetField(export.Export1.Item.Detail, "zipCode4");

                field.Error = true;

                ExitState = "OE0000_ZIPCODE4_MUST_BE_4_DIGITS";

                goto Test1;
              }

              if (Length(TrimEnd(export.Export1.Item.Detail.ZipCode4)) > 0 && Verify
                (export.Export1.Item.Detail.ZipCode4, "0123456789") != 0)
              {
                var field = GetField(export.Export1.Item.Detail, "zipCode4");

                field.Error = true;

                ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

                goto Test1;
              }

              // -----------------------------------------------
              //      Validate Phone Area Code. This must be numeric and must 
              // have 3 digits.
              // ----------------------------------------------
              if (export.Export1.Item.Detail.PhoneAreaCode.
                GetValueOrDefault() > 0 && export
                .Export1.Item.Detail.PhoneAreaCode.GetValueOrDefault() < 100)
              {
                var field =
                  GetField(export.Export1.Item.Detail, "phoneAreaCode");

                field.Error = true;

                ExitState = "OE0000_PHONE_AREA_CODE_ERROR";

                goto Test1;
              }

              if (export.Export1.Item.Detail.Phone.GetValueOrDefault() > 0 && export
                .Export1.Item.Detail.Phone.GetValueOrDefault() < 1000000)
              {
                var field = GetField(export.Export1.Item.Detail, "phone");

                field.Error = true;

                ExitState = "OE0000_PHONE_ERROR";

                goto Test1;
              }

              if (export.Export1.Item.Detail.Phone.GetValueOrDefault() > 0 && export
                .Export1.Item.Detail.PhoneAreaCode.GetValueOrDefault() == 0)
              {
                var field =
                  GetField(export.Export1.Item.Detail, "phoneAreaCode");

                field.Error = true;

                ExitState = "OE0000_PHONE_AREA_REQD";

                goto Test1;
              }
            }
          }
        }

        export.Export1.CheckIndex();
      }
    }

Test1:

    switch(TrimEnd(global.Command))
    {
      case "RTLIST":
        if (AsChar(export.FilterStatePrompt.Flag) == 'S')
        {
          export.FilterStatePrompt.Flag = "+";

          var field = GetField(export.Filter, "state");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(export.HiddenCodeValue.Cdvalue))
          {
            export.Filter.State = export.HiddenCodeValue.Cdvalue;
          }

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.DetailStatePrompt.Flag) == 'S')
            {
              export.Export1.Update.DetailStatePrompt.Flag = "+";

              var field = GetField(export.Export1.Item.Detail, "state");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(export.HiddenCodeValue.Cdvalue))
              {
                export.Export1.Update.Detail.State =
                  export.HiddenCodeValue.Cdvalue;
              }

              goto Test2;
            }
          }
        }

        export.Export1.CheckIndex();

        break;
      case "LIST":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailStatePrompt.Flag) == 'S' && AsChar
            (export.FilterStatePrompt.Flag) == 'S')
          {
            var field1 = GetField(export.FilterStatePrompt, "flag");

            field1.Error = true;

            var field2 =
              GetField(export.Export1.Item.DetailStatePrompt, "flag");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            goto Test2;
          }
        }

        export.Export1.CheckIndex();

        if (AsChar(export.FilterStatePrompt.Flag) == 'S')
        {
          export.HiddenCode.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_CODE_TABLES";

          return;
        }
        else if (AsChar(export.FilterStatePrompt.Flag) == '+')
        {
        }
        else
        {
          var field = GetField(export.FilterStatePrompt, "flag");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
        }

        local.CountSelChar.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            ++local.CountSelChar.Count;
          }
        }

        export.Export1.CheckIndex();

        if (local.CountSelChar.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.DetailStatePrompt.Flag) == 'S')
            {
              export.HiddenCode.CodeName = "STATE CODE";
              ExitState = "ECO_LNK_TO_CODE_TABLES";

              return;
            }
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else if (local.CountSelChar.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
            {
              var field = GetField(export.Export1.Item.SelChar, "selectChar");

              field.Error = true;
            }
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";
        }
        else if (local.CountSelChar.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            var field = GetField(export.Export1.Item.SelChar, "selectChar");

            field.Error = true;
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "CLEAR":
        MoveJailAddresses2(local.Blank, export.Filter);
        export.FilterStatePrompt.Flag = "+";

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.SelChar.SelectChar = "";
          MoveJailAddresses1(local.Blank, export.Export1.Update.Detail);
          MoveJailAddresses1(local.Blank, export.Export1.Update.DetailHidden);
          export.Export1.Update.DetailStatePrompt.Flag = "+";
        }

        export.Export1.CheckIndex();

        break;
      case "DELETE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            UseOeFaclDeleteJailAddress();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.SelChar.SelectChar = "*";
            }
            else
            {
              var field = GetField(export.Export1.Item.SelChar, "selectChar");

              field.Error = true;

              break;
            }
          }
        }

        export.Export1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      case "UPDATE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            UseOeFaclUpdateJailAddresses();
          }
        }

        export.Export1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
            {
              export.Export1.Update.SelChar.SelectChar = "*";
              export.Export1.Update.DetailHidden.Assign(
                export.Export1.Item.Detail);
            }
          }

          export.Export1.CheckIndex();
        }

        break;
      case "ADD":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            UseOeFaclCreateJailAddress();
          }
        }

        export.Export1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
            {
              export.Export1.Update.SelChar.SelectChar = "*";
              export.Export1.Update.DetailHidden.Assign(
                export.Export1.Item.Detail);
            }
          }

          export.Export1.CheckIndex();
        }

        break;
      case "DISPLAY":
        export.FilterStatePrompt.Flag = "+";

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailStatePrompt.Flag = "+";

          if (!IsEmpty(export.Export1.Item.SelChar.SelectChar))
          {
            export.Export1.Update.SelChar.SelectChar = "";
          }
        }

        export.Export1.CheckIndex();

        if (IsEmpty(export.Filter.State))
        {
          export.Filter.State = "KS";
        }

        if (!IsEmpty(export.Filter.State))
        {
          // -----------------------------------------------
          //      Validate  State code value.
          // ----------------------------------------------
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = export.Filter.State ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.Rtn.Flag) != 'Y')
          {
            var field = GetField(export.Filter, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            break;
          }
        }

        if (!Equal(export.Filter.City, export.FilterHidden.City) || !
          Equal(export.Filter.State, export.FilterHidden.State))
        {
          // -----------------------------------------------------------------------------
          // One of the filters changed. Read the database to populate the 
          // export view based on new filter values.
          // -----------------------------------------------------------------------------
          for(export.Export2Hidden.Index = 0; export.Export2Hidden.Index < export
            .Export2Hidden.Count; ++export.Export2Hidden.Index)
          {
            if (!export.Export2Hidden.CheckSize())
            {
              break;
            }

            export.Export2Hidden.Update.Export2HiddenDetail.Assign(local.Blank);
          }

          export.Export2Hidden.CheckIndex();
          export.Export2Hidden.Index = -1;
          export.Export2Hidden.Count = 0;
          UseOeFaclReadJailAddresses();

          // -----------------------------------------------------------------------------
          // Clear the group view which display data on the screen. Need to 
          // display new data.
          // -----------------------------------------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            MoveJailAddresses1(local.Blank, export.Export1.Update.Detail);
            MoveJailAddresses1(local.Blank, export.Export1.Update.DetailHidden);
          }

          export.Export1.CheckIndex();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (export.Export2Hidden.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
              export.HiddenInitialIndex.Count = 0;
              export.HiddenFinalIndex.Count = 0;

              break;
            }
            else
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            }
          }

          export.Export1.Index = -1;
          export.Export1.Count = 0;
          export.HiddenInitialIndex.Count = 0;
          export.HiddenFinalIndex.Count = 0;

          for(export.Export2Hidden.Index = 0; export.Export2Hidden.Index < Export
            .Export2HiddenGroup.Capacity; ++export.Export2Hidden.Index)
          {
            if (!export.Export2Hidden.CheckSize())
            {
              break;
            }

            export.Export1.Index = export.Export2Hidden.Index;
            export.Export1.CheckSize();

            MoveJailAddresses1(export.Export2Hidden.Item.Export2HiddenDetail,
              export.Export1.Update.Detail);
            MoveJailAddresses1(export.Export2Hidden.Item.Export2HiddenDetail,
              export.Export1.Update.DetailHidden);

            if (export.Export1.Count >= Export.ExportGroup.Capacity)
            {
              break;
            }
          }

          export.Export2Hidden.CheckIndex();
          export.HiddenInitialIndex.Count = 1;
          export.HiddenFinalIndex.Count = export.Export1.Count;
        }

        break;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "RETURN":
        local.SelectedCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.SelChar.SelectChar) == 'S')
          {
            ++local.SelectedCount.Count;

            if (local.SelectedCount.Count > 1)
            {
              ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";

              goto Test2;
            }

            export.SelectedIncarceration.InstitutionName =
              export.Export1.Item.Detail.JailName ?? "";
            export.SelectedIncarceration.PhoneAreaCode =
              export.Export1.Item.Detail.PhoneAreaCode.GetValueOrDefault();
            export.SelectedIncarceration.Phone =
              export.Export1.Item.Detail.Phone.GetValueOrDefault();
            export.SelectedIncarceration.PhoneExt =
              export.Export1.Item.Detail.PhoneExtension ?? "";
            export.SelectedIncarcerationAddress.Street1 =
              export.Export1.Item.Detail.Street1 ?? "";
            export.SelectedIncarcerationAddress.Street2 =
              export.Export1.Item.Detail.Street2 ?? "";
            export.SelectedIncarcerationAddress.City =
              export.Export1.Item.Detail.City ?? "";
            export.SelectedIncarcerationAddress.State =
              export.Export1.Item.Detail.State ?? "";
            export.SelectedIncarcerationAddress.ZipCode5 =
              export.Export1.Item.Detail.ZipCode5 ?? "";
            export.SelectedIncarcerationAddress.ZipCode4 =
              export.Export1.Item.Detail.ZipCode4 ?? "";
            export.SelectedIncarcerationAddress.ZipCode4 =
              export.Export1.Item.Detail.ZipCode4 ?? "";
          }
          else if (!IsEmpty(export.Export1.Item.SelChar.SelectChar))
          {
            var field = GetField(export.Export1.Item.SelChar, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            goto Test2;
          }
        }

        export.Export1.CheckIndex();
        global.Command = "DISPPAGE";
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        if (export.HiddenFinalIndex.Count == export.Export2Hidden.Count || export
          .Export2Hidden.Count <= 4)
        {
          if (AsChar(export.HiddenGroupFull.Flag) == 'Y')
          {
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_FORWARD";
          }
        }
        else
        {
          // -------------------------------------------------------------
          // The following code will set the 'Scroll Indicator' values.
          // -------------------------------------------------------------
          export.HiddenInitialIndex.Count = export.HiddenFinalIndex.Count + 1;
          export.HiddenFinalIndex.Count = export.HiddenInitialIndex.Count + (
            export.Export1.Count - 1);

          if (export.HiddenFinalIndex.Count > export.Export2Hidden.Count)
          {
            export.HiddenFinalIndex.Count = export.Export2Hidden.Count;
          }

          // -------------------------------------------------------------
          // Clear the screen before populating the data.
          // -------------------------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < Export
            .ExportGroup.Capacity; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            MoveJailAddresses1(local.Blank, export.Export1.Update.Detail);
            MoveJailAddresses1(local.Blank, export.Export1.Update.DetailHidden);
            export.Export1.Update.SelChar.SelectChar = "";
          }

          export.Export1.CheckIndex();
          export.Export1.Index = -1;
          export.Export2Hidden.Index = export.HiddenInitialIndex.Count - 1;

          for(var limit = export.HiddenFinalIndex.Count; export
            .Export2Hidden.Index < limit; ++export.Export2Hidden.Index)
          {
            if (!export.Export2Hidden.CheckSize())
            {
              break;
            }

            ++export.Export1.Index;
            export.Export1.CheckSize();

            MoveJailAddresses1(export.Export2Hidden.Item.Export2HiddenDetail,
              export.Export1.Update.Detail);
            MoveJailAddresses1(export.Export2Hidden.Item.Export2HiddenDetail,
              export.Export1.Update.DetailHidden);
          }

          export.Export2Hidden.CheckIndex();
        }

        break;
      case "PREV":
        if (export.HiddenInitialIndex.Count <= 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }
        else
        {
          // -------------------------------------------------------------
          // The following code will set the 'Scroll Indicator' values.
          // -------------------------------------------------------------
          export.HiddenFinalIndex.Count = export.HiddenInitialIndex.Count - 1;
          export.HiddenInitialIndex.Count = export.HiddenFinalIndex.Count - 3;

          if (export.HiddenInitialIndex.Count == 0)
          {
            export.HiddenInitialIndex.Count = 1;
          }

          // -------------------------------------------------------------
          // Clear the screen before populating the data.
          // -------------------------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < Export
            .ExportGroup.Capacity; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            MoveJailAddresses1(local.Blank, export.Export1.Update.Detail);
            MoveJailAddresses1(local.Blank, export.Export1.Update.DetailHidden);
            export.Export1.Update.SelChar.SelectChar = "";
          }

          export.Export1.CheckIndex();
          export.Export1.Index = -1;
          export.Export2Hidden.Index = export.HiddenInitialIndex.Count - 1;

          for(var limit = export.HiddenFinalIndex.Count; export
            .Export2Hidden.Index < limit; ++export.Export2Hidden.Index)
          {
            if (!export.Export2Hidden.CheckSize())
            {
              break;
            }

            ++export.Export1.Index;
            export.Export1.CheckSize();

            MoveJailAddresses1(export.Export2Hidden.Item.Export2HiddenDetail,
              export.Export1.Update.Detail);
            MoveJailAddresses1(export.Export2Hidden.Item.Export2HiddenDetail,
              export.Export1.Update.DetailHidden);
          }

          export.Export2Hidden.CheckIndex();
        }

        break;
      case "SIGNOFF":
        // **** begin group D ****
        UseScCabSignoff();

        return;

        // **** end   group D ****
        break;
      case "":
        // **** begin group C ****
        // -------------------------------------------------------------
        // the command will only be spaces if  you are returning from a
        // procedure to the menu because of a security violation.
        // -------------------------------------------------------------
        // **** end   group C ****
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test2:

    // -------------------------------------------------------------
    // The following code will set the 'Scroll Indicator' values.
    // -------------------------------------------------------------
    if (export.Export2Hidden.Count <= 4)
    {
      export.ScrollIndicator.Text9 = "More:";
    }
    else if (export.Export2Hidden.Count > 8 && export.HiddenFinalIndex.Count > 4
      && export.HiddenFinalIndex.Count < export.Export2Hidden.Count)
    {
      export.ScrollIndicator.Text9 = "More: - +";
    }
    else if (export.Export2Hidden.Count > 4 && export
      .HiddenFinalIndex.Count <= 4)
    {
      export.ScrollIndicator.Text9 = "More:   +";
    }
    else if (export.Export2Hidden.Count > 4 && export
      .HiddenFinalIndex.Count == export.Export2Hidden.Count)
    {
      export.ScrollIndicator.Text9 = "More: -";
    }
  }

  private static void MoveExport2(OeFaclReadJailAddresses.Export.
    Export2Group source, Export.Export2HiddenGroup target)
  {
    target.Export2HiddenDetail.Assign(source.Export2Detail);
  }

  private static void MoveJailAddresses1(JailAddresses source,
    JailAddresses target)
  {
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.JailName = source.JailName;
    target.Phone = source.Phone;
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExtension = source.PhoneExtension;
  }

  private static void MoveJailAddresses2(JailAddresses source,
    JailAddresses target)
  {
    target.City = source.City;
    target.State = source.State;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Rtn.Flag = useExport.ValidCode.Flag;
  }

  private void UseOeFaclCreateJailAddress()
  {
    var useImport = new OeFaclCreateJailAddress.Import();
    var useExport = new OeFaclCreateJailAddress.Export();

    MoveJailAddresses1(export.Export1.Item.Detail, useImport.JailAddresses);

    Call(OeFaclCreateJailAddress.Execute, useImport, useExport);

    MoveJailAddresses1(useExport.JailAddresses, export.Export1.Update.Detail);
  }

  private void UseOeFaclDeleteJailAddress()
  {
    var useImport = new OeFaclDeleteJailAddress.Import();
    var useExport = new OeFaclDeleteJailAddress.Export();

    MoveJailAddresses1(export.Export1.Item.Detail, useImport.JailAddresses);

    Call(OeFaclDeleteJailAddress.Execute, useImport, useExport);
  }

  private void UseOeFaclReadJailAddresses()
  {
    var useImport = new OeFaclReadJailAddresses.Import();
    var useExport = new OeFaclReadJailAddresses.Export();

    MoveJailAddresses2(export.Filter, useImport.Filter);

    Call(OeFaclReadJailAddresses.Execute, useImport, useExport);

    useExport.Export2.CopyTo(export.Export2Hidden, MoveExport2);
    export.HiddenGroupFull.Flag = useExport.HiddenGroupFull.Flag;
  }

  private void UseOeFaclUpdateJailAddresses()
  {
    var useImport = new OeFaclUpdateJailAddresses.Import();
    var useExport = new OeFaclUpdateJailAddresses.Export();

    MoveJailAddresses1(export.Export1.Item.Detail, useImport.JailAddresses);

    Call(OeFaclUpdateJailAddresses.Execute, useImport, useExport);
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
    /// <summary>A ImpGroup group.</summary>
    [Serializable]
    public class ImpGroup
    {
      /// <summary>
      /// A value of SectChar.
      /// </summary>
      [JsonPropertyName("sectChar")]
      public Common SectChar
      {
        get => sectChar ??= new();
        set => sectChar = value;
      }

      /// <summary>
      /// A value of Deatail.
      /// </summary>
      [JsonPropertyName("deatail")]
      public Incarceration Deatail
      {
        get => deatail ??= new();
        set => deatail = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public IncarcerationAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Common sectChar;
      private Incarceration deatail;
      private IncarcerationAddress detail;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public JailAddresses DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
      }

      /// <summary>
      /// A value of SelChar.
      /// </summary>
      [JsonPropertyName("selChar")]
      public Common SelChar
      {
        get => selChar ??= new();
        set => selChar = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public JailAddresses Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailStatePrompt.
      /// </summary>
      [JsonPropertyName("detailStatePrompt")]
      public Common DetailStatePrompt
      {
        get => detailStatePrompt ??= new();
        set => detailStatePrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private JailAddresses detailHidden;
      private Common selChar;
      private JailAddresses detail;
      private Common detailStatePrompt;
    }

    /// <summary>A Import2HiddenGroup group.</summary>
    [Serializable]
    public class Import2HiddenGroup
    {
      /// <summary>
      /// A value of Import2HiddenDetail.
      /// </summary>
      [JsonPropertyName("import2HiddenDetail")]
      public JailAddresses Import2HiddenDetail
      {
        get => import2HiddenDetail ??= new();
        set => import2HiddenDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private JailAddresses import2HiddenDetail;
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
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public CsePerson H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// Gets a value of Imp.
    /// </summary>
    [JsonIgnore]
    public Array<ImpGroup> Imp => imp ??= new(ImpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Imp for json serialization.
    /// </summary>
    [JsonPropertyName("imp")]
    [Computed]
    public IList<ImpGroup> Imp_Json
    {
      get => imp;
      set => Imp.Assign(value);
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public JailAddresses Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of FilterStatePrompt.
    /// </summary>
    [JsonPropertyName("filterStatePrompt")]
    public Common FilterStatePrompt
    {
      get => filterStatePrompt ??= new();
      set => filterStatePrompt = value;
    }

    /// <summary>
    /// A value of FilterHidden.
    /// </summary>
    [JsonPropertyName("filterHidden")]
    public JailAddresses FilterHidden
    {
      get => filterHidden ??= new();
      set => filterHidden = value;
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
    /// Gets a value of Import2Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<Import2HiddenGroup> Import2Hidden => import2Hidden ??= new(
      Import2HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import2Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("import2Hidden")]
    [Computed]
    public IList<Import2HiddenGroup> Import2Hidden_Json
    {
      get => import2Hidden;
      set => Import2Hidden.Assign(value);
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
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of HiddenInitialIndex.
    /// </summary>
    [JsonPropertyName("hiddenInitialIndex")]
    public Common HiddenInitialIndex
    {
      get => hiddenInitialIndex ??= new();
      set => hiddenInitialIndex = value;
    }

    /// <summary>
    /// A value of HiddenFinalIndex.
    /// </summary>
    [JsonPropertyName("hiddenFinalIndex")]
    public Common HiddenFinalIndex
    {
      get => hiddenFinalIndex ??= new();
      set => hiddenFinalIndex = value;
    }

    /// <summary>
    /// A value of HiddenGroupFull.
    /// </summary>
    [JsonPropertyName("hiddenGroupFull")]
    public Common HiddenGroupFull
    {
      get => hiddenGroupFull ??= new();
      set => hiddenGroupFull = value;
    }

    private Standard standard;
    private CsePerson h;
    private Common work;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ImpGroup> imp;
    private JailAddresses filter;
    private Common filterStatePrompt;
    private JailAddresses filterHidden;
    private Array<ImportGroup> import1;
    private Array<Import2HiddenGroup> import2Hidden;
    private CodeValue hiddenCodeValue;
    private Code hiddenCode;
    private WorkArea scrollIndicator;
    private Common hiddenInitialIndex;
    private Common hiddenFinalIndex;
    private Common hiddenGroupFull;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExpGroup group.</summary>
    [Serializable]
    public class ExpGroup
    {
      /// <summary>
      /// A value of SelectChar.
      /// </summary>
      [JsonPropertyName("selectChar")]
      public Common SelectChar
      {
        get => selectChar ??= new();
        set => selectChar = value;
      }

      /// <summary>
      /// A value of Datail.
      /// </summary>
      [JsonPropertyName("datail")]
      public Incarceration Datail
      {
        get => datail ??= new();
        set => datail = value;
      }

      /// <summary>
      /// A value of Deatail.
      /// </summary>
      [JsonPropertyName("deatail")]
      public IncarcerationAddress Deatail
      {
        get => deatail ??= new();
        set => deatail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Common selectChar;
      private Incarceration datail;
      private IncarcerationAddress deatail;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public JailAddresses DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
      }

      /// <summary>
      /// A value of SelChar.
      /// </summary>
      [JsonPropertyName("selChar")]
      public Common SelChar
      {
        get => selChar ??= new();
        set => selChar = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public JailAddresses Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailStatePrompt.
      /// </summary>
      [JsonPropertyName("detailStatePrompt")]
      public Common DetailStatePrompt
      {
        get => detailStatePrompt ??= new();
        set => detailStatePrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private JailAddresses detailHidden;
      private Common selChar;
      private JailAddresses detail;
      private Common detailStatePrompt;
    }

    /// <summary>A Export2HiddenGroup group.</summary>
    [Serializable]
    public class Export2HiddenGroup
    {
      /// <summary>
      /// A value of Export2HiddenDetail.
      /// </summary>
      [JsonPropertyName("export2HiddenDetail")]
      public JailAddresses Export2HiddenDetail
      {
        get => export2HiddenDetail ??= new();
        set => export2HiddenDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private JailAddresses export2HiddenDetail;
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
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public CsePerson H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of SelectedIncarceration.
    /// </summary>
    [JsonPropertyName("selectedIncarceration")]
    public Incarceration SelectedIncarceration
    {
      get => selectedIncarceration ??= new();
      set => selectedIncarceration = value;
    }

    /// <summary>
    /// A value of SelectedIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("selectedIncarcerationAddress")]
    public IncarcerationAddress SelectedIncarcerationAddress
    {
      get => selectedIncarcerationAddress ??= new();
      set => selectedIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// Gets a value of Exp.
    /// </summary>
    [JsonIgnore]
    public Array<ExpGroup> Exp => exp ??= new(ExpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Exp for json serialization.
    /// </summary>
    [JsonPropertyName("exp")]
    [Computed]
    public IList<ExpGroup> Exp_Json
    {
      get => exp;
      set => Exp.Assign(value);
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public JailAddresses Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of FilterStatePrompt.
    /// </summary>
    [JsonPropertyName("filterStatePrompt")]
    public Common FilterStatePrompt
    {
      get => filterStatePrompt ??= new();
      set => filterStatePrompt = value;
    }

    /// <summary>
    /// A value of FilterHidden.
    /// </summary>
    [JsonPropertyName("filterHidden")]
    public JailAddresses FilterHidden
    {
      get => filterHidden ??= new();
      set => filterHidden = value;
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

    /// <summary>
    /// Gets a value of Export2Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<Export2HiddenGroup> Export2Hidden => export2Hidden ??= new(
      Export2HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export2Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("export2Hidden")]
    [Computed]
    public IList<Export2HiddenGroup> Export2Hidden_Json
    {
      get => export2Hidden;
      set => Export2Hidden.Assign(value);
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
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of HiddenInitialIndex.
    /// </summary>
    [JsonPropertyName("hiddenInitialIndex")]
    public Common HiddenInitialIndex
    {
      get => hiddenInitialIndex ??= new();
      set => hiddenInitialIndex = value;
    }

    /// <summary>
    /// A value of HiddenFinalIndex.
    /// </summary>
    [JsonPropertyName("hiddenFinalIndex")]
    public Common HiddenFinalIndex
    {
      get => hiddenFinalIndex ??= new();
      set => hiddenFinalIndex = value;
    }

    /// <summary>
    /// A value of HiddenGroupFull.
    /// </summary>
    [JsonPropertyName("hiddenGroupFull")]
    public Common HiddenGroupFull
    {
      get => hiddenGroupFull ??= new();
      set => hiddenGroupFull = value;
    }

    private Standard standard;
    private CsePerson h;
    private Incarceration selectedIncarceration;
    private IncarcerationAddress selectedIncarcerationAddress;
    private Common work;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ExpGroup> exp;
    private JailAddresses filter;
    private Common filterStatePrompt;
    private JailAddresses filterHidden;
    private Array<ExportGroup> export1;
    private Array<Export2HiddenGroup> export2Hidden;
    private CodeValue hiddenCodeValue;
    private Code hiddenCode;
    private WorkArea scrollIndicator;
    private Common hiddenInitialIndex;
    private Common hiddenFinalIndex;
    private Common hiddenGroupFull;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CountSelChar.
    /// </summary>
    [JsonPropertyName("countSelChar")]
    public Common CountSelChar
    {
      get => countSelChar ??= new();
      set => countSelChar = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public JailAddresses Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of SelectedCount.
    /// </summary>
    [JsonPropertyName("selectedCount")]
    public Common SelectedCount
    {
      get => selectedCount ??= new();
      set => selectedCount = value;
    }

    /// <summary>
    /// A value of Rtn.
    /// </summary>
    [JsonPropertyName("rtn")]
    public Common Rtn
    {
      get => rtn ??= new();
      set => rtn = value;
    }

    /// <summary>
    /// A value of BlankIndex.
    /// </summary>
    [JsonPropertyName("blankIndex")]
    public Common BlankIndex
    {
      get => blankIndex ??= new();
      set => blankIndex = value;
    }

    private CodeValue codeValue;
    private Code code;
    private Common countSelChar;
    private JailAddresses blank;
    private Common selectedCount;
    private Common rtn;
    private Common blankIndex;
  }
#endregion
}

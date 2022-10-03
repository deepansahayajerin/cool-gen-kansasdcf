// Program: SP_FDLM_FIELD_LIST_AND_MAINTAIN, ID: 372108938, model: 746.
// Short name: SWEFDLMP
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
/// A program: SP_FDLM_FIELD_LIST_AND_MAINTAIN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpFdlmFieldListAndMaintain: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_FDLM_FIELD_LIST_AND_MAINTAIN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpFdlmFieldListAndMaintain(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpFdlmFieldListAndMaintain.
  /// </summary>
  public SpFdlmFieldListAndMaintain(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ----------------------------------------------------------------
    // 09/20/1998	M. Ramirez	Initial Development
    // 01/19/1999	M. Ramirez	Added filters Dependancy and Subroutine
    // 01/19/1999	M. Ramirez	Removed 'zdel' exitstates
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ±ææææææææææææææææææææææææææææææææÉ
    // ø 1.0 Move imports to exports    ø
    // þææææææææææææææææææææææææææææææææÊ
    export.Filter.Assign(import.Filter);
    export.PreviousFilter.Assign(import.PreviousFilter);

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Gfield.Assign(import.Import1.Item.Gfield);
      export.Export1.Update.Gcommon.SelectChar =
        import.Import1.Item.Gcommon.SelectChar;
      export.Export1.Update.GexportScreenPrompt.PromptField =
        import.Import1.Item.GimportScreenPrompt.PromptField;
      export.Export1.Update.GexportPrevious.Assign(
        import.Import1.Item.GimportPrevious);
    }

    import.Import1.CheckIndex();
    MoveStandard(import.Scrolling, export.Scrolling);

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      MoveField(import.HiddenPageKeys.Item.GimportHidden,
        export.HiddenPageKeys.Update.GexportHidden);
    }

    import.HiddenPageKeys.CheckIndex();

    // mjr
    // -------------------------------------------------------------
    // Next tran and Security start here
    // ----------------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // The user requested a next tran action.
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // The user is comming into this procedure on a next tran action.
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // Flow from the menu
      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // mjr
    // -------------------------------------------------------------
    // Next tran and Security end here
    // ----------------------------------------------------------------
    if (!Equal(global.Command, "DISPLAY"))
    {
      // mjr
      // ---------------------------------------------------
      // Validate select characters
      // ------------------------------------------------------
      local.Select.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Export1.Item.Gcommon.SelectChar))
        {
          case 'S':
            ++local.Select.Count;

            break;
          case '+':
            export.Export1.Update.Gcommon.SelectChar = "";

            break;
          case '*':
            export.Export1.Update.Gcommon.SelectChar = "";

            break;
          case ' ':
            break;
          default:
            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(export.Export1.Item.GexportScreenPrompt.PromptField))
        {
          case 'S':
            ++local.Prompt.Count;

            break;
          case '+':
            export.Export1.Update.GexportScreenPrompt.PromptField = "";

            break;
          case ' ':
            break;
          default:
            var field1 =
              GetField(export.Export1.Item.GexportScreenPrompt, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }
      }

      export.Export1.CheckIndex();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if ((!Equal(export.Filter.Description, export.PreviousFilter.Description) ||
      !Equal(export.Filter.Dependancy, export.PreviousFilter.Dependancy) || !
      Equal(export.Filter.SubroutineName, export.PreviousFilter.SubroutineName) ||
      export.Scrolling.PageNumber == 0) && (
        Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV")))
    {
      export.PreviousFilter.Assign(export.Filter);
      global.Command = "DISPLAY";
    }

    // mjr
    // ---------------------------------------------------
    // Common validations for Add, Update and Delete
    // ------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (local.Select.Count < 1)
      {
        if (export.Export1.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < 1; ++
            export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_REQUEST_REQUIRES_SEL";

            return;
          }

          export.Export1.CheckIndex();
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Error = true;

            ExitState = "SP0000_REQUEST_REQUIRES_SEL";

            return;
          }

          export.Export1.CheckIndex();
        }
      }

      // mjr--->  For cab_validate_code_value
      export.Code.CodeName = "DOCUMENT TYPE";
      export.Export1.Index = 0;

      for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
        export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) != 'S')
        {
          continue;
        }

        if (IsEmpty(export.Export1.Item.Gfield.Name) && IsEmpty
          (export.Export1.Item.Gfield.Description) && IsEmpty
          (export.Export1.Item.Gfield.Dependancy) && IsEmpty
          (export.Export1.Item.Gfield.SubroutineName) && IsEmpty
          (export.Export1.Item.Gfield.ScreenName))
        {
          if (Equal(global.Command, "ADD"))
          {
            var field1 = GetField(export.Export1.Item.Gfield, "screenName");

            field1.Error = true;

            var field2 = GetField(export.Export1.Item.Gfield, "subroutineName");

            field2.Error = true;

            var field3 = GetField(export.Export1.Item.Gfield, "dependancy");

            field3.Error = true;

            var field4 = GetField(export.Export1.Item.Gfield, "description");

            field4.Error = true;

            var field5 = GetField(export.Export1.Item.Gfield, "name");

            field5.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
          else
          {
            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Error = true;

            ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
          }

          continue;
        }

        if (AsChar(export.Export1.Item.GexportScreenPrompt.PromptField) == 'S')
        {
          var field1 =
            GetField(export.Export1.Item.GexportScreenPrompt, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }

        if (IsEmpty(export.Export1.Item.Gfield.ScreenName))
        {
          var field1 = GetField(export.Export1.Item.Gfield, "screenName");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
        else
        {
          local.CodeValue.Cdvalue = export.Export1.Item.Gfield.ScreenName;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            var field1 = GetField(export.Export1.Item.Gfield, "screenName");

            field1.Error = true;

            ExitState = "INVALID_VALUE";
          }
        }

        if (IsEmpty(export.Export1.Item.Gfield.SubroutineName))
        {
          var field1 = GetField(export.Export1.Item.Gfield, "subroutineName");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.Export1.Item.Gfield.Dependancy))
        {
          var field1 = GetField(export.Export1.Item.Gfield, "dependancy");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.Export1.Item.Gfield.Description))
        {
          var field1 = GetField(export.Export1.Item.Gfield, "description");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.Export1.Item.Gfield.Name))
        {
          var field1 = GetField(export.Export1.Item.Gfield, "name");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }

      export.Export1.CheckIndex();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // mjr
    // -------------------------------------------------------------
    //                C A S E   O F   C O M M A N D
    // ----------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            UseSpCabCreateField();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("FIELD_AE"))
              {
                var field1 = GetField(export.Export1.Item.Gfield, "name");

                field1.Error = true;
              }
              else
              {
                var field1 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field1.Error = true;
              }

              return;
            }

            export.Export1.Update.Gcommon.SelectChar = "*";
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        return;
      case "UPDATE":
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (!Equal(export.Export1.Item.Gfield.Name,
              export.Export1.Item.GexportPrevious.Name))
            {
              var field1 = GetField(export.Export1.Item.Gfield, "name");

              field1.Error = true;

              ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";

              return;
            }

            if (Equal(export.Export1.Item.Gfield.Description,
              export.Export1.Item.GexportPrevious.Description) && Equal
              (export.Export1.Item.Gfield.Dependancy,
              export.Export1.Item.GexportPrevious.Dependancy) && Equal
              (export.Export1.Item.Gfield.SubroutineName,
              export.Export1.Item.GexportPrevious.SubroutineName) && Equal
              (export.Export1.Item.Gfield.ScreenName,
              export.Export1.Item.GexportPrevious.ScreenName))
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;

              ExitState = "SP0000_DATA_NOT_CHANGED";

              return;
            }

            UseSpCabUpdateField();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("FIELD_NF") || IsExitState("FIELD_NU"))
              {
                var field1 = GetField(export.Export1.Item.Gfield, "name");

                field1.Error = true;
              }
              else
              {
                var field1 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field1.Error = true;
              }

              return;
            }

            export.Export1.Update.Gcommon.SelectChar = "*";
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        return;
      case "DELETE":
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (!Equal(export.Export1.Item.Gfield.Name,
              export.Export1.Item.GexportPrevious.Name))
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_NEW_KEY_ON_DELETE";

              return;
            }

            UseSpCabDeleteField();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("FIELD_NF"))
              {
                var field1 = GetField(export.Export1.Item.Gfield, "name");

                field1.Error = true;
              }
              else
              {
                var field1 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field1.Error = true;
              }

              return;
            }

            export.Export1.Update.Gcommon.SelectChar = "*";
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        return;
      case "LIST":
        if (local.Select.Count == 1 && local.Prompt.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S' && AsChar
              (export.Export1.Item.GexportScreenPrompt.PromptField) == 'S')
            {
              export.Code.CodeName = "DOCUMENT TYPE";
              ExitState = "ECO_LNK_TO_LIST";

              return;
            }
          }

          export.Export1.CheckIndex();
        }

        // mjr--->  Everything else is an error
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S' && AsChar
            (export.Export1.Item.GexportScreenPrompt.PromptField) == 'S')
          {
            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.GexportScreenPrompt, "promptField");
                

              field2.Error = true;

              return;
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            continue;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S' || AsChar
            (export.Export1.Item.GexportScreenPrompt.PromptField) == 'S')
          {
            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              var field1 =
                GetField(export.Export1.Item.GexportScreenPrompt, "promptField");
                

              field1.Error = true;

              if (!IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              }
              else
              {
                var field2 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field2.Error = true;
              }

              return;
            }

            if (AsChar(export.Export1.Item.GexportScreenPrompt.PromptField) == 'S'
              )
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;

              if (!IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
              {
                ExitState = "SP0000_REQUEST_REQUIRES_SEL";
              }
              else
              {
                var field2 =
                  GetField(export.Export1.Item.GexportScreenPrompt,
                  "promptField");

                field2.Error = true;
              }

              return;
            }
          }
        }

        export.Export1.CheckIndex();

        for(export.Export1.Index = 0; export.Export1.Index < 1; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

          field1.Error = true;

          ExitState = "SP0000_REQUEST_REQUIRES_SEL";

          return;
        }

        export.Export1.CheckIndex();

        break;
      case "RLCVAL":
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (!IsEmpty(import.CodeValue.Cdvalue))
            {
              export.Export1.Update.Gfield.ScreenName =
                import.CodeValue.Cdvalue;
            }

            export.Export1.Update.GexportScreenPrompt.PromptField = "";

            ++export.Export1.Index;
            export.Export1.CheckSize();

            var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "RETURN":
        if (local.Select.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;

              break;
            }
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }
        else if (local.Select.Count < 1)
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.Gfield.Name) && IsEmpty
              (export.Export1.Item.Gfield.Description) && IsEmpty
              (export.Export1.Item.Gfield.Dependancy) && IsEmpty
              (export.Export1.Item.Gfield.SubroutineName) && IsEmpty
              (export.Export1.Item.Gfield.ScreenName))
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;

              ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
            }
            else
            {
              MoveField(export.Export1.Item.Gfield, export.Selected);
              ExitState = "ACO_NE0000_RETURN";
            }

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "NEXT":
        if (export.Scrolling.PageNumber == Export.HiddenPageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          return;
        }

        export.HiddenPageKeys.Index = export.Scrolling.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.GexportHidden.Description) || IsEmpty
          (export.HiddenPageKeys.Item.GexportHidden.Name))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        if (local.Select.Count > 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;
            }
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

          return;
        }

        ++export.Scrolling.PageNumber;

        break;
      case "PREV":
        if (export.Scrolling.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        if (local.Select.Count > 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field1.Error = true;
            }
          }

          export.Export1.CheckIndex();
          ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

          return;
        }

        --export.Scrolling.PageNumber;

        break;
      case "DISPLAY":
        export.Scrolling.PageNumber = 1;

        export.HiddenPageKeys.Index = 0;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHidden.Description =
          export.Filter.Description;

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      export.Export1.Count = 0;

      export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      MoveField(export.HiddenPageKeys.Item.GexportHidden, local.PageStartKey);
      export.Export1.Index = -1;

      foreach(var item in ReadField())
      {
        // mjr
        // -----------------------------------------------
        // 01/19/1999
        // Added check for new filters -- dependancy and subroutine
        // ------------------------------------------------------------
        if (!Equal(entities.Field.Dependancy, export.Filter.Dependancy) && !
          IsEmpty(export.Filter.Dependancy))
        {
          continue;
        }

        if (!Equal(entities.Field.SubroutineName, export.Filter.SubroutineName) &&
          !IsEmpty(export.Filter.SubroutineName))
        {
          continue;
        }

        // mjr
        // -----------------------------------------------
        // 01/19/1999
        // End modification
        // ------------------------------------------------------------
        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          break;
        }

        export.Export1.Update.Gcommon.SelectChar = "";
        export.Export1.Update.GexportScreenPrompt.PromptField = "";
        export.Export1.Update.Gfield.Assign(entities.Field);
        export.Export1.Update.GexportPrevious.Assign(entities.Field);
      }

      // mjr---> Handle exitstate and MORE indicator
      if (export.Export1.IsEmpty)
      {
        // mjr
        // -----------------------------------------------------------------
        // This will only happen on the first page, because on following
        // pages we check to make sure a record exists even before they scroll.
        // --------------------------------------------------------------------
        export.Scrolling.ScrollingMessage = "MORE";
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else if (export.Export1.IsFull && export.Export1.Index >= Export
        .ExportGroup.Capacity)
      {
        // mjr
        // ----------------------------------------------------------------
        // This page is full AND we found at least one more record to occupy
        // another page.
        // -------------------------------------------------------------------
        if (export.HiddenPageKeys.Index + 1 == Export
          .HiddenPageKeysGroup.Capacity)
        {
          // mjr
          // ----------------------------------------------------------------
          // The user has scrolled the maximum number of scrolls, and at
          // least one more record exists.
          // -------------------------------------------------------------------
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
          export.Scrolling.ScrollingMessage = "MORE - +";
        }
        else
        {
          // mjr
          // ----------------------------------------------------------------
          // The user has NOT scrolled the maximum number of scrolls, and at
          // least one more record exists.
          // -------------------------------------------------------------------
          ++export.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          MoveField(entities.Field, export.HiddenPageKeys.Update.GexportHidden);

          if (export.Scrolling.PageNumber > 1)
          {
            export.Scrolling.ScrollingMessage = "MORE - +";
          }
          else
          {
            export.Scrolling.ScrollingMessage = "MORE   +";
          }
        }
      }
      else
      {
        // mjr
        // ----------------------------------------------------------------
        // This page is not full (or it is full and no more records exist).
        // User cannot scroll anymore.
        // -------------------------------------------------------------------
        ++export.HiddenPageKeys.Index;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHidden.Description = "";
        export.HiddenPageKeys.Update.GexportHidden.Name = "";

        if (export.Scrolling.PageNumber <= 1)
        {
          export.Scrolling.ScrollingMessage = "MORE";
        }
        else
        {
          export.Scrolling.ScrollingMessage = "MORE -";
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          var field1 = GetField(export.Export1.Item.Gcommon, "selectChar");

          field1.Protected = false;
          field1.Focused = true;

          return;
        }

        export.Export1.CheckIndex();
      }
    }
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = export.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

  private void UseSpCabCreateField()
  {
    var useImport = new SpCabCreateField.Import();
    var useExport = new SpCabCreateField.Export();

    useImport.Field.Assign(export.Export1.Item.Gfield);

    Call(SpCabCreateField.Execute, useImport, useExport);
  }

  private void UseSpCabDeleteField()
  {
    var useImport = new SpCabDeleteField.Import();
    var useExport = new SpCabDeleteField.Export();

    useImport.Field.Name = export.Export1.Item.Gfield.Name;

    Call(SpCabDeleteField.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateField()
  {
    var useImport = new SpCabUpdateField.Import();
    var useExport = new SpCabUpdateField.Export();

    useImport.Field.Assign(export.Export1.Item.Gfield);

    Call(SpCabUpdateField.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadField()
  {
    entities.Field.Populated = false;

    return ReadEach("ReadField",
      (db, command) =>
      {
        db.SetString(command, "description", local.PageStartKey.Description);
        db.SetString(command, "name", local.PageStartKey.Name);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.ScreenName = db.GetString(reader, 3);
        entities.Field.Description = db.GetString(reader, 4);
        entities.Field.Populated = true;

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
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportHidden.
      /// </summary>
      [JsonPropertyName("gimportHidden")]
      public Field GimportHidden
      {
        get => gimportHidden ??= new();
        set => gimportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Field gimportHidden;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of Gfield.
      /// </summary>
      [JsonPropertyName("gfield")]
      public Field Gfield
      {
        get => gfield ??= new();
        set => gfield = value;
      }

      /// <summary>
      /// A value of GimportScreenPrompt.
      /// </summary>
      [JsonPropertyName("gimportScreenPrompt")]
      public Standard GimportScreenPrompt
      {
        get => gimportScreenPrompt ??= new();
        set => gimportScreenPrompt = value;
      }

      /// <summary>
      /// A value of GimportPrevious.
      /// </summary>
      [JsonPropertyName("gimportPrevious")]
      public Field GimportPrevious
      {
        get => gimportPrevious ??= new();
        set => gimportPrevious = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private Common gcommon;
      private Field gfield;
      private Standard gimportScreenPrompt;
      private Field gimportPrevious;
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
    /// A value of PreviousFilter.
    /// </summary>
    [JsonPropertyName("previousFilter")]
    public Field PreviousFilter
    {
      get => previousFilter ??= new();
      set => previousFilter = value;
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Field Filter
    {
      get => filter ??= new();
      set => filter = value;
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

    private CodeValue codeValue;
    private Field previousFilter;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard scrolling;
    private Field filter;
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
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportHidden.
      /// </summary>
      [JsonPropertyName("gexportHidden")]
      public Field GexportHidden
      {
        get => gexportHidden ??= new();
        set => gexportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Field gexportHidden;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of Gfield.
      /// </summary>
      [JsonPropertyName("gfield")]
      public Field Gfield
      {
        get => gfield ??= new();
        set => gfield = value;
      }

      /// <summary>
      /// A value of GexportScreenPrompt.
      /// </summary>
      [JsonPropertyName("gexportScreenPrompt")]
      public Standard GexportScreenPrompt
      {
        get => gexportScreenPrompt ??= new();
        set => gexportScreenPrompt = value;
      }

      /// <summary>
      /// A value of GexportPrevious.
      /// </summary>
      [JsonPropertyName("gexportPrevious")]
      public Field GexportPrevious
      {
        get => gexportPrevious ??= new();
        set => gexportPrevious = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private Common gcommon;
      private Field gfield;
      private Standard gexportScreenPrompt;
      private Field gexportPrevious;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Field Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of PreviousFilter.
    /// </summary>
    [JsonPropertyName("previousFilter")]
    public Field PreviousFilter
    {
      get => previousFilter ??= new();
      set => previousFilter = value;
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Field Filter
    {
      get => filter ??= new();
      set => filter = value;
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

    private Code code;
    private Field selected;
    private Field previousFilter;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard scrolling;
    private Field filter;
    private Array<ExportGroup> export1;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of PageStartKey.
    /// </summary>
    [JsonPropertyName("pageStartKey")]
    public Field PageStartKey
    {
      get => pageStartKey ??= new();
      set => pageStartKey = value;
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

    private Common prompt;
    private Common validCode;
    private CodeValue codeValue;
    private Field pageStartKey;
    private Common select;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    private Field field;
  }
#endregion
}

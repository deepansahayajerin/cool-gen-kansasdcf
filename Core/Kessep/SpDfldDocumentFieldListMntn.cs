// Program: SP_DFLD_DOCUMENT_FIELD_LIST_MNTN, ID: 372103439, model: 746.
// Short name: SWEDFLDP
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
/// A program: SP_DFLD_DOCUMENT_FIELD_LIST_MNTN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDfldDocumentFieldListMntn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DFLD_DOCUMENT_FIELD_LIST_MNTN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDfldDocumentFieldListMntn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDfldDocumentFieldListMntn.
  /// </summary>
  public SpDfldDocumentFieldListMntn(IContext context, Import import,
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
    // 09/21/1998	M. Ramirez			Initial Development
    // 07/10/2000	M Ramirez	80722		Add Required field indicator value 'U'
    // 07/10/2000	M Ramirez			Cleanup of some zdel exitstates
    // 07/10/2000	M Ramirez			Changed READs to SELECT ONLY
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

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
    MoveDocument(import.FilterDocument, export.FilterDocument);
    export.FilterDocumentField.Position = import.FilterDocumentField.Position;
    export.FilterPreviousDocument.Name = import.FilterPreviousDocument.Name;
    export.FilterPreviousDocumentField.Position =
      import.FilterPreviousDocumentField.Position;
    export.FilterStandard.PromptField = import.FilterStandard.PromptField;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.Gcommon.SelectChar =
        import.Import1.Item.Gcommon.SelectChar;
      export.Export1.Update.GdocumentField.Assign(
        import.Import1.Item.GdocumentField);
      export.Export1.Update.Gfield.Assign(import.Import1.Item.Gfield);
      export.Export1.Update.GexportFieldPrompt.PromptField =
        import.Import1.Item.GimportFieldPrompt.PromptField;
      export.Export1.Update.GexportPreviousDocumentField.Assign(
        import.Import1.Item.GimportPreviousDocumentField);
      export.Export1.Update.GexportPreviousField.Assign(
        import.Import1.Item.GimportPreviousField);
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

      export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position =
        import.HiddenPageKeys.Item.GimportHiddenDocumentField.Position;
      export.HiddenPageKeys.Update.GexportHiddenField.Name =
        import.HiddenPageKeys.Item.GimportHiddenField.Name;
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
      // Validate header select character
      // ------------------------------------------------------
      local.Select.Count = 0;

      switch(AsChar(export.FilterStandard.PromptField))
      {
        case 'S':
          switch(TrimEnd(global.Command))
          {
            case "RETLINK":
              break;
            case "LIST":
              ++local.Select.Count;

              break;
            default:
              var field2 = GetField(export.FilterStandard, "promptField");

              field2.Error = true;

              ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

              break;
          }

          break;
        case '+':
          export.FilterStandard.PromptField = "";

          break;
        case ' ':
          break;
        default:
          var field1 = GetField(export.FilterStandard, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // mjr
      // ---------------------------------------------------
      // Validate select characters
      // ------------------------------------------------------
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

        switch(AsChar(export.Export1.Item.GexportFieldPrompt.PromptField))
        {
          case 'S':
            ++local.Prompt.Count;

            break;
          case '+':
            export.Export1.Update.GexportFieldPrompt.PromptField = "";

            break;
          case ' ':
            break;
          default:
            var field1 =
              GetField(export.Export1.Item.GexportFieldPrompt, "promptField");

            field1.Error = true;

            ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";

            break;
        }
      }

      export.Export1.CheckIndex();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if ((
      !Equal(export.FilterDocument.Name, export.FilterPreviousDocument.Name) ||
      export.FilterDocumentField.Position != export
      .FilterPreviousDocumentField.Position || export.Scrolling.PageNumber == 0
      ) && (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "NEXT") || Equal(global.Command, "PREV")))
    {
      export.FilterPreviousDocument.Name = export.FilterDocument.Name;
      export.FilterPreviousDocumentField.Position =
        export.FilterDocumentField.Position;
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

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) != 'S')
        {
          continue;
        }

        if (AsChar(export.Export1.Item.GexportFieldPrompt.PromptField) == 'S')
        {
          var field1 =
            GetField(export.Export1.Item.GexportFieldPrompt, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }

        if (export.Export1.Item.GdocumentField.Position == 0 && IsEmpty
          (export.Export1.Item.GdocumentField.RequiredSwitch) && IsEmpty
          (export.Export1.Item.GdocumentField.ScreenPrompt) && IsEmpty
          (export.Export1.Item.Gfield.Name))
        {
          if (Equal(global.Command, "ADD"))
          {
            var field1 = GetField(export.Export1.Item.Gfield, "name");

            field1.Error = true;

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
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) != 'S')
          {
            continue;
          }

          // mjr
          // ---------------------------------------
          // 09/23/1998
          // Set defaults
          // ----------------------------------------------------
          if (export.Export1.Item.GdocumentField.Position == 0)
          {
            export.Export1.Update.GdocumentField.Position = 1;
          }

          if (IsEmpty(export.Export1.Item.GdocumentField.RequiredSwitch))
          {
            export.Export1.Update.GdocumentField.RequiredSwitch = "Y";
          }
          else
          {
            switch(AsChar(export.Export1.Item.GdocumentField.RequiredSwitch))
            {
              case 'Y':
                break;
              case 'N':
                break;
              case 'U':
                if (ReadField())
                {
                  if (Equal(entities.Field.Dependancy, " KEY"))
                  {
                    var field3 =
                      GetField(export.Export1.Item.GdocumentField,
                      "requiredSwitch");

                    field3.Error = true;

                    ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                    return;
                  }
                }
                else
                {
                  var field3 = GetField(export.Export1.Item.Gfield, "name");

                  field3.Error = true;

                  ExitState = "FIELD_NF";

                  return;
                }

                break;
              default:
                var field2 =
                  GetField(export.Export1.Item.GdocumentField, "requiredSwitch");
                  

                field2.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YNU";

                return;
            }
          }

          if (IsEmpty(export.Export1.Item.GdocumentField.ScreenPrompt))
          {
            if (ReadField())
            {
              export.Export1.Update.GdocumentField.ScreenPrompt =
                entities.Field.Description;
            }
            else
            {
              var field2 = GetField(export.Export1.Item.Gfield, "name");

              field2.Error = true;

              ExitState = "FIELD_NF";

              return;
            }
          }

          UseSpCabCreateDocumentField();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("DOCUMENT_NF"))
            {
              var field2 = GetField(export.FilterDocument, "name");

              field2.Error = true;

              export.FilterDocument.Description = "";
            }
            else if (IsExitState("FIELD_NF"))
            {
              var field2 = GetField(export.Export1.Item.Gfield, "name");

              field2.Error = true;
            }
            else
            {
              var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field2.Error = true;
            }

            return;
          }

          export.Export1.Update.Gcommon.SelectChar = "*";
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        return;
      case "UPDATE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) != 'S')
          {
            continue;
          }

          if (export.Export1.Item.GexportPreviousDocumentField.Position == 0
            || IsEmpty
            (export.Export1.Item.GexportPreviousDocumentField.RequiredSwitch) ||
            IsEmpty
            (export.Export1.Item.GexportPreviousDocumentField.ScreenPrompt) || IsEmpty
            (export.Export1.Item.GexportPreviousField.Name))
          {
            var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

            return;
          }

          if (export.Export1.Item.GdocumentField.Position == export
            .Export1.Item.GexportPreviousDocumentField.Position && AsChar
            (export.Export1.Item.GdocumentField.RequiredSwitch) == AsChar
            (export.Export1.Item.GexportPreviousDocumentField.RequiredSwitch) &&
            Equal
            (export.Export1.Item.GdocumentField.ScreenPrompt,
            export.Export1.Item.GexportPreviousDocumentField.ScreenPrompt) && Equal
            (export.Export1.Item.Gfield.Name,
            export.Export1.Item.GexportPreviousField.Name))
          {
            var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field2.Error = true;

            ExitState = "SP0000_DATA_NOT_CHANGED";

            return;
          }

          // mjr
          // ---------------------------------------
          // 09/23/1998
          // Set defaults
          // ----------------------------------------------------
          if (export.Export1.Item.GdocumentField.Position == 0)
          {
            export.Export1.Update.GdocumentField.Position = 1;
          }

          if (IsEmpty(export.Export1.Item.GdocumentField.RequiredSwitch))
          {
            export.Export1.Update.GdocumentField.RequiredSwitch = "Y";
          }
          else
          {
            switch(AsChar(export.Export1.Item.GdocumentField.RequiredSwitch))
            {
              case 'Y':
                break;
              case 'N':
                break;
              case 'U':
                if (ReadField())
                {
                  if (Equal(entities.Field.Dependancy, " KEY"))
                  {
                    var field3 =
                      GetField(export.Export1.Item.GdocumentField,
                      "requiredSwitch");

                    field3.Error = true;

                    ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                    return;
                  }
                }
                else
                {
                  var field3 = GetField(export.Export1.Item.Gfield, "name");

                  field3.Error = true;

                  ExitState = "FIELD_NF";

                  return;
                }

                break;
              default:
                var field2 =
                  GetField(export.Export1.Item.GdocumentField, "requiredSwitch");
                  

                field2.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YNU";

                return;
            }
          }

          if (IsEmpty(export.Export1.Item.GdocumentField.ScreenPrompt))
          {
            if (ReadField())
            {
              export.Export1.Update.GdocumentField.ScreenPrompt =
                entities.Field.Description;
            }
            else
            {
              var field2 = GetField(export.Export1.Item.Gfield, "name");

              field2.Error = true;

              ExitState = "FIELD_NF";

              return;
            }
          }

          UseSpCabUpdateDocumentField();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("DOCUMENT_NF"))
            {
              var field2 = GetField(export.FilterDocument, "name");

              field2.Error = true;

              export.FilterDocument.Description = "";
            }
            else if (IsExitState("FIELD_NF"))
            {
              var field2 = GetField(export.Export1.Item.Gfield, "name");

              field2.Error = true;
            }
            else
            {
              var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field2.Error = true;
            }

            return;
          }

          export.Export1.Update.Gcommon.SelectChar = "*";
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        return;
      case "DELETE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) != 'S')
          {
            continue;
          }

          if (export.Export1.Item.GdocumentField.Position != export
            .Export1.Item.GexportPreviousDocumentField.Position && AsChar
            (export.Export1.Item.GdocumentField.RequiredSwitch) != AsChar
            (export.Export1.Item.GexportPreviousDocumentField.RequiredSwitch) &&
            !
            Equal(export.Export1.Item.GdocumentField.ScreenPrompt,
            export.Export1.Item.GexportPreviousDocumentField.ScreenPrompt) && !
            Equal(export.Export1.Item.Gfield.Name,
            export.Export1.Item.GexportPreviousField.Name))
          {
            var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

            return;
          }

          UseSpCabDeleteDocumentField();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("DOCUMENT_NF"))
            {
              var field2 = GetField(export.FilterDocument, "name");

              field2.Error = true;

              export.FilterDocument.Description = "";
            }
            else if (IsExitState("FIELD_NF"))
            {
              var field2 = GetField(export.Export1.Item.Gfield, "name");

              field2.Error = true;
            }
            else
            {
              var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field2.Error = true;
            }

            return;
          }

          export.Export1.Update.Gcommon.SelectChar = "*";
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        return;
      case "LIST":
        if (local.Select.Count == 1 && local.Prompt.Count == 0 && AsChar
          (export.FilterStandard.PromptField) == 'S')
        {
          ExitState = "ECO_XFR_TO_DOCM";

          return;
        }

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
              (export.Export1.Item.GexportFieldPrompt.PromptField) == 'S')
            {
              ExitState = "ECO_XFR_TO_FDLM";

              return;
            }
          }

          export.Export1.CheckIndex();
        }

        // mjr--->  Everything else is an error
        if (AsChar(export.FilterStandard.PromptField) == 'S')
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S' && AsChar
            (export.Export1.Item.GexportFieldPrompt.PromptField) == 'S')
          {
            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.GexportFieldPrompt, "promptField");
                

              field3.Error = true;

              return;
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            continue;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S' || AsChar
            (export.Export1.Item.GexportFieldPrompt.PromptField) == 'S')
          {
            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              var field2 =
                GetField(export.Export1.Item.GexportFieldPrompt, "promptField");
                

              field2.Error = true;

              if (!IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              }
              else
              {
                var field3 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field3.Error = true;
              }

              return;
            }

            if (AsChar(export.Export1.Item.GexportFieldPrompt.PromptField) == 'S'
              )
            {
              var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field2.Error = true;

              if (!IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
              {
                ExitState = "SP0000_REQUEST_REQUIRES_SEL";
              }
              else
              {
                var field3 =
                  GetField(export.Export1.Item.GexportFieldPrompt, "promptField");
                  

                field3.Error = true;
              }

              return;
            }
          }
        }

        export.Export1.CheckIndex();

        var field1 = GetField(export.FilterStandard, "promptField");

        field1.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
      case "RETLINK":
        if (AsChar(export.FilterStandard.PromptField) == 'S')
        {
          export.FilterStandard.PromptField = "";

          if (!IsEmpty(import.FromPromptDocument.Name))
          {
            MoveDocument(import.FromPromptDocument, export.FilterDocument);

            // mjr
            // ----------------------------------------
            // 09/28/1998
            // Perform an auto display
            // -----------------------------------------------------
            export.Scrolling.PageNumber = 1;

            export.HiddenPageKeys.Index = 0;
            export.HiddenPageKeys.CheckSize();

            export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position =
              export.FilterDocumentField.Position;
            global.Command = "DISPLAY";

            break;
          }

          var field2 = GetField(export.FilterDocumentField, "position");

          field2.Protected = false;
          field2.Focused = true;

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
            if (!IsEmpty(import.FromPromptField.Name))
            {
              export.Export1.Update.Gfield.Name = import.FromPromptField.Name;
            }

            export.Export1.Update.GexportFieldPrompt.PromptField = "";

            var field2 =
              GetField(export.Export1.Item.GdocumentField, "requiredSwitch");

            field2.Protected = false;
            field2.Focused = true;

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

        if (IsEmpty(export.HiddenPageKeys.Item.GexportHiddenField.Name) || export
          .HiddenPageKeys.Item.GexportHiddenDocumentField.Position <= 0)
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
              var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field2.Error = true;
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
              var field2 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field2.Error = true;
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

        export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position =
          export.FilterDocumentField.Position;

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
      export.FilterStandard.PromptField = "";
      export.FilterDocument.Description = "";

      if (ReadDocument())
      {
        MoveDocument(entities.Document, export.FilterDocument);
      }
      else
      {
        ExitState = "DOCUMENT_NF";

        return;
      }

      export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      local.PageStartKeyField.Name =
        export.HiddenPageKeys.Item.GexportHiddenField.Name;
      local.PageStartKeyDocumentField.Position =
        export.HiddenPageKeys.Item.GexportHiddenDocumentField.Position;
      export.Export1.Index = -1;

      foreach(var item in ReadDocumentFieldField())
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          break;
        }

        export.Export1.Update.Gcommon.SelectChar = "";
        export.Export1.Update.GexportFieldPrompt.PromptField = "";
        MoveField(entities.Field, export.Export1.Update.Gfield);
        export.Export1.Update.GdocumentField.Assign(entities.DocumentField);
        MoveField(entities.Field, export.Export1.Update.GexportPreviousField);
        export.Export1.Update.GexportPreviousDocumentField.Assign(
          entities.DocumentField);
      }

      if (export.Export1.IsEmpty)
      {
        local.PageStartKeyField.Name = "";
        local.PageStartKeyDocumentField.Position = 0;

        if (export.Scrolling.PageNumber == 1)
        {
          export.Scrolling.ScrollingMessage = "MORE";
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }
      }

      if (export.Export1.IsFull && export.Export1.Index >= Export
        .ExportGroup.Capacity)
      {
        local.PageStartKeyField.Name = entities.Field.Name;
        local.PageStartKeyDocumentField.Position =
          entities.DocumentField.Position;

        if (export.Scrolling.PageNumber > 1)
        {
          export.Scrolling.ScrollingMessage = "MORE - +";
        }
        else
        {
          export.Scrolling.ScrollingMessage = "MORE   +";
        }
      }
      else
      {
        local.PageStartKeyField.Name = "";
        local.PageStartKeyDocumentField.Position = 0;

        if (export.Scrolling.PageNumber <= 1)
        {
          export.Scrolling.ScrollingMessage = "MORE";
        }
        else
        {
          export.Scrolling.ScrollingMessage = "MORE -";
        }
      }

      if (export.HiddenPageKeys.Index + 1 == Export
        .HiddenPageKeysGroup.Capacity)
      {
        ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
        export.Scrolling.ScrollingMessage = "MORE -";
      }
      else
      {
        ++export.HiddenPageKeys.Index;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHiddenField.Name =
          local.PageStartKeyField.Name;
        export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position =
          local.PageStartKeyDocumentField.Position;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }

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

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
    target.Dependancy = source.Dependancy;
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

  private void UseSpCabCreateDocumentField()
  {
    var useImport = new SpCabCreateDocumentField.Import();
    var useExport = new SpCabCreateDocumentField.Export();

    useImport.Field.Name = export.Export1.Item.Gfield.Name;
    useImport.Document.Name = export.FilterDocument.Name;
    useImport.DocumentField.Assign(export.Export1.Item.GdocumentField);

    Call(SpCabCreateDocumentField.Execute, useImport, useExport);
  }

  private void UseSpCabDeleteDocumentField()
  {
    var useImport = new SpCabDeleteDocumentField.Import();
    var useExport = new SpCabDeleteDocumentField.Export();

    useImport.Field.Name = export.Export1.Item.Gfield.Name;
    useImport.DocumentField.Assign(export.Export1.Item.GdocumentField);
    useImport.Document.Name = export.FilterDocument.Name;

    Call(SpCabDeleteDocumentField.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateDocumentField()
  {
    var useImport = new SpCabUpdateDocumentField.Import();
    var useExport = new SpCabUpdateDocumentField.Export();

    useImport.New1.Name = export.Export1.Item.Gfield.Name;
    useImport.Old.Name = export.Export1.Item.GexportPreviousField.Name;
    useImport.Document.Name = export.FilterDocument.Name;
    useImport.DocumentField.Assign(export.Export1.Item.GdocumentField);

    Call(SpCabUpdateDocumentField.Execute, useImport, useExport);
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", export.FilterDocument.Name);
        db.
          SetDate(command, "expirationDate", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Description = db.GetNullableString(reader, 1);
        entities.Document.EffectiveDate = db.GetDate(reader, 2);
        entities.Document.ExpirationDate = db.GetDate(reader, 3);
        entities.Document.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDocumentFieldField()
  {
    entities.DocumentField.Populated = false;
    entities.Field.Populated = false;

    return ReadEach("ReadDocumentFieldField",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
        db.SetInt32(
          command, "orderPosition", local.PageStartKeyDocumentField.Position);
        db.SetString(command, "name", local.PageStartKeyField.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.ScreenPrompt = db.GetString(reader, 2);
        entities.DocumentField.FldName = db.GetString(reader, 3);
        entities.Field.Name = db.GetString(reader, 3);
        entities.DocumentField.DocName = db.GetString(reader, 4);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 5);
        entities.Field.Dependancy = db.GetString(reader, 6);
        entities.Field.Description = db.GetString(reader, 7);
        entities.DocumentField.Populated = true;
        entities.Field.Populated = true;

        return true;
      });
  }

  private bool ReadField()
  {
    entities.Field.Populated = false;

    return Read("ReadField",
      (db, command) =>
      {
        db.SetString(command, "name", export.Export1.Item.Gfield.Name);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.Description = db.GetString(reader, 2);
        entities.Field.Populated = true;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GdocumentField.
      /// </summary>
      [JsonPropertyName("gdocumentField")]
      public DocumentField GdocumentField
      {
        get => gdocumentField ??= new();
        set => gdocumentField = value;
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
      /// A value of GimportPreviousDocumentField.
      /// </summary>
      [JsonPropertyName("gimportPreviousDocumentField")]
      public DocumentField GimportPreviousDocumentField
      {
        get => gimportPreviousDocumentField ??= new();
        set => gimportPreviousDocumentField = value;
      }

      /// <summary>
      /// A value of GimportPreviousField.
      /// </summary>
      [JsonPropertyName("gimportPreviousField")]
      public Field GimportPreviousField
      {
        get => gimportPreviousField ??= new();
        set => gimportPreviousField = value;
      }

      /// <summary>
      /// A value of GimportFieldPrompt.
      /// </summary>
      [JsonPropertyName("gimportFieldPrompt")]
      public Standard GimportFieldPrompt
      {
        get => gimportFieldPrompt ??= new();
        set => gimportFieldPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common gcommon;
      private DocumentField gdocumentField;
      private Field gfield;
      private DocumentField gimportPreviousDocumentField;
      private Field gimportPreviousField;
      private Standard gimportFieldPrompt;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportHiddenDocumentField.
      /// </summary>
      [JsonPropertyName("gimportHiddenDocumentField")]
      public DocumentField GimportHiddenDocumentField
      {
        get => gimportHiddenDocumentField ??= new();
        set => gimportHiddenDocumentField = value;
      }

      /// <summary>
      /// A value of GimportHiddenField.
      /// </summary>
      [JsonPropertyName("gimportHiddenField")]
      public Field GimportHiddenField
      {
        get => gimportHiddenField ??= new();
        set => gimportHiddenField = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private DocumentField gimportHiddenDocumentField;
      private Field gimportHiddenField;
    }

    /// <summary>
    /// A value of FromPromptDocument.
    /// </summary>
    [JsonPropertyName("fromPromptDocument")]
    public Document FromPromptDocument
    {
      get => fromPromptDocument ??= new();
      set => fromPromptDocument = value;
    }

    /// <summary>
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
    }

    /// <summary>
    /// A value of FilterPreviousDocument.
    /// </summary>
    [JsonPropertyName("filterPreviousDocument")]
    public Document FilterPreviousDocument
    {
      get => filterPreviousDocument ??= new();
      set => filterPreviousDocument = value;
    }

    /// <summary>
    /// A value of FilterStandard.
    /// </summary>
    [JsonPropertyName("filterStandard")]
    public Standard FilterStandard
    {
      get => filterStandard ??= new();
      set => filterStandard = value;
    }

    /// <summary>
    /// A value of FilterDocumentField.
    /// </summary>
    [JsonPropertyName("filterDocumentField")]
    public DocumentField FilterDocumentField
    {
      get => filterDocumentField ??= new();
      set => filterDocumentField = value;
    }

    /// <summary>
    /// A value of FilterPreviousDocumentField.
    /// </summary>
    [JsonPropertyName("filterPreviousDocumentField")]
    public DocumentField FilterPreviousDocumentField
    {
      get => filterPreviousDocumentField ??= new();
      set => filterPreviousDocumentField = value;
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

    /// <summary>
    /// A value of FromPromptField.
    /// </summary>
    [JsonPropertyName("fromPromptField")]
    public Field FromPromptField
    {
      get => fromPromptField ??= new();
      set => fromPromptField = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CodeValue Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ZdelImportPreviousFilter.
    /// </summary>
    [JsonPropertyName("zdelImportPreviousFilter")]
    public Field ZdelImportPreviousFilter
    {
      get => zdelImportPreviousFilter ??= new();
      set => zdelImportPreviousFilter = value;
    }

    /// <summary>
    /// A value of ZdelImportFilter.
    /// </summary>
    [JsonPropertyName("zdelImportFilter")]
    public Field ZdelImportFilter
    {
      get => zdelImportFilter ??= new();
      set => zdelImportFilter = value;
    }

    private Document fromPromptDocument;
    private Document filterDocument;
    private Document filterPreviousDocument;
    private Standard filterStandard;
    private DocumentField filterDocumentField;
    private DocumentField filterPreviousDocumentField;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard scrolling;
    private NextTranInfo hidden;
    private Standard standard;
    private Field fromPromptField;
    private CodeValue zdel;
    private Field zdelImportPreviousFilter;
    private Field zdelImportFilter;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GdocumentField.
      /// </summary>
      [JsonPropertyName("gdocumentField")]
      public DocumentField GdocumentField
      {
        get => gdocumentField ??= new();
        set => gdocumentField = value;
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
      /// A value of GexportPreviousDocumentField.
      /// </summary>
      [JsonPropertyName("gexportPreviousDocumentField")]
      public DocumentField GexportPreviousDocumentField
      {
        get => gexportPreviousDocumentField ??= new();
        set => gexportPreviousDocumentField = value;
      }

      /// <summary>
      /// A value of GexportPreviousField.
      /// </summary>
      [JsonPropertyName("gexportPreviousField")]
      public Field GexportPreviousField
      {
        get => gexportPreviousField ??= new();
        set => gexportPreviousField = value;
      }

      /// <summary>
      /// A value of GexportFieldPrompt.
      /// </summary>
      [JsonPropertyName("gexportFieldPrompt")]
      public Standard GexportFieldPrompt
      {
        get => gexportFieldPrompt ??= new();
        set => gexportFieldPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common gcommon;
      private DocumentField gdocumentField;
      private Field gfield;
      private DocumentField gexportPreviousDocumentField;
      private Field gexportPreviousField;
      private Standard gexportFieldPrompt;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportHiddenDocumentField.
      /// </summary>
      [JsonPropertyName("gexportHiddenDocumentField")]
      public DocumentField GexportHiddenDocumentField
      {
        get => gexportHiddenDocumentField ??= new();
        set => gexportHiddenDocumentField = value;
      }

      /// <summary>
      /// A value of GexportHiddenField.
      /// </summary>
      [JsonPropertyName("gexportHiddenField")]
      public Field GexportHiddenField
      {
        get => gexportHiddenField ??= new();
        set => gexportHiddenField = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private DocumentField gexportHiddenDocumentField;
      private Field gexportHiddenField;
    }

    /// <summary>
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
    }

    /// <summary>
    /// A value of FilterPreviousDocument.
    /// </summary>
    [JsonPropertyName("filterPreviousDocument")]
    public Document FilterPreviousDocument
    {
      get => filterPreviousDocument ??= new();
      set => filterPreviousDocument = value;
    }

    /// <summary>
    /// A value of FilterStandard.
    /// </summary>
    [JsonPropertyName("filterStandard")]
    public Standard FilterStandard
    {
      get => filterStandard ??= new();
      set => filterStandard = value;
    }

    /// <summary>
    /// A value of FilterDocumentField.
    /// </summary>
    [JsonPropertyName("filterDocumentField")]
    public DocumentField FilterDocumentField
    {
      get => filterDocumentField ??= new();
      set => filterDocumentField = value;
    }

    /// <summary>
    /// A value of FilterPreviousDocumentField.
    /// </summary>
    [JsonPropertyName("filterPreviousDocumentField")]
    public DocumentField FilterPreviousDocumentField
    {
      get => filterPreviousDocumentField ??= new();
      set => filterPreviousDocumentField = value;
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

    /// <summary>
    /// A value of ZdelExportFilter.
    /// </summary>
    [JsonPropertyName("zdelExportFilter")]
    public Field ZdelExportFilter
    {
      get => zdelExportFilter ??= new();
      set => zdelExportFilter = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public Code Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ZdelExportSelected.
    /// </summary>
    [JsonPropertyName("zdelExportSelected")]
    public Field ZdelExportSelected
    {
      get => zdelExportSelected ??= new();
      set => zdelExportSelected = value;
    }

    /// <summary>
    /// A value of ZdelExportPreviousFilter.
    /// </summary>
    [JsonPropertyName("zdelExportPreviousFilter")]
    public Field ZdelExportPreviousFilter
    {
      get => zdelExportPreviousFilter ??= new();
      set => zdelExportPreviousFilter = value;
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

    private Document filterDocument;
    private Document filterPreviousDocument;
    private Standard filterStandard;
    private DocumentField filterDocumentField;
    private DocumentField filterPreviousDocumentField;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard scrolling;
    private NextTranInfo hidden;
    private Standard standard;
    private Field zdelExportFilter;
    private Code zdel;
    private Field zdelExportSelected;
    private Field zdelExportPreviousFilter;
    private Code code;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of PageStartKeyDocumentField.
    /// </summary>
    [JsonPropertyName("pageStartKeyDocumentField")]
    public DocumentField PageStartKeyDocumentField
    {
      get => pageStartKeyDocumentField ??= new();
      set => pageStartKeyDocumentField = value;
    }

    /// <summary>
    /// A value of PageStartKeyField.
    /// </summary>
    [JsonPropertyName("pageStartKeyField")]
    public Field PageStartKeyField
    {
      get => pageStartKeyField ??= new();
      set => pageStartKeyField = value;
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
    /// A value of ZdelLocalValidCode.
    /// </summary>
    [JsonPropertyName("zdelLocalValidCode")]
    public Common ZdelLocalValidCode
    {
      get => zdelLocalValidCode ??= new();
      set => zdelLocalValidCode = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CodeValue Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private Common prompt;
    private DateWorkArea max;
    private DocumentField pageStartKeyDocumentField;
    private Field pageStartKeyField;
    private Common select;
    private Common zdelLocalValidCode;
    private CodeValue zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    private Document document;
    private DocumentField documentField;
    private Field field;
  }
#endregion
}

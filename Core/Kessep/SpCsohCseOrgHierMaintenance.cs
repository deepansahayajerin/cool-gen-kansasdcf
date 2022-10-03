// Program: SP_CSOH_CSE_ORG_HIER_MAINTENANCE, ID: 371780024, model: 746.
// Short name: SWECSOHP
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
/// A program: SP_CSOH_CSE_ORG_HIER_MAINTENANCE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCsohCseOrgHierMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CSOH_CSE_ORG_HIER_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCsohCseOrgHierMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCsohCseOrgHierMaintenance.
  /// </summary>
  public SpCsohCseOrgHierMaintenance(IContext context, Import import,
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
    // Date      Developer         Request #  	Description
    // --------  ----------------  ---------  	------------------------
    // 04/13/95  J KEMP			CREATE TEMPLATE
    // 05/15/95  R GREY     			MODIFY
    // 02/15/96  A HACKLER  			RETRO FITS
    // 04/22/96  S KONKADER 			Removed RETURN logic, changed exit state when
    // 					County is Parent in hierarchy.  Added logic
    // 					for header/detail change before add/del
    // 05/28/96  J Rookard			Removed validation routine which checked for
    // 					existence of County services prior to
    // 					deletion of a county from a hierarchy.
    // 					Removing a county from a Region's hierarchy
    // 					does not impact the county services provided
    // 					to a county through a CSE Office.
    // ??/??/??  R. Marchman			Add new security/next tran.
    // 10/01/98  Anita Massey			Removed all logic referring to "D" Division.
    // 					CSE business requirements have changed and
    // 					they no longer have divisions in their
    // 					heirarchy.   Added code to give an  error
    // 					message on multiple prompts on a PF4 list
    // 					when the prompt is parent and child.
    // 09/02/11  GVandy	   CQ29124	Modifications to support multiple types of
    // 					user defined hierarchies and general
    // 					restructuring/cleanup.
    // ***************************************************************************************
    // ********************************************
    // * Set initial EXIT STATE.
    // *
    // * Move all IMPORTs to EXPORTs.
    // ********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      // *** per the user SME, Sana Beall, she wants to be able to signoff at 
      // any point in time.
      UseScCabSignoff();

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Parent.Assign(import.Parent);
    export.ParentHidden.Assign(import.ParentHidden);
    export.PromptParentCd.Flag = import.PromptParentCd.Flag;
    export.CseOrganizationRelationship.ReasonCode =
      import.CseOrganizationRelationship.ReasonCode;
    export.HiddenCseOrganizationRelationship.ReasonCode =
      import.HiddenCseOrganizationRelationship.ReasonCode;
    export.PromptHierarchyType.Flag = import.PromptHierarchyType.Flag;
    export.Hierarchy.Description = import.Hierarchy.Description;
    export.StartingFilter.Code = import.StartingFilter.Code;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Sel.SelectChar = import.Import1.Item.Sel.SelectChar;

      if (AsChar(export.Export1.Item.Sel.SelectChar) == '*')
      {
        export.Export1.Update.Sel.SelectChar = "";
      }

      export.Export1.Update.ChildTypePrompt.Flag =
        import.Import1.Item.ChildTypePrompt.Flag;
      export.Export1.Update.CseOrganization.Assign(
        import.Import1.Item.CseOrganization);
      export.Export1.Update.Hidden.Assign(import.Import1.Item.Hidden);
      export.Export1.Next();
    }

    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        // ***if the next tran info is not equal to spaces, this implies the 
        // user requested a next tran action. now validate
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ***  this is where you set your export value to the export hidden next 
      // tran values if the user is comming into this procedure on a next tran
      // action.
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // *** this is where you set your command to do whatever is necessary to 
      // do on a flow from the menu, maybe just escape....
      // *** You should get this information from the Dialog Flow Diagram.  It 
      // is the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    if (Equal(global.Command, "RTLIST") || Equal(global.Command, "RLCVAL") || Equal
      (global.Command, "ENTER"))
    {
    }
    else
    {
      // ***  to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // ********************************************
    // * Validate processing rules
    // *
    // * Determine Selection.
    // *
    // * If user has done an add, they must
    // * display before another add/upd/del
    // ********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE"))
    {
      // ********************************************
      // * Must Display before Add or Delete
      // ********************************************
      if (!Equal(export.Parent.Code, export.ParentHidden.Code) || IsEmpty
        (export.Parent.Code) || AsChar(export.Parent.Type1) != AsChar
        (export.ParentHidden.Type1) || IsEmpty(export.Parent.Type1) || !
        Equal(export.CseOrganizationRelationship.ReasonCode,
        export.HiddenCseOrganizationRelationship.ReasonCode) || IsEmpty
        (export.CseOrganizationRelationship.ReasonCode))
      {
        var field1 = GetField(export.Parent, "code");

        field1.Error = true;

        var field2 = GetField(export.Parent, "type1");

        field2.Error = true;

        var field3 = GetField(export.CseOrganizationRelationship, "reasonCode");

        field3.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      if (Equal(global.Command, "DELETE"))
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.CseOrganization.Type1) != AsChar
            (export.Export1.Item.Hidden.Type1) || !
            Equal(export.Export1.Item.CseOrganization.Code,
            export.Export1.Item.Hidden.Code))
          {
            var field = GetField(export.Export1.Item.CseOrganization, "code");

            field.Error = true;

            ExitState = "ACO_NE0000_NEW_KEY_ON_DELETE";
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ********************************************
      // * Mandatory Child Organization (group) info
      // ********************************************
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
        {
          if (IsEmpty(export.Export1.Item.CseOrganization.Code))
          {
            var field = GetField(export.Export1.Item.CseOrganization, "code");

            field.Error = true;

            if (Equal(global.Command, "ADD"))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
            else
            {
              ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
            }
          }
        }
        else
        {
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.Common.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.Sel.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            break;
          default:
            var field = GetField(export.Export1.Item.Sel, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      // ******************************************
      // * Validate Child CSE Organization types.
      // ******************************************
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
        {
          if (IsEmpty(export.Export1.Item.CseOrganization.Type1))
          {
            export.Export1.Update.CseOrganization.Type1 =
              Substring(export.CseOrganizationRelationship.ReasonCode, 2, 1);
          }
          else if (AsChar(export.Export1.Item.CseOrganization.Type1) != CharAt
            (export.CseOrganizationRelationship.ReasonCode, 2))
          {
            var field1 = GetField(export.Export1.Item.CseOrganization, "type1");

            field1.Error = true;

            var field2 =
              GetField(export.CseOrganizationRelationship, "reasonCode");

            field2.Error = true;

            ExitState = "SP0000_INVALID_CHILD_HIERARCHY";
          }
        }
        else
        {
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "RTLIST"))
    {
      if (AsChar(export.PromptParentCd.Flag) == 'S')
      {
        // ********************************************
        // * Return from selecting a Parent
        // * Organization and force a display.
        // ********************************************
        export.PromptParentCd.Flag = "";
        export.StartingFilter.Code = "";

        var field = GetField(export.PromptParentCd, "flag");

        field.Protected = false;
        field.Focused = true;

        if (!IsEmpty(import.FromCsor.Type1))
        {
          export.Parent.Type1 = import.FromCsor.Type1;
          export.Parent.Code = import.FromCsor.Code;
          export.Parent.Name = import.FromCsor.Name;
          global.Command = "DISPLAY";
        }
      }
      else
      {
        // ********************************************
        // * Return from selecting a Child Organization.
        // ********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.ChildTypePrompt.Flag) == 'S')
          {
            export.Export1.Update.ChildTypePrompt.Flag = "";

            var field = GetField(export.Export1.Item.ChildTypePrompt, "flag");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.FromCsor.Type1))
            {
              export.Export1.Update.CseOrganization.Type1 =
                import.FromCsor.Type1;
              export.Export1.Update.CseOrganization.Code = import.FromCsor.Code;
              export.Export1.Update.CseOrganization.Name = import.FromCsor.Name;
              export.Export1.Update.Sel.SelectChar = "S";
            }
          }
        }
      }
    }

    // ********************************************
    // * Main Case of Command
    // ********************************************
    switch(TrimEnd(global.Command))
    {
      case "RTLIST":
        break;
      case "RLCVAL":
        // ********************************************
        // * Return from selecting a Hierarchy Code Value
        // ********************************************
        if (AsChar(export.PromptHierarchyType.Flag) == 'S')
        {
          export.PromptHierarchyType.Flag = "";

          var field = GetField(export.PromptHierarchyType, "flag");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.FromCdvl.Cdvalue))
          {
            export.CseOrganizationRelationship.ReasonCode =
              import.FromCdvl.Cdvalue;
            export.Hierarchy.Description = import.FromCdvl.Description;
          }
        }

        break;
      case "DISPLAY":
        // ********************************************
        // * Clear export group so it will be empty
        // * if any of the edits below fail.
        // ********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          export.Export1.Update.CseOrganization.
            Assign(local.NullCseOrganization);
          export.Export1.Update.Hidden.Assign(local.NullCseOrganization);
          export.Export1.Update.ChildTypePrompt.Flag = local.NullCommon.Flag;
          export.Export1.Update.Sel.SelectChar = local.NullCommon.SelectChar;
        }

        // ********************************************
        // * Check for mandatory fields.
        // ********************************************
        if (IsEmpty(export.CseOrganizationRelationship.ReasonCode))
        {
          var field =
            GetField(export.CseOrganizationRelationship, "reasonCode");

          field.Error = true;

          export.Hierarchy.Description =
            Spaces(CodeValue.Description_MaxLength);
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsEmpty(export.Parent.Type1))
        {
          var field = GetField(export.Parent, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsEmpty(export.Parent.Code))
        {
          var field = GetField(export.Parent, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ********************************************
        // * Validate Parent Organization Type.
        // ********************************************
        local.Code.CodeName = "CSE ORGANIZATION TYPE";
        local.CodeValue.Cdvalue = export.Parent.Type1;
        UseCabValidateCodeValue2();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
        }
        else
        {
          var field = GetField(export.Parent, "type1");

          field.Error = true;

          ExitState = "INVALID_TYPE_CODE";

          return;
        }

        // ******************************************
        // * Validate Relationship Reason Code.
        // ******************************************
        local.Code.CodeName = "CSE ORGANIZATION HIERARCHY";
        local.CodeValue.Cdvalue = import.CseOrganizationRelationship.ReasonCode;
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
          export.Hierarchy.Description = local.CodeValue.Description;
        }
        else
        {
          export.Hierarchy.Description =
            Spaces(CodeValue.Description_MaxLength);

          var field =
            GetField(export.CseOrganizationRelationship, "reasonCode");

          field.Error = true;

          ExitState = "INVALID_TYPE_CODE";

          return;
        }

        // ******************************************
        // * Validate Parent CSE Organization type
        // * against the relationship reason code.
        // ******************************************
        if (AsChar(export.Parent.Type1) != CharAt
          (export.CseOrganizationRelationship.ReasonCode, 1))
        {
          var field1 = GetField(export.Parent, "type1");

          field1.Error = true;

          var field2 =
            GetField(export.CseOrganizationRelationship, "reasonCode");

          field2.Error = true;

          ExitState = "SP0000_INVALID_PARENT_ORG_TYPE";

          return;
        }

        // ******************************************
        // * Read for hierarchy info.
        // ******************************************
        UseSpReadOrgHierarchy();

        if (IsExitState("CSE_ORGANIZATION_NF"))
        {
          var field = GetField(export.Parent, "code");

          field.Error = true;

          if (!IsEmpty(export.StartingFilter.Code))
          {
            var field1 = GetField(export.StartingFilter, "code");

            field1.Error = true;
          }

          return;
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        export.ParentHidden.Assign(export.Parent);
        export.HiddenCseOrganizationRelationship.ReasonCode =
          export.CseOrganizationRelationship.ReasonCode;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          export.Export1.Update.Hidden.Assign(
            export.Export1.Item.CseOrganization);
        }

        break;
      case "LIST":
        // *******************************************
        // * Validate selection characters and insure
        // * only one prompt was selected.
        // *******************************************
        local.Common.Count = 0;

        switch(AsChar(export.PromptParentCd.Flag))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            break;
          default:
            var field = GetField(export.PromptParentCd, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(export.PromptHierarchyType.Flag))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            break;
          default:
            var field = GetField(export.PromptHierarchyType, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.ChildTypePrompt.Flag))
          {
            case ' ':
              break;
            case 'S':
              ++local.Common.Count;

              break;
            default:
              var field = GetField(export.Export1.Item.ChildTypePrompt, "flag");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            if (AsChar(export.PromptParentCd.Flag) == 'S')
            {
              // -- Prompt for parent organization.
              export.ToCsor.Type1 = export.Parent.Type1;
              ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";

              return;
            }

            if (AsChar(export.PromptHierarchyType.Flag) == 'S')
            {
              // -- Prompt for hierarchy type.
              export.ToCdvl.CodeName = "CSE ORGANIZATION HIERARCHY";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.ChildTypePrompt.Flag) == 'S')
              {
                // -- Prompt for child organization.
                export.ToCsor.Type1 =
                  Substring(export.CseOrganizationRelationship.ReasonCode, 2, 1);
                  
                ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";

                return;
              }
            }

            break;
          default:
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.ChildTypePrompt.Flag) == 'S')
              {
                var field =
                  GetField(export.Export1.Item.ChildTypePrompt, "flag");

                field.Error = true;
              }
            }

            if (AsChar(export.PromptHierarchyType.Flag) == 'S')
            {
              var field = GetField(export.PromptHierarchyType, "flag");

              field.Error = true;
            }

            if (AsChar(export.PromptParentCd.Flag) == 'S')
            {
              var field = GetField(export.PromptParentCd, "flag");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "ADD":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            UseSpCreateOrgRelationship();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field = GetField(export.Export1.Item.CseOrganization, "code");

              field.Error = true;

              return;
            }

            export.Export1.Update.ChildTypePrompt.Flag = "";
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            export.Export1.Update.Sel.SelectChar = "*";
            export.Export1.Update.ChildTypePrompt.Flag = "";
            export.Export1.Update.Hidden.Assign(
              export.Export1.Item.CseOrganization);
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "DELETE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Sel.SelectChar) == 'S')
          {
            UseSpDeleteCseOrgRelation();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              export.Export1.Update.Sel.SelectChar = "";

              var field = GetField(export.Export1.Item.Sel, "selectChar");

              field.Error = true;

              return;
            }

            export.Export1.Update.ChildTypePrompt.Flag = "";
          }
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          if (AsChar(import.Import1.Item.Sel.SelectChar) == 'S')
          {
            // -- Remove deleted child from export group view.
            export.Export1.Next();

            continue;
          }
          else
          {
            export.Export1.Update.CseOrganization.Assign(
              import.Import1.Item.CseOrganization);
            export.Export1.Update.ChildTypePrompt.Flag =
              import.Import1.Item.ChildTypePrompt.Flag;
            export.Export1.Update.Hidden.Assign(import.Import1.Item.Hidden);
            export.Export1.Update.Sel.SelectChar =
              import.Import1.Item.Sel.SelectChar;
          }

          export.Export1.Next();
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private static void MoveExport1(SpReadOrgHierarchy.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.Sel.SelectChar = source.Sel.SelectChar;
    target.CseOrganization.Assign(source.CseOrganization);
    target.ChildTypePrompt.Flag = source.ChildTypePrompt.Flag;
    target.Hidden.Assign(source.Hidden);
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

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

  private void UseSpCreateOrgRelationship()
  {
    var useImport = new SpCreateOrgRelationship.Import();
    var useExport = new SpCreateOrgRelationship.Export();

    MoveCseOrganization(export.Parent, useImport.Parent);
    MoveCseOrganization(export.Export1.Item.CseOrganization, useImport.Child);
    useImport.CseOrganizationRelationship.ReasonCode =
      export.CseOrganizationRelationship.ReasonCode;

    Call(SpCreateOrgRelationship.Execute, useImport, useExport);

    MoveCseOrganization(useExport.Parent, export.Parent);
    export.Export1.Update.CseOrganization.Assign(useExport.Child);
  }

  private void UseSpDeleteCseOrgRelation()
  {
    var useImport = new SpDeleteCseOrgRelation.Import();
    var useExport = new SpDeleteCseOrgRelation.Export();

    MoveCseOrganization(export.Parent, useImport.Parent);
    MoveCseOrganization(export.Export1.Item.CseOrganization, useImport.Child);

    Call(SpDeleteCseOrgRelation.Execute, useImport, useExport);
  }

  private void UseSpReadOrgHierarchy()
  {
    var useImport = new SpReadOrgHierarchy.Import();
    var useExport = new SpReadOrgHierarchy.Export();

    useImport.Parent.Assign(export.Parent);
    useImport.StartCode.Code = export.StartingFilter.Code;
    useImport.CseOrganizationRelationship.ReasonCode =
      export.CseOrganizationRelationship.ReasonCode;

    Call(SpReadOrgHierarchy.Execute, useImport, useExport);

    export.Parent.Assign(useExport.Parent);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
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
      /// A value of ChildTypePrompt.
      /// </summary>
      [JsonPropertyName("childTypePrompt")]
      public Common ChildTypePrompt
      {
        get => childTypePrompt ??= new();
        set => childTypePrompt = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CseOrganization Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common sel;
      private CseOrganization cseOrganization;
      private Common childTypePrompt;
      private CseOrganization hidden;
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
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    /// <summary>
    /// A value of ParentHidden.
    /// </summary>
    [JsonPropertyName("parentHidden")]
    public CseOrganization ParentHidden
    {
      get => parentHidden ??= new();
      set => parentHidden = value;
    }

    /// <summary>
    /// A value of PromptParentCd.
    /// </summary>
    [JsonPropertyName("promptParentCd")]
    public Common PromptParentCd
    {
      get => promptParentCd ??= new();
      set => promptParentCd = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of HiddenCseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganizationRelationship")]
    public CseOrganizationRelationship HiddenCseOrganizationRelationship
    {
      get => hiddenCseOrganizationRelationship ??= new();
      set => hiddenCseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of PromptHierarchyType.
    /// </summary>
    [JsonPropertyName("promptHierarchyType")]
    public Common PromptHierarchyType
    {
      get => promptHierarchyType ??= new();
      set => promptHierarchyType = value;
    }

    /// <summary>
    /// A value of Hierarchy.
    /// </summary>
    [JsonPropertyName("hierarchy")]
    public CodeValue Hierarchy
    {
      get => hierarchy ??= new();
      set => hierarchy = value;
    }

    /// <summary>
    /// A value of StartingFilter.
    /// </summary>
    [JsonPropertyName("startingFilter")]
    public CseOrganization StartingFilter
    {
      get => startingFilter ??= new();
      set => startingFilter = value;
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
    /// A value of FromCsor.
    /// </summary>
    [JsonPropertyName("fromCsor")]
    public CseOrganization FromCsor
    {
      get => fromCsor ??= new();
      set => fromCsor = value;
    }

    /// <summary>
    /// A value of FromCdvl.
    /// </summary>
    [JsonPropertyName("fromCdvl")]
    public CodeValue FromCdvl
    {
      get => fromCdvl ??= new();
      set => fromCdvl = value;
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

    private Standard standard;
    private CseOrganization parent;
    private CseOrganization parentHidden;
    private Common promptParentCd;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganizationRelationship hiddenCseOrganizationRelationship;
    private Common promptHierarchyType;
    private CodeValue hierarchy;
    private CseOrganization startingFilter;
    private Array<ImportGroup> import1;
    private CseOrganization fromCsor;
    private CodeValue fromCdvl;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
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
      /// A value of ChildTypePrompt.
      /// </summary>
      [JsonPropertyName("childTypePrompt")]
      public Common ChildTypePrompt
      {
        get => childTypePrompt ??= new();
        set => childTypePrompt = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CseOrganization Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common sel;
      private CseOrganization cseOrganization;
      private Common childTypePrompt;
      private CseOrganization hidden;
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
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public CseOrganization Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    /// <summary>
    /// A value of ParentHidden.
    /// </summary>
    [JsonPropertyName("parentHidden")]
    public CseOrganization ParentHidden
    {
      get => parentHidden ??= new();
      set => parentHidden = value;
    }

    /// <summary>
    /// A value of PromptParentCd.
    /// </summary>
    [JsonPropertyName("promptParentCd")]
    public Common PromptParentCd
    {
      get => promptParentCd ??= new();
      set => promptParentCd = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of HiddenCseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganizationRelationship")]
    public CseOrganizationRelationship HiddenCseOrganizationRelationship
    {
      get => hiddenCseOrganizationRelationship ??= new();
      set => hiddenCseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of PromptHierarchyType.
    /// </summary>
    [JsonPropertyName("promptHierarchyType")]
    public Common PromptHierarchyType
    {
      get => promptHierarchyType ??= new();
      set => promptHierarchyType = value;
    }

    /// <summary>
    /// A value of Hierarchy.
    /// </summary>
    [JsonPropertyName("hierarchy")]
    public CodeValue Hierarchy
    {
      get => hierarchy ??= new();
      set => hierarchy = value;
    }

    /// <summary>
    /// A value of StartingFilter.
    /// </summary>
    [JsonPropertyName("startingFilter")]
    public CseOrganization StartingFilter
    {
      get => startingFilter ??= new();
      set => startingFilter = value;
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
    /// A value of ToCsor.
    /// </summary>
    [JsonPropertyName("toCsor")]
    public CseOrganization ToCsor
    {
      get => toCsor ??= new();
      set => toCsor = value;
    }

    /// <summary>
    /// A value of ToCdvl.
    /// </summary>
    [JsonPropertyName("toCdvl")]
    public Code ToCdvl
    {
      get => toCdvl ??= new();
      set => toCdvl = value;
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

    private Standard standard;
    private CseOrganization parent;
    private CseOrganization parentHidden;
    private Common promptParentCd;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganizationRelationship hiddenCseOrganizationRelationship;
    private Common promptHierarchyType;
    private CodeValue hierarchy;
    private CseOrganization startingFilter;
    private Array<ExportGroup> export1;
    private CseOrganization toCsor;
    private Code toCdvl;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullCommon.
    /// </summary>
    [JsonPropertyName("nullCommon")]
    public Common NullCommon
    {
      get => nullCommon ??= new();
      set => nullCommon = value;
    }

    /// <summary>
    /// A value of NullCseOrganization.
    /// </summary>
    [JsonPropertyName("nullCseOrganization")]
    public CseOrganization NullCseOrganization
    {
      get => nullCseOrganization ??= new();
      set => nullCseOrganization = value;
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

    private Common nullCommon;
    private CseOrganization nullCseOrganization;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common common;
  }
#endregion
}

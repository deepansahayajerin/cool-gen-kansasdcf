// Program: SP_CSOR_CSE_ORGANIZATION_MAINT, ID: 371780580, model: 746.
// Short name: SWECSORP
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
/// A program: SP_CSOR_CSE_ORGANIZATION_MAINT.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCsorCseOrganizationMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CSOR_CSE_ORGANIZATION_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCsorCseOrganizationMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCsorCseOrganizationMaint.
  /// </summary>
  public SpCsorCseOrganizationMaint(IContext context, Import import,
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
    // ** DATE      *  DESCRIPTION
    // ** 04/13/95     J. Kemp
    // ** 04/28/95	R. Grey
    // ** 02/15/96     A. HACKLER   RETRO FITS
    // ** 04/23/96     S. Konkader  Added code for AUD w/o display,
    // **                           PF4 w/o S,
    // **                           Detail key changes not allowed on Upd
    // ** 05/28/96     J. Rookard   Added validation routine to prevent     **
    // deletion of CSE Organization when organization **
    // is a parent in the CSE Organization Hierarchy.
    // **  01/03/97	R. Marchman  Add new security/next tran.
    // **  10/01/98     Anita Massey   Removed all logic referring to "D"
    // **                           Division, as CSE business requirements
    // **                           have changed and they no longer have
    // **                           Divisions.
    // **   6/10/99    Anita Massey    changed read property to
    // **
    // 
    // select only
    // ********************************************
    // *******************************************
    // * Set initial EXIT STATE.
    // *
    // * Move all IMPORTS to EXPORTS.
    // *******************************************
    ExitState = "ACO_NN0000_ALL_OK";

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

    export.HidPrevCommand.Command = import.HidPrevCommand.Command;
    export.SearchCodePromt.SearchCode.CodeName =
      import.SearchCodePromt.SearchCode.CodeName;
    export.SearchCodePromt.SearchCodeValue.Assign(
      import.SearchCodePromt.SearchCodeValue);
    export.SearchCodePromt.SearchReturnVal.Count =
      import.SearchCodePromt.SearchReturnVal.Count;
    MoveCseOrganization(import.Search, export.Search);
    export.SelTypePrompt.Flag = import.SelTypePrompt.Flag;
    export.DateWorkArea.Date = import.DateWorkArea.Date;

    export.Export2.Index = 0;
    export.Export2.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export2.IsFull)
      {
        break;
      }

      MoveCommon(import.Import1.Item.Common, export.Export2.Update.Common);
      export.Export2.Update.CseOrganization.Assign(
        import.Import1.Item.CseOrganization);
      export.Export2.Update.Hidden.Assign(import.Import1.Item.Hidden);

      if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
      {
        ++local.Common.Count;
        export.ReturnSelect.Assign(export.Export2.Item.CseOrganization);
      }

      export.Export2.Next();
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
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

    // **** end   group B ****
    if (Equal(global.Command, "RLCVAL"))
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

      // **** end   group C ****
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "RETURN"))
    {
      if (!Equal(global.Command, "RETURN"))
      {
        if (IsEmpty(export.Search.Type1))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

          return;
        }
      }

      // ********************************************
      // * Validate processing rules
      // *
      // * Determine Selections
      // * Display before Update, or Delete
      // ********************************************
      if (Equal(global.Command, "DELETE"))
      {
        local.Common.Count = 0;

        for(export.Export2.Index = 0; export.Export2.Index < export
          .Export2.Count; ++export.Export2.Index)
        {
          switch(AsChar(export.Export2.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.Common.Count;

              if (AsChar(export.Export2.Item.CseOrganization.Type1) != AsChar
                (export.Export2.Item.Hidden.Type1) || !
                Equal(export.Export2.Item.CseOrganization.Code,
                export.Export2.Item.Hidden.Code) || !
                Equal(export.Export2.Item.CseOrganization.Name,
                export.Export2.Item.Hidden.Name) || !
                Equal(export.Export2.Item.CseOrganization.TaxId,
                export.Export2.Item.Hidden.TaxId) || export
                .Export2.Item.CseOrganization.TaxSuffix.GetValueOrDefault() != export
                .Export2.Item.Hidden.TaxSuffix.GetValueOrDefault())
              {
                var field1 = GetField(export.Export2.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

                return;
              }

              break;
            default:
              var field = GetField(export.Export2.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }
        }

        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }

      local.Common.Count = 0;

      for(export.Export2.Index = 0; export.Export2.Index < export
        .Export2.Count; ++export.Export2.Index)
      {
        if (Equal(global.Command, "UPDATE"))
        {
          local.Common.Count = 0;

          if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
          {
            if (AsChar(export.Export2.Item.CseOrganization.Type1) == AsChar
              (export.Export2.Item.Hidden.Type1) && Equal
              (export.Export2.Item.CseOrganization.Code,
              export.Export2.Item.Hidden.Code) && Equal
              (export.Export2.Item.CseOrganization.Name,
              export.Export2.Item.Hidden.Name) && Equal
              (export.Export2.Item.CseOrganization.TaxId,
              export.Export2.Item.Hidden.TaxId) && export
              .Export2.Item.CseOrganization.TaxSuffix.GetValueOrDefault() == export
              .Export2.Item.Hidden.TaxSuffix.GetValueOrDefault())
            {
              var field = GetField(export.Export2.Item.Common, "selectChar");

              field.Error = true;

              export.Export2.Update.Common.SelectChar = "";
              ExitState = "INVALID_UPDATE";

              return;
            }

            if (AsChar(export.Export2.Item.CseOrganization.Type1) != AsChar
              (export.Export2.Item.Hidden.Type1))
            {
              var field =
                GetField(export.Export2.Item.CseOrganization, "type1");

              field.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (!Equal(export.Export2.Item.CseOrganization.Code,
              export.Export2.Item.Hidden.Code))
            {
              var field = GetField(export.Export2.Item.CseOrganization, "code");

              field.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        switch(AsChar(export.Export2.Item.Common.SelectChar))
        {
          case '*':
            export.Export2.Update.Common.SelectChar = "";

            break;
          case 'S':
            ++local.Common.Count;

            // *******************************************
            // * Validate key data.
            // *******************************************
            if (Equal(global.Command, "ADD") || Equal
              (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
            {
              if (IsEmpty(export.Export2.Item.CseOrganization.Name))
              {
                var field1 =
                  GetField(export.Export2.Item.CseOrganization, "name");

                field1.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }

              if (IsEmpty(export.Export2.Item.CseOrganization.Type1))
              {
                var field1 =
                  GetField(export.Export2.Item.CseOrganization, "type1");

                field1.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }

              if (IsEmpty(export.Export2.Item.CseOrganization.Code))
              {
                var field1 =
                  GetField(export.Export2.Item.CseOrganization, "code");

                field1.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
            {
              if (AsChar(export.Export2.Item.CseOrganization.Type1) == 'S' || AsChar
                (export.Export2.Item.CseOrganization.Type1) == 'R')
              {
                if (!IsEmpty(export.Export2.Item.CseOrganization.TaxId))
                {
                  var field1 =
                    GetField(export.Export2.Item.CseOrganization, "taxId");

                  field1.Error = true;

                  ExitState = "INVALID_TAX_ID";

                  goto Test1;
                }

                if (export.Export2.Item.CseOrganization.TaxSuffix.
                  GetValueOrDefault() > 0)
                {
                  var field1 =
                    GetField(export.Export2.Item.CseOrganization, "taxSuffix");

                  field1.Error = true;

                  ExitState = "INVALID_TAX_SUFFIX";
                }
              }
            }

Test1:

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (Equal(global.Command, "ADD"))
            {
              // *******************************************
              // * Validate CSE Organization Type
              // *******************************************
              export.SearchCodePromt.SearchCode.CodeName =
                "CSE ORGANIZATION TYPE";
              export.SearchCodePromt.SearchCodeValue.Cdvalue =
                export.Export2.Item.CseOrganization.Type1;
              UseCabValidateCodeValue2();

              if (export.SearchCodePromt.SearchReturnVal.Count > 0)
              {
                var field1 =
                  GetField(export.Export2.Item.CseOrganization, "type1");

                field1.Error = true;

                ExitState = "INVALID_TYPE_CODE";

                return;
              }
            }

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Export2.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test2;
        }
      }

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";
      }
      else if (local.Common.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
      }
    }

Test2:

    // *******************************************
    // * Main CASE OF COMMAND
    // *******************************************
    if (Equal(global.Command, "RLCVAL"))
    {
      // *******************************************
      // * Return from Selecting a Code Value from
      // * the screen Header and force a display.
      // *******************************************
      if (!IsEmpty(export.SelTypePrompt.Flag))
      {
        if (!IsEmpty(import.SearchCodePromt.SearchCodeValue.Cdvalue))
        {
          MoveCseOrganization(import.Search, export.Search);
          export.Search.Type1 = import.SearchCodePromt.SearchCodeValue.Cdvalue;
        }

        global.Command = "DISPLAY";
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RLCVAL":
        // *******************************************
        // * Return from Selecting a Code Value in the
        // * scrollable area of the screen.
        // *******************************************
        export.Export2.Index = 0;
        export.Export2.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export2.IsFull)
          {
            break;
          }

          export.Export2.Update.CseOrganization.Assign(
            import.Import1.Item.CseOrganization);
          MoveCommon(import.Import1.Item.Common, export.Export2.Update.Common);

          if (AsChar(export.Export2.Item.Common.Flag) == 'S')
          {
            var field = GetField(export.Export2.Item.Common, "flag");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.SearchCodePromt.SearchCodeValue.Cdvalue))
            {
              export.Export2.Update.CseOrganization.Type1 =
                import.SearchCodePromt.SearchCodeValue.Cdvalue;
            }

            export.Export2.Update.Common.Flag = "";
          }

          export.Export2.Next();
        }

        break;
      case "DISPLAY":
        if (IsEmpty(export.Search.Type1))
        {
          var field = GetField(export.Search, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          export.SelTypePrompt.Flag = "";

          for(export.Export2.Index = 0; export.Export2.Index < export
            .Export2.Count; ++export.Export2.Index)
          {
            export.Export2.Update.Common.SelectChar = "";
            export.Export2.Update.Common.Flag = "";
          }

          // *******************************************
          // * Validate CSE Organization Type
          // *******************************************
          if (!IsEmpty(import.SelTypePrompt.Flag))
          {
            export.SelTypePrompt.Flag = "";

            var field = GetField(export.SelTypePrompt, "flag");

            field.Protected = false;
            field.Focused = true;
          }

          export.SearchCodePromt.SearchCode.CodeName = "CSE ORGANIZATION TYPE";
          export.SearchCodePromt.SearchCodeValue.Cdvalue = export.Search.Type1;
          UseCabValidateCodeValue1();

          if (export.SearchCodePromt.SearchReturnVal.Count > 0)
          {
            var field = GetField(export.Search, "type1");

            field.Error = true;

            ExitState = "INVALID_TYPE_CODE";

            break;
          }
          else
          {
            UseSpReadCseOrg();
          }
        }

        for(export.Export2.Index = 0; export.Export2.Index < export
          .Export2.Count; ++export.Export2.Index)
        {
          if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
          {
          }
          else if (!IsEmpty(export.Export2.Item.CseOrganization.Type1))
          {
            var field1 = GetField(export.Export2.Item.CseOrganization, "type1");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Export2.Item.Common, "flag");

            field2.Color = "cyan";
            field2.Protected = true;
          }
        }

        if (export.Export2.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST":
        // *********************************************
        // * Check to see if we have prompted for an
        // * ORGANIZATION-TYPE selection list from the
        // * Start Org Type Prompt.
        // *********************************************
        switch(AsChar(export.SelTypePrompt.Flag))
        {
          case ' ':
            // *********************************************
            // * Check to see if we have prompted for an
            // * ORGANIZATION-TYPE selection list from the
            // * scrollable list.
            // *********************************************
            local.Common.Count = 0;

            for(export.Export2.Index = 0; export.Export2.Index < export
              .Export2.Count; ++export.Export2.Index)
            {
              if (!IsEmpty(export.Export2.Item.Common.Flag))
              {
                if (AsChar(export.Export2.Item.Common.Flag) == 'S')
                {
                  ++local.Common.Count;
                }
                else
                {
                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                  var field1 = GetField(export.Export2.Item.Common, "flag");

                  field1.Error = true;
                }
              }
            }

            switch(local.Common.Count)
            {
              case 0:
                if (IsEmpty(export.SelTypePrompt.Flag))
                {
                  if (IsEmpty(export.Search.Type1))
                  {
                    var field1 = GetField(export.SelTypePrompt, "flag");

                    field1.Error = true;
                  }

                  ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                }

                break;
              case 1:
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                return;
              default:
                for(export.Export2.Index = 0; export.Export2.Index < export
                  .Export2.Count; ++export.Export2.Index)
                {
                  if (AsChar(export.Export2.Item.Common.Flag) == 'S')
                  {
                    var field1 = GetField(export.Export2.Item.Common, "flag");

                    field1.Error = true;

                    ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
                  }
                }

                break;
            }

            break;
          case 'S':
            for(export.Export2.Index = 0; export.Export2.Index < export
              .Export2.Count; ++export.Export2.Index)
            {
              if (AsChar(export.Export2.Item.Common.Flag) == 'S')
              {
                ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

                var field1 = GetField(export.SelTypePrompt, "flag");

                field1.Error = true;

                return;
              }
            }

            export.SearchCodePromt.SearchCode.CodeName =
              "CSE ORGANIZATION TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          default:
            var field = GetField(export.SelTypePrompt, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        break;
      case "ADD":
        for(export.Export2.Index = 0; export.Export2.Index < export
          .Export2.Count; ++export.Export2.Index)
        {
          if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
          {
            // ***  Note the type being added must match the filter type at the 
            // top of the screen.
            if (AsChar(export.Search.Type1) == AsChar
              (export.Export2.Item.CseOrganization.Type1))
            {
              UseSpCreateCseOrganization();

              if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
              {
                export.Export2.Update.Common.SelectChar = "*";
                export.Export2.Update.Common.Flag = "";

                if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                {
                  export.Export2.Update.Common.SelectChar = "";

                  var field =
                    GetField(export.Export2.Item.Common, "selectChar");

                  field.Error = true;
                }
              }
            }
            else
            {
              ExitState = "INVALID_COMBINATION";

              var field1 = GetField(export.Search, "type1");

              field1.Error = true;

              var field2 =
                GetField(export.Export2.Item.CseOrganization, "type1");

              field2.Error = true;
            }
          }
        }

        break;
      case "UPDATE":
        for(export.Export2.Index = 0; export.Export2.Index < export
          .Export2.Count; ++export.Export2.Index)
        {
          if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
          {
            UseSpUpdateCseOrganization();

            if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
            {
              export.Export2.Update.Common.SelectChar = "*";
              export.Export2.Update.Common.Flag = "";

              if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
                export.Export2.Update.Common.SelectChar = "";

                var field = GetField(export.Export2.Item.Common, "selectChar");

                field.Error = true;
              }
            }
          }
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        // ---------------------------------------------------------------
        // return back to the calling procedure.  A selection is not
        // required, but if made, only one selection is allowed.
        // --------------------------------------------------------------
        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DELETE":
        for(export.Export2.Index = 0; export.Export2.Index < export
          .Export2.Count; ++export.Export2.Index)
        {
          if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
          {
            // Determine if the organization targeted for deletion is a parent 
            // in the CSE Organization hierarchy.  If so, disallow the deletion.
            if (ReadCseOrganization2())
            {
              if (ReadCseOrganization1())
              {
                var field = GetField(export.Export2.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_ORG_IS_PARENT_N_HIERARCHY";

                return;
              }
            }
            else
            {
              ExitState = "CSE_ORGANIZATION_NF";

              var field1 =
                GetField(export.Export2.Item.CseOrganization, "type1");

              field1.Error = true;

              var field2 =
                GetField(export.Export2.Item.CseOrganization, "code");

              field2.Error = true;

              var field3 = GetField(export.Export2.Item.Common, "selectChar");

              field3.Error = true;

              return;
            }

            if (AsChar(export.Export2.Item.CseOrganization.Type1) == 'C')
            {
              // *********************************************
              // * Determine if this Organization is a County
              // * and, if so, whether any service are already
              // * assigned to it.  In which case, deletion is
              // * not allowed.
              // *********************************************
              if (ReadCountyService())
              {
                var field = GetField(export.Export2.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "DELETE_CANCEL_CTSV";

                return;
              }

              if (ReadOffice())
              {
                var field = GetField(export.Export2.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_COUNTY_ASSOC_TO_OFFICE";

                return;
              }
            }

            if (IsExitState("ACO_NN0000_ALL_OK") && AsChar
              (export.Export2.Item.Common.SelectChar) == 'S')
            {
              UseSpDeleteCseOrg();
            }

            if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
            {
              export.Export2.Update.Common.SelectChar = "*";

              if (!IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
              {
                export.Export2.Update.Common.SelectChar = "";

                var field = GetField(export.Export2.Item.Common, "selectChar");

                field.Error = true;
              }

              export.Export2.Update.Common.Flag = "";
            }
          }
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    for(export.Export2.Index = 0; export.Export2.Index < export.Export2.Count; ++
      export.Export2.Index)
    {
      if (AsChar(export.Export2.Item.Common.SelectChar) == 'S')
      {
      }
      else if (!IsEmpty(export.Export2.Item.CseOrganization.Type1))
      {
        var field1 = GetField(export.Export2.Item.CseOrganization, "type1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Export2.Item.Common, "flag");

        field2.Color = "cyan";
        field2.Protected = true;
      }
    }

    export.HidPrevCommand.Command = global.Command;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private static void MoveExport1ToExport2(SpReadCseOrg.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveCommon(source.Common, target.Common);
    target.CseOrganization.Assign(source.CseOrganization);
    target.Hidden.Assign(source.Hidden);
    target.HidVerifyDel.SystemGeneratedIdentifier =
      source.HidVerifyDel.SystemGeneratedIdentifier;
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

    useImport.Code.CodeName = export.SearchCodePromt.SearchCode.CodeName;
    useImport.CodeValue.Cdvalue =
      export.SearchCodePromt.SearchCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    export.SearchCodePromt.SearchCodeValue.Assign(useExport.CodeValue);
    export.SearchCodePromt.SearchReturnVal.Count = useExport.ReturnCode.Count;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = export.SearchCodePromt.SearchCode.CodeName;
    useImport.CodeValue.Cdvalue =
      export.SearchCodePromt.SearchCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    export.SearchCodePromt.SearchReturnVal.Count = useExport.ReturnCode.Count;
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

  private void UseSpCreateCseOrganization()
  {
    var useImport = new SpCreateCseOrganization.Import();
    var useExport = new SpCreateCseOrganization.Export();

    useImport.CseOrganization.Assign(export.Export2.Item.CseOrganization);

    Call(SpCreateCseOrganization.Execute, useImport, useExport);

    export.Export2.Update.CseOrganization.Assign(useExport.CseOrganization);
  }

  private void UseSpDeleteCseOrg()
  {
    var useImport = new SpDeleteCseOrg.Import();
    var useExport = new SpDeleteCseOrg.Export();

    MoveCseOrganization(export.Export2.Item.CseOrganization,
      useImport.CseOrganization);

    Call(SpDeleteCseOrg.Execute, useImport, useExport);
  }

  private void UseSpReadCseOrg()
  {
    var useImport = new SpReadCseOrg.Import();
    var useExport = new SpReadCseOrg.Export();

    useImport.Search.Assign(export.Search);

    Call(SpReadCseOrg.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export2, MoveExport1ToExport2);
  }

  private void UseSpUpdateCseOrganization()
  {
    var useImport = new SpUpdateCseOrganization.Import();
    var useExport = new SpUpdateCseOrganization.Export();

    useImport.CseOrganization.Assign(export.Export2.Item.CseOrganization);

    Call(SpUpdateCseOrganization.Execute, useImport, useExport);

    export.Export2.Update.CseOrganization.Assign(useExport.CseOrganization);
  }

  private bool ReadCountyService()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService",
      (db, command) =>
      {
        db.SetNullableString(command, "cogTypeCode", entities.Parent.Type1);
        db.SetNullableString(command, "cogCode", entities.Parent.Code);
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 1);
        entities.CountyService.CogCode = db.GetNullableString(reader, 2);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadCseOrganization1()
  {
    entities.Child.Populated = false;

    return Read("ReadCseOrganization1",
      (db, command) =>
      {
        db.SetString(command, "cogChildType", entities.Parent.Type1);
        db.SetString(command, "cogChildCode", entities.Parent.Code);
      },
      (db, reader) =>
      {
        entities.Child.Code = db.GetString(reader, 0);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.Child.Populated = true;
      });
  }

  private bool ReadCseOrganization2()
  {
    entities.Parent.Populated = false;

    return Read("ReadCseOrganization2",
      (db, command) =>
      {
        db.SetString(
          command, "organztnId", export.Export2.Item.CseOrganization.Code);
        db.SetString(
          command, "typeCode", export.Export2.Item.CseOrganization.Type1);
      },
      (db, reader) =>
      {
        entities.Parent.Code = db.GetString(reader, 0);
        entities.Parent.Type1 = db.GetString(reader, 1);
        entities.Parent.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetNullableString(command, "cogTypeCode", entities.Parent.Type1);
        db.SetNullableString(command, "cogCode", entities.Parent.Code);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 1);
        entities.Office.CogCode = db.GetNullableString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
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
    /// <summary>A SearchCodePromtGroup group.</summary>
    [Serializable]
    public class SearchCodePromtGroup
    {
      /// <summary>
      /// A value of SearchCode.
      /// </summary>
      [JsonPropertyName("searchCode")]
      public Code SearchCode
      {
        get => searchCode ??= new();
        set => searchCode = value;
      }

      /// <summary>
      /// A value of SearchCodeValue.
      /// </summary>
      [JsonPropertyName("searchCodeValue")]
      public CodeValue SearchCodeValue
      {
        get => searchCodeValue ??= new();
        set => searchCodeValue = value;
      }

      /// <summary>
      /// A value of SearchReturnVal.
      /// </summary>
      [JsonPropertyName("searchReturnVal")]
      public Common SearchReturnVal
      {
        get => searchReturnVal ??= new();
        set => searchReturnVal = value;
      }

      private Code searchCode;
      private CodeValue searchCodeValue;
      private Common searchReturnVal;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
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
      /// A value of CseOrganization.
      /// </summary>
      [JsonPropertyName("cseOrganization")]
      public CseOrganization CseOrganization
      {
        get => cseOrganization ??= new();
        set => cseOrganization = value;
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

      /// <summary>
      /// A value of HidVerifyDel.
      /// </summary>
      [JsonPropertyName("hidVerifyDel")]
      public CountyService HidVerifyDel
      {
        get => hidVerifyDel ??= new();
        set => hidVerifyDel = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 110;

      private Common common;
      private CseOrganization cseOrganization;
      private CseOrganization hidden;
      private CountyService hidVerifyDel;
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
    /// A value of HidPrevCommand.
    /// </summary>
    [JsonPropertyName("hidPrevCommand")]
    public Common HidPrevCommand
    {
      get => hidPrevCommand ??= new();
      set => hidPrevCommand = value;
    }

    /// <summary>
    /// Gets a value of SearchCodePromt.
    /// </summary>
    [JsonPropertyName("searchCodePromt")]
    public SearchCodePromtGroup SearchCodePromt
    {
      get => searchCodePromt ?? (searchCodePromt = new());
      set => searchCodePromt = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CseOrganization Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SelTypePrompt.
    /// </summary>
    [JsonPropertyName("selTypePrompt")]
    public Common SelTypePrompt
    {
      get => selTypePrompt ??= new();
      set => selTypePrompt = value;
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

    private DateWorkArea dateWorkArea;
    private Common hidPrevCommand;
    private SearchCodePromtGroup searchCodePromt;
    private CseOrganization search;
    private Common selTypePrompt;
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
    /// <summary>A SearchCodePromtGroup group.</summary>
    [Serializable]
    public class SearchCodePromtGroup
    {
      /// <summary>
      /// A value of SearchCode.
      /// </summary>
      [JsonPropertyName("searchCode")]
      public Code SearchCode
      {
        get => searchCode ??= new();
        set => searchCode = value;
      }

      /// <summary>
      /// A value of SearchCodeValue.
      /// </summary>
      [JsonPropertyName("searchCodeValue")]
      public CodeValue SearchCodeValue
      {
        get => searchCodeValue ??= new();
        set => searchCodeValue = value;
      }

      /// <summary>
      /// A value of SearchReturnVal.
      /// </summary>
      [JsonPropertyName("searchReturnVal")]
      public Common SearchReturnVal
      {
        get => searchReturnVal ??= new();
        set => searchReturnVal = value;
      }

      private Code searchCode;
      private CodeValue searchCodeValue;
      private Common searchReturnVal;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
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
      /// A value of CseOrganization.
      /// </summary>
      [JsonPropertyName("cseOrganization")]
      public CseOrganization CseOrganization
      {
        get => cseOrganization ??= new();
        set => cseOrganization = value;
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

      /// <summary>
      /// A value of HidVerifyDel.
      /// </summary>
      [JsonPropertyName("hidVerifyDel")]
      public CountyService HidVerifyDel
      {
        get => hidVerifyDel ??= new();
        set => hidVerifyDel = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 110;

      private Common common;
      private CseOrganization cseOrganization;
      private CseOrganization hidden;
      private CountyService hidVerifyDel;
    }

    /// <summary>A Export1Group group.</summary>
    [Serializable]
    public class Export1Group
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
      /// A value of ChildTypePrompt.
      /// </summary>
      [JsonPropertyName("childTypePrompt")]
      public Common ChildTypePrompt
      {
        get => childTypePrompt ??= new();
        set => childTypePrompt = value;
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
      /// A value of Export3.
      /// </summary>
      [JsonPropertyName("export3")]
      public CseOrganization Export3
      {
        get => export3 ??= new();
        set => export3 = value;
      }

      /// <summary>
      /// A value of HidSel.
      /// </summary>
      [JsonPropertyName("hidSel")]
      public Common HidSel
      {
        get => hidSel ??= new();
        set => hidSel = value;
      }

      /// <summary>
      /// A value of HidChPrmt.
      /// </summary>
      [JsonPropertyName("hidChPrmt")]
      public Common HidChPrmt
      {
        get => hidChPrmt ??= new();
        set => hidChPrmt = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CseOrganizationRelationship Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of Hidden1.
      /// </summary>
      [JsonPropertyName("hidden1")]
      public CseOrganization Hidden1
      {
        get => hidden1 ??= new();
        set => hidden1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common sel;
      private Common childTypePrompt;
      private CseOrganizationRelationship cseOrganizationRelationship;
      private CseOrganization export3;
      private Common hidSel;
      private Common hidChPrmt;
      private CseOrganizationRelationship hidden;
      private CseOrganization hidden1;
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
    /// A value of HidPrevCommand.
    /// </summary>
    [JsonPropertyName("hidPrevCommand")]
    public Common HidPrevCommand
    {
      get => hidPrevCommand ??= new();
      set => hidPrevCommand = value;
    }

    /// <summary>
    /// A value of ReturnSelect.
    /// </summary>
    [JsonPropertyName("returnSelect")]
    public CseOrganization ReturnSelect
    {
      get => returnSelect ??= new();
      set => returnSelect = value;
    }

    /// <summary>
    /// Gets a value of SearchCodePromt.
    /// </summary>
    [JsonPropertyName("searchCodePromt")]
    public SearchCodePromtGroup SearchCodePromt
    {
      get => searchCodePromt ?? (searchCodePromt = new());
      set => searchCodePromt = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CseOrganization Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SelTypePrompt.
    /// </summary>
    [JsonPropertyName("selTypePrompt")]
    public Common SelTypePrompt
    {
      get => selTypePrompt ??= new();
      set => selTypePrompt = value;
    }

    /// <summary>
    /// Gets a value of Export2.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export2 => export2 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export2 for json serialization.
    /// </summary>
    [JsonPropertyName("export2")]
    [Computed]
    public IList<ExportGroup> Export2_Json
    {
      get => export2;
      set => Export2.Assign(value);
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<Export1Group> Export1 =>
      export1 ??= new(Export1Group.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<Export1Group> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private DateWorkArea dateWorkArea;
    private Common hidPrevCommand;
    private CseOrganization returnSelect;
    private SearchCodePromtGroup searchCodePromt;
    private CseOrganization search;
    private Common selTypePrompt;
    private Array<ExportGroup> export2;
    private NextTranInfo hidden;
    private Standard standard;
    private Array<Export1Group> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroValueInitialize.
    /// </summary>
    [JsonPropertyName("zeroValueInitialize")]
    public DateWorkArea ZeroValueInitialize
    {
      get => zeroValueInitialize ??= new();
      set => zeroValueInitialize = value;
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
    /// A value of CurrCommand.
    /// </summary>
    [JsonPropertyName("currCommand")]
    public Common CurrCommand
    {
      get => currCommand ??= new();
      set => currCommand = value;
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

    private DateWorkArea zeroValueInitialize;
    private DateWorkArea dateWorkArea;
    private Common currCommand;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CseOrganization Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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

    private CseOrganization child;
    private CseOrganization parent;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private Office office;
    private CseOrganization cseOrganization;
    private CountyService countyService;
  }
#endregion
}

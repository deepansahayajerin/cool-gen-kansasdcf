// Program: LE_LACS_LST_LGL_ACTN_BY_CSE_CASE, ID: 371972211, model: 746.
// Short name: SWELACSP
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
/// A program: LE_LACS_LST_LGL_ACTN_BY_CSE_CASE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLacsLstLglActnByCseCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LACS_LST_LGL_ACTN_BY_CSE_CASE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLacsLstLglActnByCseCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLacsLstLglActnByCseCase.
  /// </summary>
  public LeLacsLstLglActnByCseCase(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 05/10/95  Dave Allen			Initial Code
    // 11/19/98  P McElderry	None listed	Enhanced efficiency of group views.  
    // Renamed uneeded entity views
    // 					to ZDEL.   These need to be deleted when possible.  Modified READ
    // 					statements to eliminate uneeded DB2 inquiries.
    // 01/11/01  GVandy	WR275		Add PF Key to flow to IWGL.
    // 12/10/01  GVandy	PR133994	Pass selected legal action id on nextran.
    // 06/28/02  VMadhira	PR# 149883	Fix for scrolling problem. Change screen to
    // explicit  scrolling.
    // 12/23/02  GVandy	WR10492		Display new attribute legal_action 
    // system_gen_ind.
    // 06/15/11  T. Pierce	CQ23493		Overhaul to change scrolling capability, 
    // allowing unlimited paging.
    // 					Removal of disused views.
    // -----------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -----------------------
    // Move Imports to Exports
    // -----------------------
    export.Case1.Number = import.Case1.Number;
    export.Previous.Number = import.Previous.Number;

    if (!IsEmpty(export.Case1.Number))
    {
      local.TextWorkArea.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    export.CourtCaseNumberOnly.Text1 = import.CourtCaseNumberOnly.Text1;
    export.CutOff.Date = import.CutOff.Date;
    MoveLegalAction3(import.Filter, export.Filter);
    export.ListLactsPriorToCase.Flag = import.ListLactsPriorToCase.Flag;
    export.PromptClassification.PromptField =
      import.PromptClassification.PromptField;

    // -----------------------------------------------------
    // CQ23493 T. Pierce
    // New views added to support scrolling mechanism.
    // -----------------------------------------------------
    export.More.Flag = import.More.Flag;
    export.Prev.Flag = import.Prev.Flag;
    export.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      import.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.NextCourtCaseNumOnlyTribunal.Identifier =
      import.NextCourtCaseNumOnlyTribunal.Identifier;
    MoveLegalAction2(import.NextAllActions, export.NextAllActions);
    export.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      import.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.PrevCourtCaseNumOnlyTribunal.Identifier =
      import.PrevCourtCaseNumOnlyTribunal.Identifier;
    MoveLegalAction2(import.PrevAllActions, export.PrevAllActions);

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "NEXT1"))
    {
      global.Command = "NEXT";
    }

    export.Plus.Text1 = import.Plus.Text1;
    export.Minus.Text1 = import.Minus.Text1;

    for(import.List.Index = 0; import.List.Index < import.List.Count; ++
      import.List.Index)
    {
      if (!import.List.CheckSize())
      {
        break;
      }

      export.List.Index = import.List.Index;
      export.List.CheckSize();

      export.List.Update.DetailLaAppInd.Flag =
        import.List.Item.DetailLaAppInd.Flag;
      export.List.Update.DetailForeign.Country =
        import.List.Item.DetailForeign.Country;
      export.List.Update.DetailFips.Assign(import.List.Item.DetailFips);
      export.List.Update.DetailActionTaken.Description =
        import.List.Item.DetailActionTaken.Description;
      export.List.Update.DetailTribunal.Assign(import.List.Item.DetailTribunal);
      export.List.Update.Common.SelectChar = import.List.Item.Common.SelectChar;
      export.List.Update.LegalAction.Assign(import.List.Item.LegalAction);
    }

    import.List.CheckIndex();

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ----------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // ----------------------------------------------------------
      export.Case1.Number = local.NextTranInfo.CaseNumber ?? Spaces(10);

      if (IsEmpty(export.Case1.Number))
      {
        return;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // -----------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // ------------------------------------------------------------
      local.NextTranInfo.CaseNumber = export.Case1.Number;
      local.Selected.Count = 0;

      for(export.List.Index = 0; export.List.Index < export.List.Count; ++
        export.List.Index)
      {
        if (!export.List.CheckSize())
        {
          break;
        }

        switch(AsChar(export.List.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Selected.Count;

            if (local.Selected.Count > 1)
            {
              var field1 = GetField(export.List.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            }

            MoveLegalAction1(export.List.Item.LegalAction, export.DlgflwSelected);
              

            break;
          case ' ':
            // -------------------
            // Continue processing
            // -------------------
            break;
          default:
            var field = GetField(export.List.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }
      }

      export.List.CheckIndex();
      local.NextTranInfo.LegalActionIdentifier =
        export.DlgflwSelected.Identifier;
      UseScCabNextTranPut();

      return;
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "LACT") || Equal(global.Command, "IWGL") || Equal
      (global.Command, "GLDV"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "BYPASS";
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // --------------------
    // Edit required fields
    // ---------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // -------------------------------------------------------------------
      //              Initialize the Group Views.
      // ------------------------------------------------------------------
      // -----------------------------------------------------
      // CQ23493 T. Pierce
      // Changes to remove obsolete code so views
      // can be cleaned up.
      // -----------------------------------------------------
      for(export.List.Index = 0; export.List.Index < export.List.Count; ++
        export.List.Index)
      {
        if (!export.List.CheckSize())
        {
          break;
        }

        export.List.Update.DetailLaAppInd.Flag =
          local.List.Item.DetailLaAppInd.Flag;
        export.List.Update.DetailForeign.Country =
          local.List.Item.DetailForeign.Country;
        export.List.Update.DetailFips.Assign(local.List.Item.DetailFips);
        export.List.Update.DetailActionTaken.Description =
          local.List.Item.DetailActionTaken.Description;
        export.List.Update.DetailTribunal.
          Assign(local.List.Item.DetailTribunal);
        export.List.Update.Common.SelectChar =
          local.List.Item.Common.SelectChar;
        export.List.Update.LegalAction.Assign(local.List.Item.LegalAction);
      }

      export.List.CheckIndex();
      export.List.Index = -1;
      export.List.Count = 0;

      if (IsEmpty(export.Case1.Number))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        global.Command = "BYPASS";

        goto Test1;
      }

      // ----------------------
      // 11/25/98 begin changes
      // ----------------------
      if (!Equal(export.Case1.Number, export.Previous.Number))
      {
        if (ReadCase())
        {
          export.Case1.Number = entities.Case1.Number;
          export.Previous.Number = entities.Case1.Number;
        }
        else
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "CASE_NF";
          global.Command = "BYPASS";

          goto Test1;
        }
      }
      else
      {
        // -----------------------
        // No processing required
        // -----------------------
        // ----------------------
        // 11/25/98 end changes
        // ----------------------
      }

      // ---------------------------------------------
      // Edit "Show Court Case Numbers Only (Y/N)".
      // ---------------------------------------------
      if (IsEmpty(export.CourtCaseNumberOnly.Text1))
      {
        export.CourtCaseNumberOnly.Text1 = "N";
      }

      switch(AsChar(export.CourtCaseNumberOnly.Text1))
      {
        case 'Y':
          export.ListLactsPriorToCase.Flag = "";

          if (!IsEmpty(export.Filter.Classification))
          {
            var field1 = GetField(export.Filter, "classification");

            field1.Error = true;

            ExitState = "LE0000_VALID_ONLY_4_LST_ALL_LACT";
          }

          if (Lt(local.InitialisedToZeros.Date, export.CutOff.Date))
          {
            var field1 = GetField(export.CutOff, "date");

            field1.Error = true;

            ExitState = "LE0000_VALID_ONLY_4_LST_ALL_LACT";
          }

          break;
        case 'N':
          if (!IsEmpty(export.Filter.Classification))
          {
            local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
            local.CodeValue.Cdvalue = export.Filter.Classification;
            UseCabValidateCodeValue();

            if (AsChar(local.ValidCode.Flag) == 'N')
            {
              ExitState = "LE0000_INVALID_LEG_ACT_CLASSIFN";

              var field1 = GetField(export.Filter, "classification");

              field1.Error = true;
            }
          }

          if (!IsEmpty(export.Filter.CourtCaseNumber))
          {
            var field1 = GetField(export.Filter, "courtCaseNumber");

            field1.Error = true;

            ExitState = "LE0000_VALID_ONLY_4_LST_CT_CASES";
          }

          switch(AsChar(export.ListLactsPriorToCase.Flag))
          {
            case 'Y':
              // -------------------
              // Continue processing
              // -------------------
              break;
            case 'N':
              // -------------------
              // Continue processing
              // -------------------
              break;
            case ' ':
              export.ListLactsPriorToCase.Flag = "Y";

              break;
            default:
              ExitState = "LE0000_INVALID_LIST_PRIOR_LACTS";

              var field1 = GetField(export.ListLactsPriorToCase, "flag");

              field1.Error = true;

              break;
          }

          break;
        default:
          var field = GetField(export.CourtCaseNumberOnly, "text1");

          field.Error = true;

          ExitState = "ZD_ACO_NE00_INVALID_INDICATOR_YN";

          break;
      }
    }

Test1:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    local.Common.Count = 0;

    for(export.List.Index = 0; export.List.Index < export.List.Count; ++
      export.List.Index)
    {
      if (!export.List.CheckSize())
      {
        break;
      }

      switch(AsChar(export.List.Item.Common.SelectChar))
      {
        case 'S':
          ++local.Common.Count;

          if (local.Common.Count > 1)
          {
            var field1 = GetField(export.List.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }

          break;
        case ' ':
          // -------------------
          // Continue processing
          // -------------------
          break;
        default:
          var field = GetField(export.List.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }

      if (Equal(global.Command, "LIST") && !
        IsEmpty(export.List.Item.Common.SelectChar))
      {
        var field = GetField(export.List.Item.Common, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECTION";
      }
      else
      {
        // -------------------
        // Continue processing
        // -------------------
      }
    }

    export.List.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }
    else
    {
      // -------------------
      // Continue processing
      // -------------------
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "IWGL":
        if (local.Common.Count == 0)
        {
          ExitState = "LE0000_LACT_MUST_BE_SELECTED";
        }
        else
        {
          for(export.List.Index = 0; export.List.Index < export.List.Count; ++
            export.List.Index)
          {
            if (!export.List.CheckSize())
            {
              break;
            }

            if (AsChar(export.List.Item.Common.SelectChar) == 'S')
            {
              if (AsChar(export.List.Item.LegalAction.Classification) == 'I'
                || AsChar(export.List.Item.LegalAction.Classification) == 'G')
              {
                MoveLegalAction1(export.List.Item.LegalAction,
                  export.DlgflwSelected);
                export.DlgflwIwglType.Text1 =
                  export.List.Item.LegalAction.Classification;
                ExitState = "ECO_LNK_TO_IWGL";

                goto Test2;
              }
              else
              {
                var field = GetField(export.List.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "LE0000_I_OR_G_CLASS_FOR_IWGL";
              }
            }
            else
            {
            }
          }

          export.List.CheckIndex();
        }

        break;
      case "DISPLAY":
        export.List.Index = -1;
        export.List.Count = 0;

        // -----------------------------------------------------
        // CQ23493 T. Pierce
        // Changes to match new views to views in the
        // called action block.
        // -----------------------------------------------------
        // -------------------------------------------------------------
        // If a display is required, call the action block that reads the
        // next group of data based on the starting key value.
        // --------------------------------------------------------------
        UseLeLacsListLegActsByCseCas3();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!export.List.IsEmpty)
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (export.List.IsEmpty)
          {
            ExitState = "FN0000_NO_RECORDS_FOUND";
          }
        }

        break;
      case "LIST":
        switch(AsChar(export.PromptClassification.PromptField))
        {
          case ' ':
            ExitState = "ZD_ACO_NE0000_MUST_SEL_4_PROMPT";

            var field1 = GetField(export.PromptClassification, "promptField");

            field1.Error = true;

            break;
          case 'S':
            export.DlgflwRequired.CodeName = "LEGAL ACTION CLASSIFICATION";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field2 = GetField(export.PromptClassification, "promptField");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        break;
      case "RETCDVL":
        if (AsChar(export.PromptClassification.PromptField) == 'S')
        {
          export.PromptClassification.PromptField = "";

          if (!IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            export.Filter.Classification = import.DlgflwSelected.Cdvalue;

            var field = GetField(export.CutOff, "date");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.Filter, "classification");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "LACT":
        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (!export.List.CheckSize())
          {
            break;
          }

          switch(AsChar(export.List.Item.Common.SelectChar))
          {
            case 'S':
              MoveLegalAction1(export.List.Item.LegalAction,
                export.DlgflwSelected);
              ExitState = "ECO_LNK_TO_LEGAL_ACTION";

              goto Test2;
            case ' ':
              // -------------------
              // Continue processing
              // -------------------
              break;
            default:
              var field = GetField(export.List.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }
        }

        export.List.CheckIndex();
        ExitState = "ZD_ACO_NE0000_NO_SELECTN_MADE_2";

        break;
      case "GLDV":
        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (!export.List.CheckSize())
          {
            break;
          }

          switch(AsChar(export.List.Item.Common.SelectChar))
          {
            case 'S':
              MoveLegalAction1(export.List.Item.LegalAction,
                export.DlgflwSelected);
              export.LacsLegalAction.Identifier =
                export.List.Item.LegalAction.Identifier;

              if (ReadGuidelineDeviations())
              {
                export.LacsWorkArea.Text1 = "Y";
                ExitState = "ECO_LINK_TO_GLDV";

                return;
              }
              else
              {
                ExitState = "GUIDELINE_DEVIATION_NF";

                var field1 = GetField(export.List.Item.Common, "selectChar");

                field1.Error = true;

                return;
              }

              break;
            case ' ':
              // -------------------
              // Continue processing
              // -------------------
              break;
            default:
              var field = GetField(export.List.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }
        }

        export.List.CheckIndex();
        ExitState = "ZD_ACO_NE0000_NO_SELECTN_MADE_2";

        break;
      case "NEXT":
        // -----------------------------------------------------
        // CQ23493 T. Pierce
        // Changes to match new views to views in the
        // called action block.
        // -----------------------------------------------------
        if (AsChar(export.More.Flag) == 'Y')
        {
          UseLeLacsListLegActsByCseCas1();
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!export.List.IsEmpty)
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (export.List.IsEmpty)
          {
            ExitState = "FN0000_NO_RECORDS_FOUND";
          }
        }

        break;
      case "PREV":
        // -----------------------------------------------------
        // CQ23493 T. Pierce
        // Changes to match new views to views in the
        // called action block.
        // -----------------------------------------------------
        if (AsChar(export.Prev.Flag) == 'Y')
        {
          UseLeLacsListLegActsByCseCas2();
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!export.List.IsEmpty)
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (export.List.IsEmpty)
          {
            ExitState = "FN0000_NO_RECORDS_FOUND";
          }
        }

        break;
      case "RETURN":
        // ---------------------------------------------
        // Don't allow more than one occurrence to be
        // selected.
        // ---------------------------------------------
        local.Selected.Count = 0;

        for(export.List.Index = 0; export.List.Index < export.List.Count; ++
          export.List.Index)
        {
          if (!export.List.CheckSize())
          {
            break;
          }

          switch(AsChar(export.List.Item.Common.SelectChar))
          {
            case 'S':
              ++local.Selected.Count;

              if (local.Selected.Count > 1)
              {
                var field1 = GetField(export.List.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

                goto Test2;
              }
              else
              {
                MoveLegalAction1(export.List.Item.LegalAction,
                  export.DlgflwSelected);
                export.SelectedTribunal.Assign(export.List.Item.DetailTribunal);
                export.SelectedFips.Assign(export.List.Item.DetailFips);
                export.SelectedForeign.Country =
                  export.List.Item.DetailForeign.Country;
                export.SelectedLactActionTakn.Description =
                  export.List.Item.DetailActionTaken.Description;
              }

              break;
            case ' ':
              // -------------------
              // Continue processing
              // -------------------
              break;
            default:
              var field = GetField(export.List.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }
        }

        export.List.CheckIndex();
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "BYPASS":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test2:

    // --------------------------------------------------------------------------
    //         The following code will set the "MORE" indicator.
    // --------------------------------------------------------------------------
    // -----------------------------------------------------
    // CQ23493 T. Pierce
    // Changes to use new "more" and "prev" flags.
    // -----------------------------------------------------
    if (AsChar(export.More.Flag) == 'Y')
    {
      export.Plus.Text1 = "+";
    }
    else
    {
      export.Plus.Text1 = "";
    }

    if (AsChar(export.Prev.Flag) == 'Y')
    {
      export.Minus.Text1 = "-";
    }
    else
    {
      export.Minus.Text1 = "";
    }
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CreatedTstamp = source.CreatedTstamp;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveList(LeLacsListLegActsByCseCas.Export.
    ListGroup source, Export.ListGroup target)
  {
    target.DetailLaAppInd.Flag = source.DetailLaAppInd.Flag;
    target.DetailForeign.Country = source.DetailForeign.Country;
    target.DetailFips.Assign(source.DetailFips);
    target.DetailActionTaken.Description = source.DetailActionTaken.Description;
    target.DetailTribunal.Assign(source.DetailTribunal);
    target.Common.SelectChar = source.DetailCommon.SelectChar;
    target.LegalAction.Assign(source.DetailLegalAction);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

  private void UseLeLacsListLegActsByCseCas1()
  {
    var useImport = new LeLacsListLegActsByCseCas.Import();
    var useExport = new LeLacsListLegActsByCseCas.Export();

    MoveLegalAction2(import.NextAllActions, useImport.StartAllActions);
    useImport.StartCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      import.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    useImport.StartCourtCaseNumOnlyTribunal.Identifier =
      import.NextCourtCaseNumOnlyTribunal.Identifier;
    useImport.ListLactsPriorToCase.Flag = export.ListLactsPriorToCase.Flag;
    MoveLegalAction3(export.Filter, useImport.Filter);
    useImport.CutOff.Date = export.CutOff.Date;
    useImport.Search.Number = export.Case1.Number;
    useImport.CourtCaseNumberOnly.Text1 = export.CourtCaseNumberOnly.Text1;

    Call(LeLacsListLegActsByCseCas.Execute, useImport, useExport);

    MoveLegalAction2(useExport.NextAllActions, export.NextAllActions);
    MoveLegalAction2(useExport.PrevAllActions, export.PrevAllActions);
    export.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      useExport.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.NextCourtCaseNumOnlyTribunal.Identifier =
      useExport.NextCourtCaseNumOnlyTribunal.Identifier;
    export.More.Flag = useExport.More.Flag;
    export.Prev.Flag = useExport.Prev.Flag;
    export.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      useExport.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.PrevCourtCaseNumOnlyTribunal.Identifier =
      useExport.PrevCourtCaseNumOnlyTribunal.Identifier;
    useExport.List.CopyTo(export.List, MoveList);
  }

  private void UseLeLacsListLegActsByCseCas2()
  {
    var useImport = new LeLacsListLegActsByCseCas.Import();
    var useExport = new LeLacsListLegActsByCseCas.Export();

    MoveLegalAction2(import.PrevAllActions, useImport.StartAllActions);
    useImport.StartCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      import.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    useImport.StartCourtCaseNumOnlyTribunal.Identifier =
      import.PrevCourtCaseNumOnlyTribunal.Identifier;
    useImport.ListLactsPriorToCase.Flag = export.ListLactsPriorToCase.Flag;
    MoveLegalAction3(export.Filter, useImport.Filter);
    useImport.CutOff.Date = export.CutOff.Date;
    useImport.Search.Number = export.Case1.Number;
    useImport.CourtCaseNumberOnly.Text1 = export.CourtCaseNumberOnly.Text1;

    Call(LeLacsListLegActsByCseCas.Execute, useImport, useExport);

    MoveLegalAction2(useExport.NextAllActions, export.NextAllActions);
    MoveLegalAction2(useExport.PrevAllActions, export.PrevAllActions);
    export.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      useExport.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.NextCourtCaseNumOnlyTribunal.Identifier =
      useExport.NextCourtCaseNumOnlyTribunal.Identifier;
    export.More.Flag = useExport.More.Flag;
    export.Prev.Flag = useExport.Prev.Flag;
    export.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      useExport.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.PrevCourtCaseNumOnlyTribunal.Identifier =
      useExport.PrevCourtCaseNumOnlyTribunal.Identifier;
    useExport.List.CopyTo(export.List, MoveList);
  }

  private void UseLeLacsListLegActsByCseCas3()
  {
    var useImport = new LeLacsListLegActsByCseCas.Import();
    var useExport = new LeLacsListLegActsByCseCas.Export();

    useImport.ListLactsPriorToCase.Flag = export.ListLactsPriorToCase.Flag;
    MoveLegalAction3(export.Filter, useImport.Filter);
    useImport.CutOff.Date = export.CutOff.Date;
    useImport.CourtCaseNumberOnly.Text1 = export.CourtCaseNumberOnly.Text1;
    useImport.Search.Number = export.Case1.Number;
    useImport.StartCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      import.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber;

    Call(LeLacsListLegActsByCseCas.Execute, useImport, useExport);

    MoveLegalAction2(useExport.NextAllActions, export.NextAllActions);
    MoveLegalAction2(useExport.PrevAllActions, export.PrevAllActions);
    export.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      useExport.NextCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.NextCourtCaseNumOnlyTribunal.Identifier =
      useExport.NextCourtCaseNumOnlyTribunal.Identifier;
    export.More.Flag = useExport.More.Flag;
    export.Prev.Flag = useExport.Prev.Flag;
    export.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber =
      useExport.PrevCourtCaseNumOnlyLegalAction.CourtCaseNumber;
    export.PrevCourtCaseNumOnlyTribunal.Identifier =
      useExport.PrevCourtCaseNumOnlyTribunal.Identifier;
    useExport.List.CopyTo(export.List, MoveList);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadGuidelineDeviations()
  {
    entities.GuidelineDeviations.Populated = false;

    return Read("ReadGuidelineDeviations",
      (db, command) =>
      {
        db.SetInt32(command, "ckfk01738", export.LacsLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.GuidelineDeviations.Identifier = db.GetInt32(reader, 0);
        entities.GuidelineDeviations.FkCktLegalAclegalActionId =
          db.GetInt32(reader, 1);
        entities.GuidelineDeviations.Populated = true;
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
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of DetailLaAppInd.
      /// </summary>
      [JsonPropertyName("detailLaAppInd")]
      public Common DetailLaAppInd
      {
        get => detailLaAppInd ??= new();
        set => detailLaAppInd = value;
      }

      /// <summary>
      /// A value of DetailForeign.
      /// </summary>
      [JsonPropertyName("detailForeign")]
      public FipsTribAddress DetailForeign
      {
        get => detailForeign ??= new();
        set => detailForeign = value;
      }

      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailActionTaken.
      /// </summary>
      [JsonPropertyName("detailActionTaken")]
      public CodeValue DetailActionTaken
      {
        get => detailActionTaken ??= new();
        set => detailActionTaken = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common detailLaAppInd;
      private FipsTribAddress detailForeign;
      private Fips detailFips;
      private CodeValue detailActionTaken;
      private Tribunal detailTribunal;
      private Common common;
      private LegalAction legalAction;
    }

    /// <summary>
    /// A value of NextAllActions.
    /// </summary>
    [JsonPropertyName("nextAllActions")]
    public LegalAction NextAllActions
    {
      get => nextAllActions ??= new();
      set => nextAllActions = value;
    }

    /// <summary>
    /// A value of PrevAllActions.
    /// </summary>
    [JsonPropertyName("prevAllActions")]
    public LegalAction PrevAllActions
    {
      get => prevAllActions ??= new();
      set => prevAllActions = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Common Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of PrevCourtCaseNumOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("prevCourtCaseNumOnlyLegalAction")]
    public LegalAction PrevCourtCaseNumOnlyLegalAction
    {
      get => prevCourtCaseNumOnlyLegalAction ??= new();
      set => prevCourtCaseNumOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of PrevCourtCaseNumOnlyTribunal.
    /// </summary>
    [JsonPropertyName("prevCourtCaseNumOnlyTribunal")]
    public Tribunal PrevCourtCaseNumOnlyTribunal
    {
      get => prevCourtCaseNumOnlyTribunal ??= new();
      set => prevCourtCaseNumOnlyTribunal = value;
    }

    /// <summary>
    /// A value of NextCourtCaseNumOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("nextCourtCaseNumOnlyLegalAction")]
    public LegalAction NextCourtCaseNumOnlyLegalAction
    {
      get => nextCourtCaseNumOnlyLegalAction ??= new();
      set => nextCourtCaseNumOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of NextCourtCaseNumOnlyTribunal.
    /// </summary>
    [JsonPropertyName("nextCourtCaseNumOnlyTribunal")]
    public Tribunal NextCourtCaseNumOnlyTribunal
    {
      get => nextCourtCaseNumOnlyTribunal ??= new();
      set => nextCourtCaseNumOnlyTribunal = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of ListLactsPriorToCase.
    /// </summary>
    [JsonPropertyName("listLactsPriorToCase")]
    public Common ListLactsPriorToCase
    {
      get => listLactsPriorToCase ??= new();
      set => listLactsPriorToCase = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of CutOff.
    /// </summary>
    [JsonPropertyName("cutOff")]
    public DateWorkArea CutOff
    {
      get => cutOff ??= new();
      set => cutOff = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CourtCaseNumberOnly.
    /// </summary>
    [JsonPropertyName("courtCaseNumberOnly")]
    public WorkArea CourtCaseNumberOnly
    {
      get => courtCaseNumberOnly ??= new();
      set => courtCaseNumberOnly = value;
    }

    /// <summary>
    /// A value of PromptClassification.
    /// </summary>
    [JsonPropertyName("promptClassification")]
    public Standard PromptClassification
    {
      get => promptClassification ??= new();
      set => promptClassification = value;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    private LegalAction nextAllActions;
    private LegalAction prevAllActions;
    private Common more;
    private Common prev;
    private LegalAction prevCourtCaseNumOnlyLegalAction;
    private Tribunal prevCourtCaseNumOnlyTribunal;
    private LegalAction nextCourtCaseNumOnlyLegalAction;
    private Tribunal nextCourtCaseNumOnlyTribunal;
    private Case1 previous;
    private Common listLactsPriorToCase;
    private LegalAction filter;
    private DateWorkArea cutOff;
    private Standard standard;
    private Case1 case1;
    private WorkArea courtCaseNumberOnly;
    private Standard promptClassification;
    private CodeValue dlgflwSelected;
    private Array<ListGroup> list;
    private WorkArea minus;
    private WorkArea plus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of DetailLaAppInd.
      /// </summary>
      [JsonPropertyName("detailLaAppInd")]
      public Common DetailLaAppInd
      {
        get => detailLaAppInd ??= new();
        set => detailLaAppInd = value;
      }

      /// <summary>
      /// A value of DetailForeign.
      /// </summary>
      [JsonPropertyName("detailForeign")]
      public FipsTribAddress DetailForeign
      {
        get => detailForeign ??= new();
        set => detailForeign = value;
      }

      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailActionTaken.
      /// </summary>
      [JsonPropertyName("detailActionTaken")]
      public CodeValue DetailActionTaken
      {
        get => detailActionTaken ??= new();
        set => detailActionTaken = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common detailLaAppInd;
      private FipsTribAddress detailForeign;
      private Fips detailFips;
      private CodeValue detailActionTaken;
      private Tribunal detailTribunal;
      private Common common;
      private LegalAction legalAction;
    }

    /// <summary>
    /// A value of NextAllActions.
    /// </summary>
    [JsonPropertyName("nextAllActions")]
    public LegalAction NextAllActions
    {
      get => nextAllActions ??= new();
      set => nextAllActions = value;
    }

    /// <summary>
    /// A value of PrevAllActions.
    /// </summary>
    [JsonPropertyName("prevAllActions")]
    public LegalAction PrevAllActions
    {
      get => prevAllActions ??= new();
      set => prevAllActions = value;
    }

    /// <summary>
    /// A value of NextCourtCaseNumOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("nextCourtCaseNumOnlyLegalAction")]
    public LegalAction NextCourtCaseNumOnlyLegalAction
    {
      get => nextCourtCaseNumOnlyLegalAction ??= new();
      set => nextCourtCaseNumOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of NextCourtCaseNumOnlyTribunal.
    /// </summary>
    [JsonPropertyName("nextCourtCaseNumOnlyTribunal")]
    public Tribunal NextCourtCaseNumOnlyTribunal
    {
      get => nextCourtCaseNumOnlyTribunal ??= new();
      set => nextCourtCaseNumOnlyTribunal = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Common Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of PrevCourtCaseNumOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("prevCourtCaseNumOnlyLegalAction")]
    public LegalAction PrevCourtCaseNumOnlyLegalAction
    {
      get => prevCourtCaseNumOnlyLegalAction ??= new();
      set => prevCourtCaseNumOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of PrevCourtCaseNumOnlyTribunal.
    /// </summary>
    [JsonPropertyName("prevCourtCaseNumOnlyTribunal")]
    public Tribunal PrevCourtCaseNumOnlyTribunal
    {
      get => prevCourtCaseNumOnlyTribunal ??= new();
      set => prevCourtCaseNumOnlyTribunal = value;
    }

    /// <summary>
    /// A value of DlgflwIwglType.
    /// </summary>
    [JsonPropertyName("dlgflwIwglType")]
    public WorkArea DlgflwIwglType
    {
      get => dlgflwIwglType ??= new();
      set => dlgflwIwglType = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of ListLactsPriorToCase.
    /// </summary>
    [JsonPropertyName("listLactsPriorToCase")]
    public Common ListLactsPriorToCase
    {
      get => listLactsPriorToCase ??= new();
      set => listLactsPriorToCase = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of CutOff.
    /// </summary>
    [JsonPropertyName("cutOff")]
    public DateWorkArea CutOff
    {
      get => cutOff ??= new();
      set => cutOff = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CourtCaseNumberOnly.
    /// </summary>
    [JsonPropertyName("courtCaseNumberOnly")]
    public WorkArea CourtCaseNumberOnly
    {
      get => courtCaseNumberOnly ??= new();
      set => courtCaseNumberOnly = value;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public LegalAction DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of SelectedForeign.
    /// </summary>
    [JsonPropertyName("selectedForeign")]
    public FipsTribAddress SelectedForeign
    {
      get => selectedForeign ??= new();
      set => selectedForeign = value;
    }

    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of SelectedLactActionTakn.
    /// </summary>
    [JsonPropertyName("selectedLactActionTakn")]
    public CodeValue SelectedLactActionTakn
    {
      get => selectedLactActionTakn ??= new();
      set => selectedLactActionTakn = value;
    }

    /// <summary>
    /// A value of SelectedTribunal.
    /// </summary>
    [JsonPropertyName("selectedTribunal")]
    public Tribunal SelectedTribunal
    {
      get => selectedTribunal ??= new();
      set => selectedTribunal = value;
    }

    /// <summary>
    /// A value of PromptClassification.
    /// </summary>
    [JsonPropertyName("promptClassification")]
    public Standard PromptClassification
    {
      get => promptClassification ??= new();
      set => promptClassification = value;
    }

    /// <summary>
    /// A value of DlgflwRequired.
    /// </summary>
    [JsonPropertyName("dlgflwRequired")]
    public Code DlgflwRequired
    {
      get => dlgflwRequired ??= new();
      set => dlgflwRequired = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

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
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public WorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public WorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of LacsWorkArea.
    /// </summary>
    [JsonPropertyName("lacsWorkArea")]
    public WorkArea LacsWorkArea
    {
      get => lacsWorkArea ??= new();
      set => lacsWorkArea = value;
    }

    /// <summary>
    /// A value of LacsLegalAction.
    /// </summary>
    [JsonPropertyName("lacsLegalAction")]
    public LegalAction LacsLegalAction
    {
      get => lacsLegalAction ??= new();
      set => lacsLegalAction = value;
    }

    private LegalAction nextAllActions;
    private LegalAction prevAllActions;
    private LegalAction nextCourtCaseNumOnlyLegalAction;
    private Tribunal nextCourtCaseNumOnlyTribunal;
    private Common more;
    private Common prev;
    private LegalAction prevCourtCaseNumOnlyLegalAction;
    private Tribunal prevCourtCaseNumOnlyTribunal;
    private WorkArea dlgflwIwglType;
    private Case1 previous;
    private Common listLactsPriorToCase;
    private LegalAction filter;
    private DateWorkArea cutOff;
    private Standard standard;
    private Case1 case1;
    private WorkArea courtCaseNumberOnly;
    private LegalAction dlgflwSelected;
    private FipsTribAddress selectedForeign;
    private Fips selectedFips;
    private CodeValue selectedLactActionTakn;
    private Tribunal selectedTribunal;
    private Standard promptClassification;
    private Code dlgflwRequired;
    private Array<ListGroup> list;
    private WorkArea minus;
    private WorkArea plus;
    private WorkArea lacsWorkArea;
    private LegalAction lacsLegalAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of DetailLaAppInd.
      /// </summary>
      [JsonPropertyName("detailLaAppInd")]
      public Common DetailLaAppInd
      {
        get => detailLaAppInd ??= new();
        set => detailLaAppInd = value;
      }

      /// <summary>
      /// A value of DetailForeign.
      /// </summary>
      [JsonPropertyName("detailForeign")]
      public FipsTribAddress DetailForeign
      {
        get => detailForeign ??= new();
        set => detailForeign = value;
      }

      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailActionTaken.
      /// </summary>
      [JsonPropertyName("detailActionTaken")]
      public CodeValue DetailActionTaken
      {
        get => detailActionTaken ??= new();
        set => detailActionTaken = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Common detailLaAppInd;
      private FipsTribAddress detailForeign;
      private Fips detailFips;
      private CodeValue detailActionTaken;
      private Tribunal detailTribunal;
      private Common common;
      private LegalAction legalAction;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

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

    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private DateWorkArea initialisedToZeros;
    private TextWorkArea textWorkArea;
    private Common common;
    private Common selected;
    private NextTranInfo nextTranInfo;
    private Array<ListGroup> list;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of GuidelineDeviations.
    /// </summary>
    [JsonPropertyName("guidelineDeviations")]
    public GuidelineDeviations GuidelineDeviations
    {
      get => guidelineDeviations ??= new();
      set => guidelineDeviations = value;
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

    private LegalAction legalAction;
    private GuidelineDeviations guidelineDeviations;
    private Case1 case1;
  }
#endregion
}

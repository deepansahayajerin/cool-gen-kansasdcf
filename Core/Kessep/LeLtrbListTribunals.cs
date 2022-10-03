// Program: LE_LTRB_LIST_TRIBUNALS, ID: 371964623, model: 746.
// Short name: SWELTRBP
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
/// A program: LE_LTRB_LIST_TRIBUNALS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLtrbListTribunals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LTRB_LIST_TRIBUNALS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLtrbListTribunals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLtrbListTribunals.
  /// </summary>
  public LeLtrbListTribunals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Description
    // 07/17/95	Dave Allen	Initial Code
    // 07/24/96	Siraj Konkader	Print
    // 01/04/97	R. Marchman	Add new security/next tran.
    // 09/29/98        R. Jean		Delete unnecessary code; set display command, 
    // initialize group view; correct screen, and procedure for standard unit
    // test problems noted; remove initialization of group view
    // 12/15/1998	M Ramirez	Revised print process.
    // 12/15/1998	M Ramirez	Added security.
    // 12/15/1998	M Ramirez	Removed command Enter from screen, and validations 
    // against it.  Also, removed case of command cases for Enter and Invalid.
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    UseSpDocSetLiterals();

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveFips(import.SearchFips, export.SearchFips);
    MoveTribunal(import.SearchTribunal, export.SearchTribunal);
    export.PromptState.SelectChar = import.PromptState.SelectChar;
    export.PromptCountry.SelectChar = import.PromptCountry.SelectChar;
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.Tribunals.IsEmpty)
    {
      export.Tribunals.Index = 0;
      export.Tribunals.Clear();

      for(import.Tribunals.Index = 0; import.Tribunals.Index < import
        .Tribunals.Count; ++import.Tribunals.Index)
      {
        if (export.Tribunals.IsFull)
        {
          break;
        }

        export.Tribunals.Update.Common.SelectChar =
          import.Tribunals.Item.Common.SelectChar;
        export.Tribunals.Update.Tribunal.Assign(import.Tribunals.Item.Tribunal);
        export.Tribunals.Update.Fips.Assign(import.Tribunals.Item.Fips);
        export.Tribunals.Next();
      }
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      UseScCabNextTranPut1();

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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

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
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //             E D I T    L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(import.SearchFips.StateAbbreviation) && IsEmpty
        (import.FipsTribAddress.Country))
      {
        var field1 = GetField(export.SearchFips, "stateAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.FipsTribAddress, "country");

        field2.Error = true;

        ExitState = "COUNTRY_OR_STATE_CODE_REQD";

        return;
      }

      if (!IsEmpty(export.SearchTribunal.JudicialDivision) || !
        IsEmpty(export.SearchTribunal.JudicialDistrict))
      {
        if (IsEmpty(export.SearchFips.StateAbbreviation) && IsEmpty
          (export.FipsTribAddress.Country))
        {
          var field = GetField(export.SearchFips, "stateAbbreviation");

          field.Protected = false;
          field.Focused = true;

          ExitState = "COUNTRY_OR_STATE_CODE_REQD";

          return;
        }
      }

      if ((!IsEmpty(export.SearchFips.StateAbbreviation) || !
        IsEmpty(export.SearchFips.CountyDescription)) && !
        IsEmpty(export.FipsTribAddress.Country))
      {
        ExitState = "EITHER_STATE_OR_CNTRY_CODE";

        var field = GetField(export.SearchFips, "stateAbbreviation");

        field.Protected = false;
        field.Focused = true;

        return;
      }
    }

    if (Equal(global.Command, "LIST"))
    {
      if (IsEmpty(export.PromptState.SelectChar) && IsEmpty
        (export.PromptCountry.SelectChar))
      {
        var field = GetField(export.PromptState, "selectChar");

        field.Protected = false;
        field.Focused = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.PromptState, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptCountry, "selectChar");

          field2.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";
        }
      }

      if (AsChar(export.PromptState.SelectChar) == 'S' && AsChar
        (export.PromptCountry.SelectChar) == 'S')
      {
        var field1 = GetField(export.PromptState, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.PromptCountry, "selectChar");

        field2.Error = true;

        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!Equal(global.Command, "RLCVAL"))
    {
      // ---------------------------------------------
      // Validate State.
      // ---------------------------------------------
      if (!Equal(global.Command, "LIST"))
      {
        if (!IsEmpty(export.SearchFips.StateAbbreviation))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = export.SearchFips.StateAbbreviation;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field = GetField(export.SearchFips, "stateAbbreviation");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
            }
          }
        }

        if (!IsEmpty(export.FipsTribAddress.Country))
        {
          local.Code.CodeName = "COUNTRY CODE";
          local.CodeValue.Cdvalue = export.FipsTribAddress.Country ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field = GetField(export.FipsTribAddress, "country");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
            }
          }
        }
      }

      // ---------------------------------------------
      // Validate Prompt State.
      // ---------------------------------------------
      switch(AsChar(export.PromptState.SelectChar))
      {
        case ' ':
          break;
        case '+':
          break;
        case 'S':
          if (!Equal(global.Command, "LIST"))
          {
            var field1 = GetField(export.PromptState, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
            }
          }

          break;
        default:
          var field = GetField(export.PromptState, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
          }

          break;
      }

      switch(AsChar(export.PromptCountry.SelectChar))
      {
        case ' ':
          break;
        case '+':
          break;
        case 'S':
          if (!Equal(global.Command, "LIST"))
          {
            var field1 = GetField(export.FipsTribAddress, "country");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
            }
          }

          break;
        default:
          var field = GetField(export.FipsTribAddress, "country");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
          }

          break;
      }
    }

    // ---------------------------------------------
    // Do not allow scrolling when a selection has
    // been made.
    // ---------------------------------------------
    if (Equal(global.Command, "PREV") || Equal(global.Command, "NEXT"))
    {
      for(export.Tribunals.Index = 0; export.Tribunals.Index < export
        .Tribunals.Count; ++export.Tribunals.Index)
      {
        if (!IsEmpty(export.Tribunals.Item.Common.SelectChar))
        {
          var field = GetField(export.Tribunals.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "TRIB":
        local.TotalSelected.Count = 0;

        for(export.Tribunals.Index = 0; export.Tribunals.Index < export
          .Tribunals.Count; ++export.Tribunals.Index)
        {
          switch(AsChar(export.Tribunals.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

                var field1 =
                  GetField(export.Tribunals.Item.Common, "selectChar");

                field1.Error = true;

                return;
              }

              export.DlgflwSelectedTribunal.Assign(
                export.Tribunals.Item.Tribunal);
              export.DlgflwSelectedFips.Assign(export.Tribunals.Item.Fips);

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Tribunals.Item.Common, "selectChar");

              field.Error = true;

              return;
          }
        }

        ExitState = "ECO_LNK_TO_TRIBUNAL";

        break;
      case "DISPLAY":
        break;
      case "LIST":
        // ---------------------------------------------
        // A Prompt must be selected when PF4 List is
        // pressed.
        // ---------------------------------------------
        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.DisplayActiveCasesOnly.Flag = "Y";
          export.Code.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        if (AsChar(export.PromptCountry.SelectChar) == 'S')
        {
          export.DisplayActiveCasesOnly.Flag = "Y";
          export.Code.CodeName = "COUNTRY CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        if (!IsExitState("ECO_LNK_TO_LIST_CODE_VALUE"))
        {
          var field = GetField(export.PromptState, "selectChar");

          field.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";
        }

        break;
      case "RLCVAL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // ---------------------------------------------
        if (AsChar(export.PromptState.SelectChar) == 'S')
        {
          export.PromptState.SelectChar = "+";

          if (!IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.SearchFips, "countyDescription");

            field.Protected = false;
            field.Focused = true;

            export.SearchFips.StateAbbreviation = import.DlgflwSelected.Cdvalue;
          }
          else
          {
            var field = GetField(export.SearchFips, "stateAbbreviation");

            field.Protected = false;
            field.Focused = true;

            export.SearchFips.StateAbbreviation = "";
          }
        }

        if (AsChar(export.PromptCountry.SelectChar) == 'S')
        {
          export.PromptCountry.SelectChar = "+";

          if (!IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.SearchTribunal, "judicialDistrict");

            field.Protected = false;
            field.Focused = true;

            export.FipsTribAddress.Country = import.DlgflwSelected.Cdvalue;
          }
          else
          {
            var field = GetField(export.FipsTribAddress, "country");

            field.Protected = false;
            field.Focused = true;

            export.FipsTribAddress.Country = "";
          }
        }

        break;
      case "NEXT":
        // __________________________________________
        // Because the procedure does implicit scrolling the procedure
        // action diagram never gets control until you have scrolled
        // beyond the last group ie. to the first page again.  Consequently,
        // a message that says you are on the last page when the
        // screen displays the first screen makes no sense, so display
        // the message you are on the first page here.
        // __________________________________________
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        // ---------------------------------------------
        // Don't allow more than one occurrence to be
        // selected.
        // ---------------------------------------------
        local.TotalSelected.Count = 0;

        for(export.Tribunals.Index = 0; export.Tribunals.Index < export
          .Tribunals.Count; ++export.Tribunals.Index)
        {
          if (!IsEmpty(export.Tribunals.Item.Common.SelectChar))
          {
            ++local.TotalSelected.Count;
          }
        }

        if (local.TotalSelected.Count > 1)
        {
          for(export.Tribunals.Index = 0; export.Tribunals.Index < export
            .Tribunals.Count; ++export.Tribunals.Index)
          {
            if (!IsEmpty(export.Tribunals.Item.Common.SelectChar))
            {
              var field = GetField(export.Tribunals.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

          return;
        }

        // ---------------------------------------------
        // Move selected view to a single view that will
        // be mapped back to calling screen view.
        // ---------------------------------------------
        for(export.Tribunals.Index = 0; export.Tribunals.Index < export
          .Tribunals.Count; ++export.Tribunals.Index)
        {
          switch(AsChar(export.Tribunals.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              export.DlgflwSelectedTribunal.Assign(
                export.Tribunals.Item.Tribunal);
              export.DlgflwSelectedFips.Assign(export.Tribunals.Item.Fips);
              export.DlgflwSelectedFipsTribAddress.Country =
                export.FipsTribAddress.Country;

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PRINT":
        for(export.Tribunals.Index = 0; export.Tribunals.Index < export
          .Tribunals.Count; ++export.Tribunals.Index)
        {
          if (AsChar(export.Tribunals.Item.Common.SelectChar) != 'S')
          {
            continue;
          }

          if (Equal(export.Tribunals.Item.Fips.StateAbbreviation, "KS"))
          {
            local.Document.Name = "REQCORKS";
          }
          else
          {
            local.Document.Name = "REQCOOUT";
          }

          // mjr
          // ------------------------------------------
          // 01/06/2000
          // NEXT TRAN needs to be cleared before invoking print process
          // -------------------------------------------------------
          export.Hidden.Assign(local.Null1);
          export.Standard.NextTransaction = "DKEY";
          export.Hidden.MiscText2 = TrimEnd(local.SpDocLiteral.IdDocument) + local
            .Document.Name;

          // mjr
          // ----------------------------------------------------
          // Place identifiers into next tran
          // -------------------------------------------------------
          export.Hidden.MiscText1 = TrimEnd(local.SpDocLiteral.IdTribunal) + NumberToString
            (export.Tribunals.Item.Tribunal.Identifier, 7, 9);
          UseScCabNextTranPut2();

          // mjr---> DKEY's trancode = SRPD
          //  Can change this to do a READ instead of hardcoding
          global.NextTran = "SRPD PRINT";

          return;
        }

        break;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();

        // mjr
        // ----------------------------------------------------
        // Extract identifiers from next tran
        // -------------------------------------------------------
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdTribunal));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchConvertNumToText.Number15 =
          StringToNumber(Substring(
            export.Hidden.MiscText1, 50, local.Position.Count + 9, 9));
        local.Tribunal.Identifier = (int)local.BatchConvertNumToText.Number15;

        if (ReadTribunal1())
        {
          if (ReadFips())
          {
            if (ReadFipsTribAddress1())
            {
              if (!Equal(entities.FipsTribAddress.Country, "US"))
              {
                MoveFips(entities.Fips, export.SearchFips);
                MoveTribunal(entities.Tribunal, export.SearchTribunal);
              }
              else
              {
                export.FipsTribAddress.Country =
                  entities.FipsTribAddress.Country;
              }
            }
            else if (ReadFipsTribAddress2())
            {
              if (!Equal(entities.FipsTribAddress.Country, "US"))
              {
                MoveFips(entities.Fips, export.SearchFips);
                MoveTribunal(entities.Tribunal, export.SearchTribunal);
              }
              else
              {
                export.FipsTribAddress.Country =
                  entities.FipsTribAddress.Country;
              }
            }
            else
            {
              export.SearchFips.StateAbbreviation = "KS";
            }
          }
          else if (ReadFipsTribAddress2())
          {
            if (!Equal(entities.FipsTribAddress.Country, "US"))
            {
              MoveFips(entities.Fips, export.SearchFips);
              MoveTribunal(entities.Tribunal, export.SearchTribunal);
            }
            else
            {
              export.FipsTribAddress.Country = entities.FipsTribAddress.Country;
            }
          }
          else
          {
            export.SearchFips.StateAbbreviation = "KS";
          }
        }
        else
        {
          export.SearchFips.StateAbbreviation = "KS";
        }

        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // mjr
    // ------------------------------------------------
    // 12/15/1998
    // Pulled command Display out of main case of command construct,
    // so it would be performed after a PrintRet.
    // -------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsEmpty(export.SearchFips.StateAbbreviation))
      {
        export.Tribunals.Index = 0;
        export.Tribunals.Clear();

        foreach(var item in ReadTribunalFips())
        {
          export.Tribunals.Update.Tribunal.Assign(entities.Tribunal);
          export.Tribunals.Update.Fips.Assign(entities.Fips);
          export.Tribunals.Next();
        }
      }

      if (!IsEmpty(export.FipsTribAddress.Country))
      {
        export.Tribunals.Index = 0;
        export.Tribunals.Clear();

        foreach(var item in ReadTribunal2())
        {
          export.Tribunals.Update.Tribunal.Assign(entities.Tribunal);
          export.Tribunals.Next();
        }
      }

      if (export.Tribunals.IsEmpty)
      {
        ExitState = "LE0000_NO_TRIB_TO_LIST";
      }
      else if (export.Tribunals.IsFull)
      {
        ExitState = "ACO_NI0000_LIST_IS_FULL";
      }
      else
      {
        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText2, NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count <= 0)
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          // mjr---> Determines the appropriate exitstate for the Print process
          local.WorkArea.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
          UseSpPrintDecodeReturnCode();
          export.Hidden.MiscText2 = local.WorkArea.Text50;
        }
      }
    }
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyDescription = source.CountyDescription;
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

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdTribunal = source.IdTribunal;
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.JudicialDistrict = source.JudicialDistrict;
    target.JudicialDivision = source.JudicialDivision;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

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

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
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

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.WorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.WorkArea.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.StateAbbreviation = db.GetString(reader, 6);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 5);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.CreatedBy = db.GetString(reader, 4);
        entities.Tribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Tribunal.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Tribunal.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.Tribunal.TaxIdSuffix = db.GetNullableString(reader, 9);
        entities.Tribunal.TaxId = db.GetNullableString(reader, 10);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 11);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 12);
        entities.Tribunal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunal2()
  {
    return ReadEach("ReadTribunal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "country", export.FipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        if (export.Tribunals.IsFull)
        {
          return false;
        }

        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.CreatedBy = db.GetString(reader, 4);
        entities.Tribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Tribunal.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Tribunal.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.Tribunal.TaxIdSuffix = db.GetNullableString(reader, 9);
        entities.Tribunal.TaxId = db.GetNullableString(reader, 10);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 11);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 12);
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTribunalFips()
  {
    return ReadEach("ReadTribunalFips",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.SearchFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyDesc", export.SearchFips.CountyDescription ?? "");
        db.SetString(
          command, "judicialDistrict", export.SearchTribunal.JudicialDistrict);
        db.SetNullableString(
          command, "judicialDivision",
          export.SearchTribunal.JudicialDivision ?? "");
      },
      (db, reader) =>
      {
        if (export.Tribunals.IsFull)
        {
          return false;
        }

        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.CreatedBy = db.GetString(reader, 4);
        entities.Tribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Tribunal.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Tribunal.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.Tribunal.TaxIdSuffix = db.GetNullableString(reader, 9);
        entities.Tribunal.TaxId = db.GetNullableString(reader, 10);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 11);
        entities.Fips.County = db.GetInt32(reader, 11);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 12);
        entities.Fips.State = db.GetInt32(reader, 12);
        entities.Fips.StateDescription = db.GetNullableString(reader, 13);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 14);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 15);
        entities.Fips.StateAbbreviation = db.GetString(reader, 16);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 17);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;

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
    /// <summary>A TribunalsGroup group.</summary>
    [Serializable]
    public class TribunalsGroup
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
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of Fips.
      /// </summary>
      [JsonPropertyName("fips")]
      public Fips Fips
      {
        get => fips ??= new();
        set => fips = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 112;

      private Common common;
      private Tribunal tribunal;
      private Fips fips;
    }

    /// <summary>
    /// A value of PromptCountry.
    /// </summary>
    [JsonPropertyName("promptCountry")]
    public Common PromptCountry
    {
      get => promptCountry ??= new();
      set => promptCountry = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    /// <summary>
    /// A value of PromptState.
    /// </summary>
    [JsonPropertyName("promptState")]
    public Common PromptState
    {
      get => promptState ??= new();
      set => promptState = value;
    }

    /// <summary>
    /// Gets a value of Tribunals.
    /// </summary>
    [JsonIgnore]
    public Array<TribunalsGroup> Tribunals => tribunals ??= new(
      TribunalsGroup.Capacity);

    /// <summary>
    /// Gets a value of Tribunals for json serialization.
    /// </summary>
    [JsonPropertyName("tribunals")]
    [Computed]
    public IList<TribunalsGroup> Tribunals_Json
    {
      get => tribunals;
      set => Tribunals.Assign(value);
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

    private Common promptCountry;
    private FipsTribAddress fipsTribAddress;
    private Tribunal searchTribunal;
    private CodeValue dlgflwSelected;
    private Standard standard;
    private Fips searchFips;
    private Common promptState;
    private Array<TribunalsGroup> tribunals;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A TribunalsGroup group.</summary>
    [Serializable]
    public class TribunalsGroup
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
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of Fips.
      /// </summary>
      [JsonPropertyName("fips")]
      public Fips Fips
      {
        get => fips ??= new();
        set => fips = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 112;

      private Common common;
      private Tribunal tribunal;
      private Fips fips;
    }

    /// <summary>
    /// A value of DlgflwSelectedFipsTribAddress.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedFipsTribAddress")]
    public FipsTribAddress DlgflwSelectedFipsTribAddress
    {
      get => dlgflwSelectedFipsTribAddress ??= new();
      set => dlgflwSelectedFipsTribAddress = value;
    }

    /// <summary>
    /// A value of PromptCountry.
    /// </summary>
    [JsonPropertyName("promptCountry")]
    public Common PromptCountry
    {
      get => promptCountry ??= new();
      set => promptCountry = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
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
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    /// <summary>
    /// A value of PromptState.
    /// </summary>
    [JsonPropertyName("promptState")]
    public Common PromptState
    {
      get => promptState ??= new();
      set => promptState = value;
    }

    /// <summary>
    /// Gets a value of Tribunals.
    /// </summary>
    [JsonIgnore]
    public Array<TribunalsGroup> Tribunals => tribunals ??= new(
      TribunalsGroup.Capacity);

    /// <summary>
    /// Gets a value of Tribunals for json serialization.
    /// </summary>
    [JsonPropertyName("tribunals")]
    [Computed]
    public IList<TribunalsGroup> Tribunals_Json
    {
      get => tribunals;
      set => Tribunals.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwSelectedFips.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedFips")]
    public Fips DlgflwSelectedFips
    {
      get => dlgflwSelectedFips ??= new();
      set => dlgflwSelectedFips = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedTribunal.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedTribunal")]
    public Tribunal DlgflwSelectedTribunal
    {
      get => dlgflwSelectedTribunal ??= new();
      set => dlgflwSelectedTribunal = value;
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private FipsTribAddress dlgflwSelectedFipsTribAddress;
    private Common promptCountry;
    private FipsTribAddress fipsTribAddress;
    private Tribunal searchTribunal;
    private Standard standard;
    private Fips searchFips;
    private Common promptState;
    private Array<TribunalsGroup> tribunals;
    private Fips dlgflwSelectedFips;
    private Tribunal dlgflwSelectedTribunal;
    private Common displayActiveCasesOnly;
    private Code code;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

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
    /// A value of TotalSelected.
    /// </summary>
    [JsonPropertyName("totalSelected")]
    public Common TotalSelected
    {
      get => totalSelected ??= new();
      set => totalSelected = value;
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

    private NextTranInfo null1;
    private SpDocLiteral spDocLiteral;
    private BatchConvertNumToText batchConvertNumToText;
    private Tribunal tribunal;
    private Document document;
    private WorkArea workArea;
    private Common position;
    private Fips fips;
    private Common totalSelected;
    private Code code;
    private Common validCode;
    private CodeValue codeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
  }
#endregion
}

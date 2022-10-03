// Program: SI_PPFX_PERSON_PROGRAM_FIX, ID: 372918890, model: 746.
// Short name: AAEPPFXP
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
/// A program: SI_PPFX_PERSON_PROGRAM_FIX.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists, adds, and updates the role of CSE PERSONs on a 
/// specified CASE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPpfxPersonProgramFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PPFX_PERSON_PROGRAM_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPpfxPersonProgramFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPpfxPersonProgramFix.
  /// </summary>
  public SiPpfxPersonProgramFix(IContext context, Import import, Export export):
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
    //                 M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 09/28/99  Carl Ott	        Initial Development - This program is a copy 
    // of PEPR which allows delete of all program types.  It is to be used only
    // to correct conversion or batch errors.
    // ------------------------------------------------------------
    // *************************************************************
    // 11/09/99   C. Ott   PR # 79636  Modified to allow create, update and 
    // delete of programs for inactive person.
    // *************************************************************
    // *************************************************
    // 03/28/00   C. Ott   PR # 91675. Modified to allow add and update of NC 
    // and CC programs.
    // *************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveCommon(import.Prompt, export.Prompt);
    export.Ap.Assign(import.Ap);
    MoveCommon(import.ApPrompt, export.ApPrompt);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    MoveStandard(import.Standard, export.Standard);
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    export.HiddenAp.Number = import.HiddenAp.Number;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.ActiveChild.Flag = import.ActiveChild.Flag;
    export.RecomputeDistribution.Flag = import.RecomputeDistribution.Flag;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailProgram.Assign(
        import.Import1.Item.DetailProgram);
      export.Export1.Update.DetailPersonProgram.Assign(
        import.Import1.Item.DetailPersonProgram);
      MoveCommon(import.Import1.Item.DetailPrompt,
        export.Export1.Update.DetailPrompt);
      export.Export1.Update.DetailCreated.Date =
        import.Import1.Item.DetailCreated.Date;

      if (!IsEmpty(import.HiddenCodeValue.Cdvalue) && AsChar
        (import.Import1.Item.DetailPrompt.SelectChar) == 'S')
      {
        export.Export1.Update.DetailProgram.Code =
          import.HiddenCodeValue.Cdvalue;
        export.Export1.Update.DetailProgram.Title =
          import.HiddenCodeValue.Description;
        export.Export1.Update.DetailPrompt.SelectChar = "";
      }
    }

    import.Import1.CheckIndex();

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      MovePersonProgram3(import.HiddenPageKeys.Item.HiddenPageKeyPersonProgram,
        export.HiddenPageKeys.Update.HiddenPageKeyPersonProgram);
      export.HiddenPageKeys.Update.HiddenPageKeyProgram.Code =
        import.HiddenPageKeys.Item.HiddenPageKeyProgram.Code;
    }

    import.HiddenPageKeys.CheckIndex();

    if (import.HiddenStandard.PageNumber == 0)
    {
      export.HiddenStandard.PageNumber = 1;

      export.HiddenPageKeys.Index = 0;
      export.HiddenPageKeys.CheckSize();
    }

    local.Add.Flag = "N";
    local.Delete.Flag = "N";

    // ============================================================
    // NEXTTRAN AND SECURITY  LOGIC
    // ============================================================
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumberAp = export.Ap.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Ap.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
        export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ------------------------------------------------------------
    // When the control is returned from a List screen,
    // populate the appropriate prompt fields.
    // ------------------------------------------------------------
    if (Equal(global.Command, "RETCOMP"))
    {
      global.Command = "DISPLAY";

      if (AsChar(import.Prompt.SelectChar) == 'S')
      {
        export.Prompt.SelectChar = "+";

        if (!IsEmpty(import.HiddenSelected.Number))
        {
          export.CsePersonsWorkSet.Number = import.HiddenSelected.Number;
        }

        var field = GetField(export.RecomputeDistribution, "flag");

        field.Protected = false;
        field.Focused = true;

        goto Test1;
      }

      if (AsChar(import.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "+";

        var field = GetField(export.Prompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(import.HiddenSelected.Number))
      {
        if (!Equal(import.HiddenSelected.Number, import.HiddenSelChild.Number))
        {
          export.Ap.Number = import.HiddenSelected.Number;
        }
      }

      if (!IsEmpty(import.HiddenSelChild.Number))
      {
        export.CsePersonsWorkSet.Number = import.HiddenSelChild.Number;
      }
    }

Test1:

    if (Equal(global.Command, "RTLIST"))
    {
    }
    else
    {
      // ---------------------------------------------
      //        S E C U R I T Y   L O G I C
      // ---------------------------------------------
      UseScCabTestSecurity();
    }

    UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      for(export.Export1.Index = 1; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        var field1 = GetField(export.Export1.Item.DetailProgram, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.DetailPrompt, "selectChar");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

        field3.Color = "cyan";
        field3.Protected = true;

        if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
          (export.Export1.Item.DetailProgram.Code, "NA") || Equal
          (export.Export1.Item.DetailProgram.Code, "NC"))
        {
          if (Lt(local.ClearPersonProgram.DiscontinueDate,
            export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
            (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
            local.Max.Date))
          {
            // -------------------------------------------------------------------
            // Protect the Program end date, if it has a value.
            // -------------------------------------------------------------------
            var field =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
        else if (Equal(export.Export1.Item.DetailProgram.Code, "NF") || Equal
          (export.Export1.Item.DetailProgram.Code, "FC") || Equal
          (export.Export1.Item.DetailProgram.Code, "CC"))
        {
          // ------------------------------------------------------------
          // Leave 'FC' and 'NF' program end date unprotected.
          // ------------------------------------------------------------
        }
        else
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field.Color = "cyan";
          field.Protected = true;
        }
      }

      export.Export1.CheckIndex();

      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "HISTORY":
        if (IsEmpty(export.RecomputeDistribution.Flag))
        {
          var field = GetField(export.RecomputeDistribution, "flag");

          field.Error = true;

          ExitState = "SI0000_MUST_SEL_RECOMPUTE";

          return;
        }

        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseSiPpfxRebuildPersonProgram();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "DISPLAY";
        }
        else
        {
          return;
        }

        break;
      case "LIST":
        // ------------------------------------------------------------
        // Check to see whether there are any invalid prompts entered
        // ------------------------------------------------------------
        switch(AsChar(export.ApPrompt.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            ++local.Common.Count;

            var field = GetField(export.ApPrompt, "selectChar");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
        }

        switch(AsChar(export.Prompt.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            ++local.Common.Count;

            var field = GetField(export.Prompt, "selectChar");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
        }

        export.Export1.Index = 0;
        export.Export1.CheckSize();

        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          switch(AsChar(export.Export1.Item.DetailPrompt.SelectChar))
          {
            case 'S':
              ++local.Common.Count;

              break;
            case '+':
              break;
            case ' ':
              break;
            default:
              ++local.Common.Count;

              var field =
                GetField(export.Export1.Item.DetailPrompt, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
              }

              break;
          }
        }

        // ------------------------------------------------------------
        // Check to see whether more than one prompt entered
        // ------------------------------------------------------------
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            switch(AsChar(export.ApPrompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              default:
                var field = GetField(export.ApPrompt, "selectChar");

                field.Error = true;

                break;
            }

            switch(AsChar(export.Prompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              default:
                var field = GetField(export.Prompt, "selectChar");

                field.Error = true;

                break;
            }

            export.Export1.Index = 0;
            export.Export1.CheckSize();

            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              switch(AsChar(export.Export1.Item.DetailPrompt.SelectChar))
              {
                case '+':
                  break;
                case ' ':
                  break;
                default:
                  var field =
                    GetField(export.Export1.Item.DetailPrompt, "selectChar");

                  field.Error = true;

                  break;
              }
            }

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ------------------------------------------------------------
        // Set appropriate exit state and escape
        // ------------------------------------------------------------
        if (AsChar(export.ApPrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }
        else
        {
        }

        if (AsChar(export.Prompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }
        else
        {
        }

        export.Export1.Index = 0;
        export.Export1.CheckSize();

        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          switch(AsChar(export.Export1.Item.DetailPrompt.SelectChar))
          {
            case 'S':
              export.HiddenCode.CodeName = "PROGRAM";
              ExitState = "ECO_LNK_TO_CODE_TABLES";

              return;
            case '+':
              break;
            case ' ':
              break;
            default:
              break;
          }
        }

        break;
      case "RTLIST":
        export.Export1.Index = 0;
        export.Export1.CheckSize();

        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          export.Export1.Update.DetailPrompt.SelectChar = "+";

          if (!IsEmpty(import.HiddenCodeValue.Cdvalue))
          {
            export.Export1.Update.DetailProgram.Code =
              import.HiddenCodeValue.Cdvalue;
            export.Export1.Update.DetailProgram.Title =
              import.HiddenCodeValue.Description;
          }
          else
          {
            export.Export1.Update.DetailProgram.Code = "";
            export.Export1.Update.DetailProgram.Title = "";
          }

          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "NEXT":
        if (export.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          break;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.HiddenPageKeyProgram.Code))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.HiddenStandard.PageNumber;

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.HiddenStandard.PageNumber;

        break;
      case "CREATE":
        // ------------------------------------------------------------
        // Validation
        // ------------------------------------------------------------
        for(export.Export1.Index = 1; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          // -----------------------------------------------------------
          // Cannot add an occurrence BELOW line 1, Namely on a line with  
          // existing data
          // ----------------------------------------------------------
          if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "REQUEST_CANNOT_BE_EXECUTED";
          }
        }

        export.Export1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        export.Export1.Index = 0;
        export.Export1.CheckSize();

        if (IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S')
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }

        if (export.Export1.Item.DetailProgram.SystemGeneratedIdentifier != 0
          || !IsEmpty(export.Export1.Item.DetailPersonProgram.LastUpdatedBy))
        {
          // -----------------------------------------------------------
          // Cannot add an occurrence on a line with existing data
          // ----------------------------------------------------------
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CANNOT_ADD_AN_EXISTING_OCCURRENC";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
          (export.Export1.Item.DetailProgram.Code, "NA") || Equal
          (export.Export1.Item.DetailProgram.Code, "NC") || Equal
          (export.Export1.Item.DetailProgram.Code, "CC"))
        {
        }
        else
        {
          if (IsEmpty(export.Export1.Item.DetailProgram.Code))
          {
            ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
          }
          else
          {
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Export1.Item.DetailProgram, "code");

            field.Error = true;

            break;
          }
        }

        // -----------------------------------------------------------
        // Validation that effective date can not be greater than current date.
        // -----------------------------------------------------------
        if (Lt(Now().Date, export.Export1.Item.DetailPersonProgram.EffectiveDate))
          
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Error = true;

          ExitState = "SI0000_INVALID_PERS_PGM_START_DT";

          break;
        }

        if (!Lt(new DateTime(1, 1, 1),
          export.Export1.Item.DetailPersonProgram.EffectiveDate))
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Error = true;

          ExitState = "ACO_NI0000_INVALID_DATE";

          break;
        }

        if (Lt(local.Max.Date,
          export.Export1.Item.DetailPersonProgram.DiscontinueDate))
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field.Error = true;

          ExitState = "ACO_NI0000_INVALID_DATE";

          break;
        }

        // ------------------------------------------------------------
        // Validate the program code entered
        // ------------------------------------------------------------
        if (!IsEmpty(export.Export1.Item.DetailProgram.Code))
        {
          local.Code.CodeName = "PROGRAM";
          local.CodeValue.Cdvalue = export.Export1.Item.DetailProgram.Code;
          UseCabValidateCodeValue();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.Export1.Item.DetailProgram, "code");

            field.Error = true;

            ExitState = "SI0000_SELECTED_PROGRAM_INVALID";
          }
        }
        else
        {
          var field = GetField(export.Export1.Item.DetailProgram, "code");

          field.Error = true;

          ExitState = "PROGRAM_MUST_BE_ENTERED";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (Equal(export.Export1.Item.DetailProgram.Code, "NC"))
        {
          // *************************************************
          // 03/28/00  C. Ott  PR # 91675.  Do not perform the date validation 
          // routine for NC program.
          // *************************************************
        }
        else
        {
          UseSiPeprValidatePersPgmPeriod();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Export1.Index = 0;
            export.Export1.CheckSize();

            if (Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
              local.Max.Date))
            {
              export.Export1.Update.DetailPersonProgram.DiscontinueDate =
                local.Null1.Date;
            }

            var field1 =
              GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");
              

            field1.Error = true;

            var field2 =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field2.Error = true;

            var field3 = GetField(export.Export1.Item.DetailProgram, "code");

            field3.Error = true;

            var field4 =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field4.Error = true;

            break;
          }
        }

        if (IsEmpty(export.RecomputeDistribution.Flag))
        {
          if (Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
            local.Max.Date))
          {
            export.Export1.Update.DetailPersonProgram.DiscontinueDate =
              local.Null1.Date;
          }

          var field = GetField(export.RecomputeDistribution, "flag");

          field.Error = true;

          ExitState = "SI0000_MUST_SEL_RECOMPUTE";

          break;
        }

        UseSiPeprCreatePersonProgram();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          break;
        }

        export.Export1.Update.DetailCommon.SelectChar = "";
        local.Add.Flag = "Y";
        global.Command = "DISPLAY";
        export.HiddenStandard.PageNumber = 1;

        break;
      case "UPDATE":
        // ------------------------------------------------------------
        // Validation
        // ------------------------------------------------------------
        export.Export1.Index = 0;
        export.Export1.CheckSize();

        if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "REQUEST_CANNOT_BE_EXECUTED";

          break;
        }

        if (IsEmpty(export.RecomputeDistribution.Flag))
        {
          var field = GetField(export.RecomputeDistribution, "flag");

          field.Error = true;

          ExitState = "SI0000_MUST_SEL_RECOMPUTE";
        }

        local.Update.Count = 0;

        for(export.Export1.Index = 1; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
          }
          else
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S')
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              }
            }

            if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
              (export.Export1.Item.DetailProgram.Code, "NA") || Equal
              (export.Export1.Item.DetailProgram.Code, "NC") || Equal
              (export.Export1.Item.DetailProgram.Code, "NF") || Equal
              (export.Export1.Item.DetailProgram.Code, "FC") || Equal
              (export.Export1.Item.DetailProgram.Code, "CC"))
            {
            }
            else
            {
              var field = GetField(export.Export1.Item.DetailProgram, "code");

              field.Error = true;

              ExitState = "SI0000_SELECTED_PROGRAM_INVALID";
            }

            if (Lt(local.Max.Date,
              export.Export1.Item.DetailPersonProgram.DiscontinueDate))
            {
              var field =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field.Error = true;

              ExitState = "ACO_NI0000_INVALID_DATE";

              break;
            }

            if (Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
              local.Null1.Date))
            {
              // -----------------------------------------------------------
              // This means that the end date is 12-31-2099
              // -----------------------------------------------------------
            }
            else
            {
              if (Lt(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                export.Export1.Item.DetailPersonProgram.EffectiveDate))
              {
                var field =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "INVALID_DATE_COMBINATION";
                }
              }

              if (Lt(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                export.Export1.Item.DetailPersonProgram.AssignedDate))
              {
                if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I'
                  || Equal(export.Export1.Item.DetailProgram.Code, "NA") || Equal
                  (export.Export1.Item.DetailProgram.Code, "NC") || Equal
                  (export.Export1.Item.DetailProgram.Code, "NF") || Equal
                  (export.Export1.Item.DetailProgram.Code, "FC") || Equal
                  (export.Export1.Item.DetailProgram.Code, "CC"))
                {
                  // -----------------------------------------------------------------
                  //   This condition is not enforced for Interstate, NA or NC 
                  // programs.
                  // ----------------------------------------------------------------
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "INVALID_DATE_COMBINATION";
                  }
                }
              }
            }

            ++local.Update.Count;
          }
        }

        export.Export1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
        else if (local.Update.Count < 1)
        {
          ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

          break;
        }

        export.Export1.Index = 1;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            UseSiPeprUpdatePersonProgram();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              goto Test2;
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
          }
          else
          {
          }

Test2:
          ;
        }

        export.Export1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        // ------------------------------------------------------------
        // Validation
        // ------------------------------------------------------------
        export.Export1.Index = 0;
        export.Export1.CheckSize();

        if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "REQUEST_CANNOT_BE_EXECUTED";

          break;
        }

        if (IsEmpty(export.RecomputeDistribution.Flag))
        {
          var field = GetField(export.RecomputeDistribution, "flag");

          field.Error = true;

          ExitState = "SI0000_MUST_SEL_RECOMPUTE";
        }

        local.Update.Count = 0;

        for(export.Export1.Index = 1; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
          }
          else
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S')
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              }
            }

            ++local.Update.Count;
          }
        }

        export.Export1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (local.Update.Count < 1)
          {
            ExitState = "SI0000_NO_RECORD_SELECTED";

            break;
          }
        }
        else
        {
          break;
        }

        export.Export1.Index = 1;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            UseSiPeprDeletePersonProgram();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              goto Test3;
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
          }
          else
          {
          }

Test3:
          ;
        }

        export.Export1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Delete.Flag = "Y";
          global.Command = "DISPLAY";
        }

        break;
      case "DISPLAY":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ---------------------------------------------
      // If a display is required, call the action
      // block that reads the next group of data based
      // on the page number.
      // ---------------------------------------------
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
        (global.Command, "PREV"))
      {
        // Clear export group views
        export.RecomputeDistribution.Flag = "";

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Export1.Update.DetailCommon.SelectChar =
            local.ClearCommon.SelectChar;
          export.Export1.Update.DetailProgram.Assign(local.ClearProgram);
          export.Export1.Update.DetailPersonProgram.Assign(
            local.ClearPersonProgram);
          MoveCommon(local.ClearCommon, export.Export1.Update.DetailPrompt);
          export.Export1.Update.DetailCreated.Date = local.Null1.Date;
        }

        export.Export1.CheckIndex();

        if (Equal(global.Command, "DISPLAY"))
        {
          export.Export1.Count = 1;
          export.HiddenStandard.PageNumber = 1;

          export.HiddenPageKeys.Index = 0;
          export.HiddenPageKeys.CheckSize();

          export.HiddenPageKeys.Update.HiddenPageKeyProgram.Code =
            local.ClearProgram.Code;
          MovePersonProgram3(local.ClearPersonProgram,
            export.HiddenPageKeys.Update.HiddenPageKeyPersonProgram);
        }

        if (IsEmpty(export.Case1.Number))
        {
          if (IsEmpty(export.Next.Number))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;

            ExitState = "CASE_NUMBER_REQUIRED";

            goto Test5;
          }

          local.TextWorkArea.Text10 = export.Next.Number;
          UseEabPadLeftWithZeros();
          export.Next.Number = local.TextWorkArea.Text10;
          export.Case1.Number = export.Next.Number;
        }

        if (!IsEmpty(export.Next.Number) && !
          Equal(export.Next.Number, export.Case1.Number))
        {
          // Check AP number and Person number to determine if they are
          // valid and associated to the new case number.
          local.TextWorkArea.Text10 = export.Next.Number;
          UseEabPadLeftWithZeros();
          export.Next.Number = local.TextWorkArea.Text10;
          local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
          UseEabPadLeftWithZeros();
          export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
          UseSiCheckCaseToApAndChild();
          export.Case1.Number = export.Next.Number;
        }

        UseSiReadCaseHeaderInformation();

        if (!IsEmpty(local.AbendData.Type1))
        {
          var field = GetField(export.Ap, "number");

          field.Error = true;

          goto Test5;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // -----------------------------------------------
          // Send selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG FLOW.
          // -----------------------------------------------
          export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("NO_APS_ON_A_CASE"))
          {
            local.NoAps.Flag = "Y";
            ExitState = "ACO_NN0000_ALL_OK";

            goto Test4;
          }

          var field = GetField(export.Ap, "number");

          field.Error = true;

          goto Test5;
        }

Test4:

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test5;
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiRetrieveChildForCase();

          if (AsChar(local.MultipleAps.Flag) == 'Y')
          {
            // -----------------------------------------------
            // Send selection needed msg to COMP.  BE SURE
            // TO MATCH next_tran_info ON THE DIALOG FLOW.
            // -----------------------------------------------
            export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }

          if (IsEmpty(export.CsePersonsWorkSet.Number))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "NO_ACTIVE_CHILD";
          }
        }

        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;

        if (ReadCsePerson())
        {
          if (ReadCaseCaseRole())
          {
            if (!Lt(local.Current.Date, entities.Child.StartDate) && !
              Lt(entities.Child.EndDate, local.Current.Date))
            {
              export.ActiveChild.Flag = "Y";
            }
            else
            {
              export.ActiveChild.Flag = "N";
            }

            goto Read;
          }

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "SI0000_CSE_PERSON_NOT_RELTD_CASE";

          goto Test5;
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          goto Test5;
        }

Read:

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
        export.HiddenPageKeys.CheckSize();

        UseSiPeprReadPersonPrograms();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test5;
        }

        export.HiddenAp.Number = export.Ap.Number;
        export.HiddenCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

        export.Export1.Index = 0;
        export.Export1.CheckSize();

        export.Export1.Update.DetailCommon.SelectChar =
          local.ClearCommon.SelectChar;
        export.Export1.Update.DetailProgram.Assign(local.ClearProgram);
        export.Export1.Update.DetailPrompt.SelectChar = "+";
        export.ApPrompt.SelectChar = "+";
        export.Prompt.SelectChar = "+";
        export.Export1.Update.DetailPersonProgram.Assign(
          local.ClearPersonProgram);
        export.Export1.Update.DetailCreated.Date = local.Null1.Date;

        if (export.Export1.IsEmpty)
        {
          goto Test5;
        }
        else
        {
          export.Export1.Index = 1;
          export.Export1.CheckSize();

          if (IsEmpty(export.Export1.Item.DetailProgram.Code))
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            goto Test5;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          if (AsChar(local.NoAps.Flag) == 'Y')
          {
            ExitState = "NO_APS_ON_A_CASE";
          }
        }

        for(export.Export1.Index = 1; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          var field1 = GetField(export.Export1.Item.DetailProgram, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.DetailPrompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          if (!IsEmpty(export.Export1.Item.DetailProgram.Code))
          {
            // ------------------------------------------------------------
            // Protect fields  of existing items
            // ------------------------------------------------------------
            if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
              (export.Export1.Item.DetailProgram.Code, "NA") || Equal
              (export.Export1.Item.DetailProgram.Code, "NC"))
            {
              if (Lt(local.ClearPersonProgram.DiscontinueDate,
                export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
                (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                local.Max.Date))
              {
                // ------------------------------------------------------------
                // Protect the Program end date, if it has a value.
                // ------------------------------------------------------------
                var field =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field.Color = "cyan";
                field.Protected = true;
              }
            }
            else if (Equal(export.Export1.Item.DetailProgram.Code, "NF") || Equal
              (export.Export1.Item.DetailProgram.Code, "FC") || Equal
              (export.Export1.Item.DetailProgram.Code, "CC"))
            {
              // ------------------------------------------------------------
              // Leave 'FC' and 'NF' program end date unprotected.
              // ------------------------------------------------------------
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field.Color = "cyan";
              field.Protected = true;
            }
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        export.Export1.CheckIndex();

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.HiddenPageKeyPersonProgram.EffectiveDate =
          local.PagePersonProgram.EffectiveDate;
        export.HiddenPageKeys.Update.HiddenPageKeyPersonProgram.
          DiscontinueDate = local.PagePersonProgram.DiscontinueDate;
        export.HiddenPageKeys.Update.HiddenPageKeyProgram.Code =
          local.PageProgram.Code;

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState("NO_APS_ON_A_CASE"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Protected = false;
          field.Focused = true;

          if (AsChar(local.Add.Flag) == 'Y')
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
          else if (AsChar(local.Delete.Flag) == 'Y')
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
        }
      }
      else
      {
        if (export.Export1.IsEmpty)
        {
          goto Test5;
        }

        for(export.Export1.Index = 1; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          var field1 = GetField(export.Export1.Item.DetailProgram, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.DetailPrompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          if (IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
            if (!IsEmpty(export.Export1.Item.DetailProgram.Code))
            {
              // ------------------------------------------------------------
              // Protect fields  of existing items
              // ------------------------------------------------------------
              if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
                (export.Export1.Item.DetailProgram.Code, "NA") || Equal
                (export.Export1.Item.DetailProgram.Code, "NC"))
              {
                if (Lt(local.ClearPersonProgram.DiscontinueDate,
                  export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
                  (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                  local.Max.Date))
                {
                  // ---------------------------------------------------------------
                  // Protect the Program end date, if it has a value.
                  // --------------------------------------------------------------
                  var field =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field.Color = "cyan";
                  field.Protected = true;
                }
              }
              else if (Equal(export.Export1.Item.DetailProgram.Code, "NF") || Equal
                (export.Export1.Item.DetailProgram.Code, "FC") || Equal
                (export.Export1.Item.DetailProgram.Code, "CC"))
              {
                // ------------------------------------------------------------
                // Leave 'FC' and 'NF' program end date unprotected.
                // ------------------------------------------------------------
              }
              else
              {
                var field =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field.Color = "cyan";
                field.Protected = true;
              }
            }
            else if (IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
            {
              var field =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field.Color = "cyan";
              field.Protected = true;
            }
          }
        }

        export.Export1.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Protected = false;
          field.Focused = true;
        }
      }
    }

Test5:

    for(export.Export1.Index = 1; export.Export1.Index < Export
      .ExportGroup.Capacity; ++export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      var field1 = GetField(export.Export1.Item.DetailProgram, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Export1.Item.DetailPrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 =
        GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

      field3.Color = "cyan";
      field3.Protected = true;

      if (!IsEmpty(export.Export1.Item.DetailProgram.Code))
      {
        // ------------------------------------------------------------
        // Protect fields  of existing items
        // ------------------------------------------------------------
        if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
          (export.Export1.Item.DetailProgram.Code, "NA") || Equal
          (export.Export1.Item.DetailProgram.Code, "NC"))
        {
          if (Lt(local.ClearPersonProgram.DiscontinueDate,
            export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
            (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
            local.Max.Date))
          {
            // -------------------------------------------------------------------
            // Protect the Program end date, if it has a value.
            // -------------------------------------------------------------------
            var field =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
        else if (Equal(export.Export1.Item.DetailProgram.Code, "NF") || Equal
          (export.Export1.Item.DetailProgram.Code, "FC") || Equal
          (export.Export1.Item.DetailProgram.Code, "CC"))
        {
          // ------------------------------------------------------------
          // Leave 'NF' program end date unprotected.
          // ------------------------------------------------------------
        }
        else
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field.Color = "cyan";
          field.Protected = true;
        }
      }
      else
      {
        var field =
          GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");

        field.Color = "cyan";
        field.Protected = true;
      }
    }

    export.Export1.CheckIndex();
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(SiPeprReadPersonPrograms.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailProgram.Assign(source.DetailProgram);
    target.DetailPrompt.Flag = source.Prompt.Flag;
    target.DetailPersonProgram.Assign(source.DetailPersonProgram);
    target.DetailPrompt2.Flag = source.Prompt2.Flag;
    target.DetailCreated.Date = source.DetailCreated.Date;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MovePersonProgram1(PersonProgram source,
    PersonProgram target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePersonProgram2(PersonProgram source,
    PersonProgram target)
  {
    target.ClosureReason = source.ClosureReason;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePersonProgram3(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
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

    useImport.Case1.Number = export.Next.Number;
    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCheckCaseToApAndChild()
  {
    var useImport = new SiCheckCaseToApAndChild.Import();
    var useExport = new SiCheckCaseToApAndChild.Export();

    useImport.Case1.Number = export.Next.Number;
    useImport.Ap.Number = export.Ap.Number;
    useImport.Child.Number = export.CsePersonsWorkSet.Number;
    useImport.HiddenAp.Number = export.HiddenAp.Number;
    useImport.HiddenChild.Number = export.HiddenCsePersonsWorkSet.Number;

    Call(SiCheckCaseToApAndChild.Execute, useImport, useExport);

    export.Ap.Number = useExport.Ap.Number;
    export.CsePersonsWorkSet.Number = useExport.Child.Number;
  }

  private void UseSiPeprCreatePersonProgram()
  {
    var useImport = new SiPeprCreatePersonProgram.Import();
    var useExport = new SiPeprCreatePersonProgram.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.RecomputeDistribution.Flag = export.RecomputeDistribution.Flag;
    useImport.Program.Code = export.Export1.Item.DetailProgram.Code;
    MovePersonProgram2(export.Export1.Item.DetailPersonProgram,
      useImport.PersonProgram);

    Call(SiPeprCreatePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprDeletePersonProgram()
  {
    var useImport = new SiPeprDeletePersonProgram.Import();
    var useExport = new SiPeprDeletePersonProgram.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.RecomputeDistribution.Flag = export.RecomputeDistribution.Flag;
    MoveProgram(export.Export1.Item.DetailProgram, useImport.Program);
    useImport.PersonProgram.Assign(export.Export1.Item.DetailPersonProgram);

    Call(SiPeprDeletePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprReadPersonPrograms()
  {
    var useImport = new SiPeprReadPersonPrograms.Import();
    var useExport = new SiPeprReadPersonPrograms.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    MovePersonProgram3(export.HiddenPageKeys.Item.HiddenPageKeyPersonProgram,
      useImport.PagePersonProgram);
    useImport.PageProgram.Code =
      export.HiddenPageKeys.Item.HiddenPageKeyProgram.Code;

    Call(SiPeprReadPersonPrograms.Execute, useImport, useExport);

    MovePersonProgram3(useExport.PagePersonProgram, local.PagePersonProgram);
    local.PageProgram.Code = useExport.PageProgram.Code;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseSiPeprUpdatePersonProgram()
  {
    var useImport = new SiPeprUpdatePersonProgram.Import();
    var useExport = new SiPeprUpdatePersonProgram.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.RecomputeDistribution.Flag = export.RecomputeDistribution.Flag;
    MoveProgram(export.Export1.Item.DetailProgram, useImport.Program);
    useImport.PersonProgram.Assign(export.Export1.Item.DetailPersonProgram);

    Call(SiPeprUpdatePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprValidatePersPgmPeriod()
  {
    var useImport = new SiPeprValidatePersPgmPeriod.Import();
    var useExport = new SiPeprValidatePersPgmPeriod.Export();

    useImport.ChCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.ChProgram.Code = export.Export1.Item.DetailProgram.Code;
    MovePersonProgram1(export.Export1.Item.DetailPersonProgram,
      useImport.ChPersonProgram);

    Call(SiPeprValidatePersPgmPeriod.Execute, useImport, useExport);

    MovePersonProgram3(useExport.Ch, export.Export1.Update.DetailPersonProgram);
  }

  private void UseSiPpfxRebuildPersonProgram()
  {
    var useImport = new SiPpfxRebuildPersonProgram.Import();
    var useExport = new SiPpfxRebuildPersonProgram.Export();

    useImport.RecomputeDistribution.Flag = export.RecomputeDistribution.Flag;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiPpfxRebuildPersonProgram.Execute, useImport, useExport);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.Ap.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    export.Ap.Assign(useExport.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private void UseSiRetrieveChildForCase()
  {
    var useImport = new SiRetrieveChildForCase.Import();
    var useExport = new SiRetrieveChildForCase.Export();

    useImport.CaseOpen.Flag = export.CaseOpen.Flag;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiRetrieveChildForCase.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleChildren.Flag;
    export.CsePersonsWorkSet.Number = useExport.Child.Number;
    export.ActiveChild.Flag = useExport.ActiveChild.Flag;
  }

  private bool ReadCaseCaseRole()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailProgram.
      /// </summary>
      [JsonPropertyName("detailProgram")]
      public Program DetailProgram
      {
        get => detailProgram ??= new();
        set => detailProgram = value;
      }

      /// <summary>
      /// A value of DetailPrompt.
      /// </summary>
      [JsonPropertyName("detailPrompt")]
      public Common DetailPrompt
      {
        get => detailPrompt ??= new();
        set => detailPrompt = value;
      }

      /// <summary>
      /// A value of DetailPersonProgram.
      /// </summary>
      [JsonPropertyName("detailPersonProgram")]
      public PersonProgram DetailPersonProgram
      {
        get => detailPersonProgram ??= new();
        set => detailPersonProgram = value;
      }

      /// <summary>
      /// A value of DetailPrompt2.
      /// </summary>
      [JsonPropertyName("detailPrompt2")]
      public Common DetailPrompt2
      {
        get => detailPrompt2 ??= new();
        set => detailPrompt2 = value;
      }

      /// <summary>
      /// A value of DetailCreated.
      /// </summary>
      [JsonPropertyName("detailCreated")]
      public DateWorkArea DetailCreated
      {
        get => detailCreated ??= new();
        set => detailCreated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common detailCommon;
      private Program detailProgram;
      private Common detailPrompt;
      private PersonProgram detailPersonProgram;
      private Common detailPrompt2;
      private DateWorkArea detailCreated;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyPersonProgram.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyPersonProgram")]
      public PersonProgram HiddenPageKeyPersonProgram
      {
        get => hiddenPageKeyPersonProgram ??= new();
        set => hiddenPageKeyPersonProgram = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyProgram.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyProgram")]
      public Program HiddenPageKeyProgram
      {
        get => hiddenPageKeyProgram ??= new();
        set => hiddenPageKeyProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PersonProgram hiddenPageKeyPersonProgram;
      private Program hiddenPageKeyProgram;
    }

    /// <summary>
    /// A value of HiddenSelChild.
    /// </summary>
    [JsonPropertyName("hiddenSelChild")]
    public CsePersonsWorkSet HiddenSelChild
    {
      get => hiddenSelChild ??= new();
      set => hiddenSelChild = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public CsePersonsWorkSet HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of RecomputeDistribution.
    /// </summary>
    [JsonPropertyName("recomputeDistribution")]
    public Common RecomputeDistribution
    {
      get => recomputeDistribution ??= new();
      set => recomputeDistribution = value;
    }

    private CsePersonsWorkSet hiddenSelChild;
    private CsePersonsWorkSet hiddenSelected;
    private CodeValue hiddenCodeValue;
    private Case1 next;
    private Case1 case1;
    private CsePersonsWorkSet ap;
    private Common apPrompt;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common prompt;
    private Standard standard;
    private Standard hiddenStandard;
    private CsePersonsWorkSet hiddenAp;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Common caseOpen;
    private Common activeChild;
    private Common recomputeDistribution;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailProgram.
      /// </summary>
      [JsonPropertyName("detailProgram")]
      public Program DetailProgram
      {
        get => detailProgram ??= new();
        set => detailProgram = value;
      }

      /// <summary>
      /// A value of DetailPrompt.
      /// </summary>
      [JsonPropertyName("detailPrompt")]
      public Common DetailPrompt
      {
        get => detailPrompt ??= new();
        set => detailPrompt = value;
      }

      /// <summary>
      /// A value of DetailPersonProgram.
      /// </summary>
      [JsonPropertyName("detailPersonProgram")]
      public PersonProgram DetailPersonProgram
      {
        get => detailPersonProgram ??= new();
        set => detailPersonProgram = value;
      }

      /// <summary>
      /// A value of DetailPrompt2.
      /// </summary>
      [JsonPropertyName("detailPrompt2")]
      public Common DetailPrompt2
      {
        get => detailPrompt2 ??= new();
        set => detailPrompt2 = value;
      }

      /// <summary>
      /// A value of DetailCreated.
      /// </summary>
      [JsonPropertyName("detailCreated")]
      public DateWorkArea DetailCreated
      {
        get => detailCreated ??= new();
        set => detailCreated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common detailCommon;
      private Program detailProgram;
      private Common detailPrompt;
      private PersonProgram detailPersonProgram;
      private Common detailPrompt2;
      private DateWorkArea detailCreated;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyPersonProgram.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyPersonProgram")]
      public PersonProgram HiddenPageKeyPersonProgram
      {
        get => hiddenPageKeyPersonProgram ??= new();
        set => hiddenPageKeyPersonProgram = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyProgram.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyProgram")]
      public Program HiddenPageKeyProgram
      {
        get => hiddenPageKeyProgram ??= new();
        set => hiddenPageKeyProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PersonProgram hiddenPageKeyPersonProgram;
      private Program hiddenPageKeyProgram;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of HiddenSelChild.
    /// </summary>
    [JsonPropertyName("hiddenSelChild")]
    public CsePersonsWorkSet HiddenSelChild
    {
      get => hiddenSelChild ??= new();
      set => hiddenSelChild = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of RecomputeDistribution.
    /// </summary>
    [JsonPropertyName("recomputeDistribution")]
    public Common RecomputeDistribution
    {
      get => recomputeDistribution ??= new();
      set => recomputeDistribution = value;
    }

    private Common caseOpen;
    private CsePersonsWorkSet hiddenSelChild;
    private Code hiddenCode;
    private Case1 case1;
    private Case1 next;
    private CsePersonsWorkSet ap;
    private Common apPrompt;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common prompt;
    private Standard standard;
    private Standard hiddenStandard;
    private CsePersonsWorkSet hiddenAp;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Common activeChild;
    private Common recomputeDistribution;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of NoAps.
    /// </summary>
    [JsonPropertyName("noAps")]
    public Common NoAps
    {
      get => noAps ??= new();
      set => noAps = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of ClearCommon.
    /// </summary>
    [JsonPropertyName("clearCommon")]
    public Common ClearCommon
    {
      get => clearCommon ??= new();
      set => clearCommon = value;
    }

    /// <summary>
    /// A value of ClearProgram.
    /// </summary>
    [JsonPropertyName("clearProgram")]
    public Program ClearProgram
    {
      get => clearProgram ??= new();
      set => clearProgram = value;
    }

    /// <summary>
    /// A value of ClearPersonProgram.
    /// </summary>
    [JsonPropertyName("clearPersonProgram")]
    public PersonProgram ClearPersonProgram
    {
      get => clearPersonProgram ??= new();
      set => clearPersonProgram = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of PagePersonProgram.
    /// </summary>
    [JsonPropertyName("pagePersonProgram")]
    public PersonProgram PagePersonProgram
    {
      get => pagePersonProgram ??= new();
      set => pagePersonProgram = value;
    }

    /// <summary>
    /// A value of PageProgram.
    /// </summary>
    [JsonPropertyName("pageProgram")]
    public Program PageProgram
    {
      get => pageProgram ??= new();
      set => pageProgram = value;
    }

    /// <summary>
    /// A value of Add.
    /// </summary>
    [JsonPropertyName("add")]
    public Common Add
    {
      get => add ??= new();
      set => add = value;
    }

    /// <summary>
    /// A value of Delete.
    /// </summary>
    [JsonPropertyName("delete")]
    public Common Delete
    {
      get => delete ??= new();
      set => delete = value;
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

    private CsePerson csePerson;
    private DateWorkArea null1;
    private Common noAps;
    private DateWorkArea current;
    private DateWorkArea max;
    private Common clearCommon;
    private Program clearProgram;
    private PersonProgram clearPersonProgram;
    private Common update;
    private Code code;
    private CodeValue codeValue;
    private Common common;
    private AbendData abendData;
    private Common multipleAps;
    private PersonProgram pagePersonProgram;
    private Program pageProgram;
    private Common add;
    private Common delete;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole child;
  }
#endregion
}

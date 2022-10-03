// Program: SI_PEPR_PERSON_PROGRAM_MAINT, ID: 372730672, model: 746.
// Short name: SWEPEPRP
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
/// A program: SI_PEPR_PERSON_PROGRAM_MAINT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists, adds, and updates the role of CSE PERSONs on a 
/// specified CASE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPeprPersonProgramMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PEPR_PERSON_PROGRAM_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPeprPersonProgramMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPeprPersonProgramMaint.
  /// </summary>
  public SiPeprPersonProgramMaint(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 09/13/95  Helen Sharland	Initial Development
    // 02-24-96  Paul Elie -MTW	Modify and Retrofit Security etc.
    // 04/24/96  G. Lofton - MTW	Unit test corrections.
    // 11/02/96  G. Lofton - MTW	Add new security and removed old.
    // 02/25/97  G. Lofton - MTW	Add logic to pad zeroes to child.
    // 06/03/97  Sid			Cleanup and fixes.
    // 10/06/97  Sid 			IDCR # 377
    // 02/25/99  C. Ott		Add Return on Link from IIMC.
    // 05/03/99 W.Campbell		Added code to send selection needed msg to COMP.
    // 				BE SURE TO MATCH next_tran_info ON THE DIALOG FLOW.
    // 06/25/99  C. Ott		Removed IF statement that only allowed delete of a 
    // Program
    // 				on the same day it was added.
    // 08/04/99  Carl Ott		Added validation so that effective date can not be
    // 				greater than current date.
    // 08/12/99  C. Ott		Allow Interstate, NA and NC programs to be closed prior
    // 				to assignment date.
    // 10/01/99  C. Ott		Problem # 76648 CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is
    // 				used to trigger recomputation of distribution when a program
    // 				change is made.
    // 11/18/99  C. Ott		Problem #80470, Fields unprotected after pressing
    // 				PF9 when no return on link.
    // 12/06/99  C. Ott		Problem # 73241.  Change to prevent multiple change of
    // 				case type CSENet transactions from being sent to other state.
    // 03/22/00  C. Ott		Problem # 86027 and # 91319.  Prevent create of AE
    // 				programs that end after 6/1/88.
    // 07/18/00  M. Lachowicz		Problem # 94450.  Do not allow future end date.
    // 07/20/00  M. Lachowicz		Problem # 99617.  Changed max AE programs start 
    // and
    // 				end date to 08-01-1989.
    // 09/06/00  M.Lachowicz		WR # 00188.  Made changes for JJA.
    // 12/19/00  M.Lachowicz		WR # 00188. Made some additional changes for JJA.
    // 08/20/02  GVandy		WR # 020138 Highlight gaps in person program coverage.
    // 04/28/03  GVandy		PR#176992 Modify how person program coverage gaps are 
    // highlighted.
    // ------------------------------------------------------------------------------------------
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
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.Prompt.Flag = import.Prompt.Flag;
    export.Ap.Assign(import.Ap);
    export.ApPrompt.Flag = import.ApPrompt.Flag;
    export.Ar.Assign(import.Ar);
    MoveStandard(import.Standard, export.Standard);
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    export.HiddenAp.Number = import.HiddenAp.Number;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;

    // 09/06/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 09/06/00 M.L End
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.ActiveChild.Flag = import.ActiveChild.Flag;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      // 09/06/00 M.L Start
      export.LastUpdatedBy.Index = import.Import1.Index;
      export.LastUpdatedBy.CheckSize();

      import.LastUpdatedBy.Index = import.Import1.Index;
      import.LastUpdatedBy.CheckSize();

      export.LastUpdatedBy.Update.UserId.Userid =
        import.LastUpdatedBy.Item.UserId.Userid;

      // 09/06/00 M.L End
      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailProgram.Assign(
        import.Import1.Item.DetailProgram);
      export.Export1.Update.DetailPersonProgram.Assign(
        import.Import1.Item.DetailPersonProgram);
      export.Export1.Update.DetailPrompt.Flag =
        import.Import1.Item.DetailPrompt.Flag;
      export.Export1.Update.DetailCreated.Date =
        import.Import1.Item.DetailCreated.Date;

      if (!IsEmpty(import.HiddenCodeValue.Cdvalue) && AsChar
        (import.Import1.Item.DetailPrompt.Flag) == 'S')
      {
        export.Export1.Update.DetailProgram.Code =
          import.HiddenCodeValue.Cdvalue;
        export.Export1.Update.DetailProgram.Title =
          import.HiddenCodeValue.Description;
        export.Export1.Update.DetailPrompt.Flag = "";
      }
    }

    import.Import1.CheckIndex();

    // 09/06/00 M.L Start
    for(import.Original.Index = 0; import.Original.Index < import
      .Original.Count; ++import.Original.Index)
    {
      if (!import.Original.CheckSize())
      {
        break;
      }

      export.Original.Index = import.Original.Index;
      export.Original.CheckSize();

      MoveProgram(import.Original.Item.OriginalDetailProgram,
        export.Original.Update.OriginalDetail);
      export.Original.Update.OriginalDetails.Assign(
        import.Original.Item.OriginalDetailPersonProgram);
    }

    import.Original.CheckIndex();

    // 09/06/00 M.L End
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

      MovePersonProgram4(import.HiddenPageKeys.Item.HiddenPageKeyPersonProgram,
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

      if (AsChar(import.Prompt.Flag) == 'S')
      {
        export.Prompt.Flag = "";

        if (!IsEmpty(import.HiddenSelected.Number))
        {
          export.CsePersonsWorkSet.Number = import.HiddenSelected.Number;

          var field = GetField(export.Prompt, "flag");

          field.Protected = false;
          field.Focused = true;
        }

        goto Test1;
      }

      if (AsChar(import.ApPrompt.Flag) == 'S')
      {
        export.ApPrompt.Flag = "";

        var field = GetField(export.ApPrompt, "flag");

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

    // 09/06/00 M.L Start
    export.Previous.Command = import.Previous.Command;

    if (Equal(global.Command, "RETNATE"))
    {
      local.ReturnFromNate.Flag = "Y";
      global.Command = "DISPLAY";
    }

    // 09/06/00 M.L End
    if (Equal(global.Command, "RTLIST") || Equal(global.Command, "NATE"))
    {
    }
    else
    {
      // ---------------------------------------------
      //        S E C U R I T Y   L O G I C
      // ---------------------------------------------
      UseScCabTestSecurity();

      // 09/06/00 M.L Start
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(global.Command, "COPY"))
        {
          ExitState = "ACO_SI0000_INVALID_PF_KEY";
        }
      }

      // 09/06/00 M.L End
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "DELETE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        // 09/06/00 M.L Start
        // 09/06/00 M.L End
      }

      // 12/19/00 M.L Start
      // 12/19/00 M.L End
    }

    UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "COPY":
        if (IsEmpty(export.CsePersonsWorkSet.Number) || !
          Equal(export.HiddenCsePersonsWorkSet.Number,
          export.CsePersonsWorkSet.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE";

          return;
        }

        local.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabCopyAePrograms();
        }

        for(export.Export1.Index = 1; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          // 09/06/00 M.L Start
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S')
          {
            var field1 = GetField(export.Export1.Item.DetailProgram, "code");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Export1.Item.DetailPrompt, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");
              

            field3.Color = "cyan";
            field3.Protected = true;
          }

          // 09/06/00 M.L End
          // M.L 04/18/00 Start
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            // 09/06/00 M.L Start
            if (Equal(export.Export1.Item.DetailProgram.Code, "NC") || Equal
              (export.Export1.Item.DetailProgram.Code, "NF") || Equal
              (export.Export1.Item.DetailProgram.Code, "FC"))
            {
              export.Original.Index = export.Export1.Index;
              export.Original.CheckSize();

              if (Lt(local.ClearPersonProgram.DiscontinueDate,
                export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
                (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                local.Max.Date))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "effectiveDate");

                field1.Protected = false;

                var field2 =
                  GetField(export.Export1.Item.DetailProgram, "code");

                field2.Color = "cyan";
                field2.Protected = true;
              }
              else
              {
                var field1 =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "effectiveDate");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Export1.Item.DetailProgram, "code");

                field2.Protected = false;

                var field3 = GetField(export.Export1.Item.DetailPrompt, "flag");

                field3.Protected = false;
              }

              var field =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field.Protected = false;
            }

            // 09/06/00 M.L End
            continue;
          }

          // M.L 04/18/00 End
          if (!IsEmpty(export.Export1.Item.DetailProgram.Code))
          {
            // ------------------------------------------------------------
            // Protect fields  of existing items
            // ------------------------------------------------------------
            // 09/06/00 M.L Start
            // Remove condition group_export_detail program code IS EQUAL TO '
            // NC'
            // 09/06/00 M.L End
            if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
              (export.Export1.Item.DetailProgram.Code, "NA"))
            {
              if (Lt(local.ClearPersonProgram.DiscontinueDate,
                export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
                (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                local.Max.Date))
              {
                // ************************************************
                // H27207 - Protect the Program end date, if it has a value.
                // **************************************************
                var field =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field.Color = "cyan";
                field.Protected = true;
              }
            }
            else
            {
              // 09/06/00 M.L Start
              export.Original.Index = export.Export1.Index;
              export.Original.CheckSize();

              if (Equal(export.Export1.Item.DetailProgram.Code, "NC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NF") || Equal
                (export.Export1.Item.DetailProgram.Code, "FC"))
              {
                if (Lt(local.ClearPersonProgram.DiscontinueDate,
                  export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
                  (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                  local.Max.Date))
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "effectiveDate");

                  field3.Protected = false;
                }
                else
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailProgram, "code");

                  field3.Protected = false;

                  var field4 =
                    GetField(export.Export1.Item.DetailPrompt, "flag");

                  field4.Protected = false;
                }

                var field =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field.Protected = false;

                continue;
              }

              // 09/06/00 M.L End
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field2.Color = "cyan";
              field2.Protected = true;
            }
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          // 12/19/00 M.L Start
          export.LastUpdatedBy.Index = export.Export1.Index;
          export.LastUpdatedBy.CheckSize();

          if (ReadServiceProvider())
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Color = "cyan";
            field.Protected = false;
          }

          // 12/19/00 M.L End
        }

        export.Export1.CheckIndex();

        return;
      case "NATE":
        // 09/06/00 M.L Start
        export.Infrastructure.CaseNumber = export.Case1.Number;
        export.Infrastructure.UserId = global.UserId;
        export.Infrastructure.ReferenceDate = local.Current.Date;
        export.ExternalEvent.EventDetailName = "GP";
        export.Infrastructure.CsePersonNumber = export.CsePersonsWorkSet.Number;
        ExitState = "ECO_LNK_TO_NATE";

        return;

        // 09/06/00 M.L End
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        // ------------------------------------------------------------
        // Check to see whether there are any invalid prompts entered
        // ------------------------------------------------------------
        switch(AsChar(export.ApPrompt.Flag))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            ++local.Common.Count;

            var field = GetField(export.ApPrompt, "flag");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
        }

        switch(AsChar(export.Prompt.Flag))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            ++local.Common.Count;

            var field = GetField(export.Prompt, "flag");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
        }

        // 09/06/00 M.L Start
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            switch(AsChar(export.Export1.Item.DetailPrompt.Flag))
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

                var field = GetField(export.Export1.Item.DetailPrompt, "flag");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
                }

                break;
            }
          }
        }

        export.Export1.CheckIndex();

        // 09/06/00 M.L End
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

            if (IsEmpty(export.ApPrompt.Flag))
            {
            }
            else
            {
              var field = GetField(export.ApPrompt, "flag");

              field.Error = true;
            }

            if (IsEmpty(export.Prompt.Flag))
            {
            }
            else
            {
              var field = GetField(export.Prompt, "flag");

              field.Error = true;
            }

            // 09/06/00 M.L Start
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
              {
                switch(AsChar(export.Export1.Item.DetailPrompt.Flag))
                {
                  case '+':
                    break;
                  case ' ':
                    break;
                  default:
                    var field =
                      GetField(export.Export1.Item.DetailPrompt, "flag");

                    field.Error = true;

                    break;
                }
              }
            }

            export.Export1.CheckIndex();

            // 09/06/00 M.L Start
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ------------------------------------------------------------
        // Set appropriate exit state and escape
        // ------------------------------------------------------------
        if (AsChar(export.ApPrompt.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }
        else
        {
        }

        if (AsChar(export.Prompt.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }
        else
        {
        }

        // 09/06/00 M.L Start
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            switch(AsChar(export.Export1.Item.DetailPrompt.Flag))
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
        }

        export.Export1.CheckIndex();

        // 09/06/00 M.L End
        break;
      case "RTLIST":
        // 09/06/00 M.L Start
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailPrompt.Flag = "+";

            if (!IsEmpty(import.HiddenCodeValue.Cdvalue))
            {
              export.Export1.Update.DetailProgram.Code =
                import.HiddenCodeValue.Cdvalue;
              export.Export1.Update.DetailProgram.Title =
                import.HiddenCodeValue.Description;
            }

            var field = GetField(export.Export1.Item.DetailPrompt, "flag");

            field.Protected = false;
            field.Focused = true;

            ExitState = "ECO_XFR_RETURN_FROM_CODE_TABLES";

            break;
          }
        }

        export.Export1.CheckIndex();

        // 09/06/00 M.L End
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

        // **************************************************************
        // 8/4/99    Carl Ott   Added validation so that effective date can not 
        // be greater than current date.
        // **************************************************************
        if (Lt(Now().Date, export.Export1.Item.DetailPersonProgram.EffectiveDate))
          
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Error = true;

          ExitState = "SI0000_INVALID_PERS_PGM_START_DT";

          break;
        }

        // 07/18/2000 M.L Start
        if (Lt(local.Current.Date,
          export.Export1.Item.DetailPersonProgram.DiscontinueDate))
        {
          if (Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
            local.Max.Date))
          {
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field.Error = true;

            ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

            break;
          }
        }

        // 07/18/2000 M.L End
        if (!Lt(local.Null1.Date,
          export.Export1.Item.DetailPersonProgram.EffectiveDate))
        {
          // M.L 09/06/00 Start
          ExitState = "EFFECTIVE_DATE_REQUIRED";

          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Error = true;

          break;

          // M.L 09/06/00 End
        }

        // M.L 04/18/00 Start
        if (Lt(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
          export.Export1.Item.DetailPersonProgram.EffectiveDate) && !
          Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
          local.Null1.Date))
        {
          var field1 =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field1.Error = true;

          var field2 =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field2.Error = true;

          ExitState = "ACO_NE0000_END_LESS_THAN_START";

          break;
        }

        // M.L 04/18/00 End
        // 09/06/00 M.L Start
        UseSiPeprValidateMinMaxDates();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.ValidDiscontinueDate.Flag) == 'N')
          {
            var field =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field.Error = true;
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");
              

            field.Error = true;
          }

          break;
        }

        // 09/06/00 M.L End
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

            ExitState = "INVALID_PROGRAM";
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

        UseSiPeprValidatePersPgmPeriod2();

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
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field2.Error = true;

          var field3 = GetField(export.Export1.Item.DetailProgram, "code");

          field3.Error = true;

          var field4 = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field4.Error = true;

          break;
        }

        local.RecomputeDistribution.Flag = "Y";
        UseSiPeprCreatePersonProgram();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.CsenetPerson.Number = export.CsePersonsWorkSet.Number;
          UseSiPersPgmCreateCsenetTran();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          // 09/06/00 M.L Start
          if (Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
            (export.Export1.Item.DetailProgram.Code, "NF") || Equal
            (export.Export1.Item.DetailProgram.Code, "NC"))
          {
            export.Previous.Command = "CREATE";
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.EventId = 5;
            local.Infrastructure.BusinessObjectCd = "CAS";
            local.Infrastructure.CaseNumber = export.Case1.Number;
            local.Infrastructure.UserId = global.UserId;
            local.Infrastructure.ReferenceDate = local.Current.Date;
            local.Infrastructure.CaseUnitNumber = 0;
            local.Infrastructure.CsePersonNumber =
              export.CsePersonsWorkSet.Number;
            local.Infrastructure.ReasonCode = "WKRMODPEPR";
            local.Infrastructure.EventDetailName =
              export.Export1.Item.DetailProgram.Code + " modified by ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + global.UserId;
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + " - check PEPR/ROLE";
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }

          // 09/06/00 M.L End
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

        local.Update.Count = 0;
        export.Export1.Index = 1;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
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

            // 09/06/00 M.L Start
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              local.SelectionIndicatorCounte.Count =
                (int)((long)local.SelectionIndicatorCounte.Count + 1);
            }

            local.Security.Userid = global.UserId;
            local.CsePerson.Number = export.CsePersonsWorkSet.Number;

            export.Original.Index = export.Export1.Index;
            export.Original.CheckSize();

            if (!Equal(export.Export1.Item.DetailProgram.Code,
              export.Original.Item.OriginalDetail.Code) && !
              Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
              export.Original.Item.OriginalDetails.DiscontinueDate))
            {
              local.ErrorDiscontinueDate.Flag = "Y";
              local.ErrorProgram.Flag = "Y";
              ExitState = "SI_END_DATE_AND_PROGRAM_CHANGED";

              var field1 = GetField(export.Export1.Item.DetailProgram, "code");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field2.Error = true;
            }

            if (!Equal(export.Export1.Item.DetailProgram.Code,
              export.Original.Item.OriginalDetail.Code))
            {
              if (Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NF"))
              {
              }
              else
              {
                var field = GetField(export.Export1.Item.DetailProgram, "code");

                field.Error = true;

                export.Export1.Update.DetailProgram.Code =
                  export.Original.Item.OriginalDetail.Code;
                local.ErrorProgram.Flag = "Y";
                ExitState = "SI_CAN_CHANGE_PRG_TO_FC_NF_NC";
              }
            }

            if ((Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
              (export.Export1.Item.DetailProgram.Code, "NC") || Equal
              (export.Export1.Item.DetailProgram.Code, "NF")) && Equal
              (export.Export1.Item.DetailProgram.Code,
              export.Original.Item.OriginalDetail.Code))
            {
              if (!Equal(export.Export1.Item.DetailPersonProgram.EffectiveDate,
                export.Original.Item.OriginalDetails.EffectiveDate) || !
                Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                export.Original.Item.OriginalDetails.DiscontinueDate))
              {
                if (Lt(local.Current.Date,
                  export.Export1.Item.DetailPersonProgram.DiscontinueDate))
                {
                  var field =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field.Error = true;

                  local.ErrorDiscontinueDate.Flag = "Y";
                  ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
                }

                if (Lt(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                  export.Export1.Item.DetailPersonProgram.EffectiveDate))
                {
                  var field1 =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "effectiveDate");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field2.Error = true;

                  local.ErrorDiscontinueDate.Flag = "Y";
                  ExitState = "ACO_NE0000_END_LESS_THAN_START";
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  goto Test2;
                }

                UseSiPeprValidateMinMaxDates();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  if (AsChar(local.ValidDiscontinueDate.Flag) == 'N')
                  {
                    local.ErrorDiscontinueDate.Flag = "Y";
                  }

                  if (AsChar(local.ValidEffectiveDate.Flag) == 'N')
                  {
                    local.ErrorEffectiveDate.Flag = "Y";
                  }

                  goto Test2;
                }

                UseSiPeprValidatePersPgmPeriod1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.ErrorEffectiveDate.Flag = "Y";
                  local.ErrorDiscontinueDate.Flag = "Y";

                  var field1 =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "effectiveDate");

                  field2.Error = true;
                }
              }
            }

Test2:

            // 09/06/00 M.L End
            if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
              (export.Export1.Item.DetailProgram.Code, "NA") || Equal
              (export.Export1.Item.DetailProgram.Code, "NC"))
            {
            }
            else
            {
              // 09/06/00 M.L Start
              if (Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NF") || Equal
                (export.Export1.Item.DetailProgram.Code, "NC"))
              {
                goto Test3;
              }

              // 09/06/00 M.L End
              var field = GetField(export.Export1.Item.DetailProgram, "code");

              field.Error = true;

              ExitState = "SI0000_SELECTED_PROGRAM_INVALID";
            }

Test3:

            if (Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
              local.Null1.Date))
            {
              // -----------------------------------------------------------
              // This means that the end date is 12-31-2099
              // -----------------------------------------------------------
            }
            else
            {
              // 07/18/2000 M.L Start
              if (Lt(local.Current.Date,
                export.Export1.Item.DetailPersonProgram.DiscontinueDate))
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  var field =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field.Error = true;

                  ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
                }
              }

              // 07/18/2000 M.L End
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
                  (export.Export1.Item.DetailProgram.Code, "NC"))
                {
                  // ***************************************************************
                  // 8/12/99   C. Ott   This condition is not enforced for 
                  // Interstate, NA or NC programs.
                  // ****************************************************************
                }
                else
                {
                  // 09/06/00 M.L Start
                  if (Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
                    (export.Export1.Item.DetailProgram.Code, "NF") || Equal
                    (export.Export1.Item.DetailProgram.Code, "NC"))
                  {
                    goto Test4;
                  }

                  // 09/06/00 M.L End
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

Test4:

            ++local.Update.Count;
          }
        }

        export.Export1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
        else
        {
          // 09/06/00 M.L Start
          if (local.SelectionIndicatorCounte.Count > 1)
          {
            for(export.Export1.Index = 1; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field.Error = true;
              }
            }

            export.Export1.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
          }

          // 09/06/00 M.L End
          if (local.Update.Count < 1)
          {
            ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

            break;
          }
        }

        export.Export1.Index = 1;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Original.Index = export.Export1.Index;
          export.Original.CheckSize();

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (!Equal(export.Export1.Item.DetailProgram.Code,
              export.Original.Item.OriginalDetail.Code))
            {
              export.Export1.Update.DetailPersonProgram.CreatedTimestamp =
                Now();
              export.Export1.Update.DetailPersonProgram.CreatedBy =
                global.UserId;
              local.CsePerson.Number = export.CsePersonsWorkSet.Number;
              UseSiChangePgmInPersonPgm();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Previous.Command = "UPDATE";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.EventId = 5;
                local.Infrastructure.BusinessObjectCd = "CAS";
                local.Infrastructure.CaseNumber = export.Case1.Number;
                local.Infrastructure.UserId = global.UserId;
                local.Infrastructure.ReferenceDate = local.Current.Date;
                local.Infrastructure.CaseUnitNumber = 0;
                local.Infrastructure.CsePersonNumber =
                  export.CsePersonsWorkSet.Number;
                local.Infrastructure.ReasonCode = "WKRMODPEPR";
                local.Infrastructure.EventDetailName =
                  export.Export1.Item.DetailProgram.Code + " modified by ";
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + global.UserId;
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + " - check PEPR/ROLE";
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (ReadProgram())
                {
                  export.Export1.Update.DetailProgram.Title =
                    entities.Program.Title;
                }

                export.Export1.Update.DetailCommon.SelectChar = "";
                ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
                export.Original.Update.OriginalDetails.Assign(
                  export.Export1.Item.DetailPersonProgram);
                MoveProgram(export.Export1.Item.DetailProgram,
                  export.Original.Update.OriginalDetail);

                var field1 =
                  GetField(export.Export1.Item.DetailProgram, "code");

                field1.Protected = false;

                var field2 = GetField(export.Export1.Item.DetailPrompt, "flag");

                field2.Protected = false;

                goto Test5;
              }
              else
              {
                export.Export1.Update.DetailPersonProgram.CreatedTimestamp =
                  export.Original.Item.OriginalDetails.CreatedTimestamp;
                export.Export1.Update.DetailPersonProgram.CreatedBy =
                  export.Original.Item.OriginalDetails.CreatedBy;

                return;
              }
            }

            local.RecomputeDistribution.Flag = "Y";
            UseSiPeprUpdatePersonProgram();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // 09/06/00 M.L Start
              export.LastUpdatedBy.Index = export.Export1.Index;
              export.LastUpdatedBy.CheckSize();

              // 02/02/2001 M. Start
              export.Export1.Update.DetailPersonProgram.LastUpdatedBy =
                global.UserId;

              // 02/02/2001 M. End
              export.LastUpdatedBy.Update.UserId.Userid = global.UserId;

              if (Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NF") || Equal
                (export.Export1.Item.DetailProgram.Code, "NC"))
              {
                export.Previous.Command = "UPDATE";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.EventId = 5;
                local.Infrastructure.BusinessObjectCd = "CAS";
                local.Infrastructure.CaseNumber = export.Case1.Number;
                local.Infrastructure.UserId = global.UserId;
                local.Infrastructure.ReferenceDate = local.Current.Date;
                local.Infrastructure.CaseUnitNumber = 0;
                local.Infrastructure.CsePersonNumber =
                  export.CsePersonsWorkSet.Number;
                local.Infrastructure.ReasonCode = "WKRMODPEPR";
                local.Infrastructure.EventDetailName =
                  export.Export1.Item.DetailProgram.Code + " modified by ";
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + global.UserId;
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + " - check PEPR/ROLE";
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                export.Original.Update.OriginalDetails.Assign(
                  export.Export1.Item.DetailPersonProgram);
                MoveProgram(export.Export1.Item.DetailProgram,
                  export.Original.Update.OriginalDetail);
              }

              // 09/06/00 M.L End
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              goto Test5;
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
          }
          else
          {
          }

Test5:
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
              (export.Export1.Item.DetailProgram.Code, "NC"))
            {
            }
            else
            {
              // 09/06/00 M.L Start
              if (Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NF"))
              {
              }

              // 09/06/00 M.L End
              // 12/19/00 M.L Start
              // 12/19/00 M.L End
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
          ExitState = "SI0000_NO_RECORD_SELECTED";

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
            // ============================================================
            // 06/25/1999    C. Ott   Removed IF statement that only allowed 
            // delete of a Program on the same day it was added.
            // ============================================================
            // **************************************************************
            // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
            // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of
            // distribution when a program change is made.
            // *************************************************************
            local.RecomputeDistribution.Flag = "Y";
            UseSiPeprDeletePersonProgram();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              goto Test6;
            }

            // 09/06/00 M.L Start
            if (Equal(export.Export1.Item.DetailProgram.Code, "FC") || Equal
              (export.Export1.Item.DetailProgram.Code, "NF") || Equal
              (export.Export1.Item.DetailProgram.Code, "NC"))
            {
              export.Previous.Command = "DELETE";
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.EventId = 5;
              local.Infrastructure.BusinessObjectCd = "CAS";
              local.Infrastructure.CaseNumber = export.Case1.Number;
              local.Infrastructure.UserId = global.UserId;
              local.Infrastructure.ReferenceDate = local.Current.Date;
              local.Infrastructure.CaseUnitNumber = 0;
              local.Infrastructure.CsePersonNumber =
                export.CsePersonsWorkSet.Number;
              local.Infrastructure.ReasonCode = "WKRMODPEPR";
              local.Infrastructure.EventDetailName =
                export.Export1.Item.DetailProgram.Code + " modified by ";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + global.UserId;
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + " - check PEPR/ROLE";
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            // 09/06/00 M.L End
            export.Export1.Update.DetailCommon.SelectChar = "";
          }
          else
          {
          }

Test6:
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
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          // 09/06/00 M.L Start
          export.LastUpdatedBy.Index = export.Export1.Index;
          export.LastUpdatedBy.CheckSize();

          export.LastUpdatedBy.Update.UserId.Userid = "";

          // 09/06/00 M.L End
          export.Export1.Update.DetailCommon.SelectChar =
            local.ClearCommon.SelectChar;
          export.Export1.Update.DetailProgram.Assign(local.ClearProgram);
          export.Export1.Update.DetailPersonProgram.Assign(
            local.ClearPersonProgram);
          export.Export1.Update.DetailPrompt.Flag = local.ClearCommon.Flag;
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
          MovePersonProgram4(local.ClearPersonProgram,
            export.HiddenPageKeys.Update.HiddenPageKeyPersonProgram);
        }

        if (IsEmpty(export.Case1.Number))
        {
          if (IsEmpty(export.Next.Number))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;

            ExitState = "CASE_NUMBER_REQUIRED";

            goto Test9;
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

          goto Test9;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
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

            goto Test7;
          }

          var field = GetField(export.Ap, "number");

          field.Error = true;

          goto Test9;
        }

Test7:

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test9;
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiRetrieveChildForCase();

          if (AsChar(local.MultipleAps.Flag) == 'Y')
          {
            // -----------------------------------------------
            // 05/03/99 W.Campbell - Added code to send
            // selection needed msg to COMP.  BE SURE
            // TO MATCH next_tran_info ON THE DIALOG
            // FLOW.
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
          // 06/20/2002 M.Lachowicz Start
          if (CharAt(export.CsePersonsWorkSet.Number, 10) == 'O')
          {
            export.CsePersonsWorkSet.FormattedName =
              entities.CsePerson.OrganizationName ?? Spaces(33);

            var field1 = GetField(export.CsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.CsePersonsWorkSet, "formattedName");

            field2.Error = true;

            ExitState = "SI0000_NO_PP_FOR_ORGANIZATION";

            goto Test9;
          }

          // 06/20/2002 M.Lachowicz End
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

          goto Test9;
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          goto Test9;
        }

Read:

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
        export.HiddenPageKeys.CheckSize();

        UseSiPeprReadPersonPrograms();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test9;
        }

        export.HiddenAp.Number = export.Ap.Number;
        export.HiddenCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

        export.Export1.Index = 0;
        export.Export1.CheckSize();

        export.Export1.Update.DetailCommon.SelectChar =
          local.ClearCommon.SelectChar;
        export.Export1.Update.DetailProgram.Assign(local.ClearProgram);
        export.Export1.Update.DetailPrompt.Flag = "+";
        export.Export1.Update.DetailPersonProgram.Assign(
          local.ClearPersonProgram);
        export.Export1.Update.DetailCreated.Date = local.Null1.Date;

        if (export.Export1.IsEmpty)
        {
          goto Test8;
        }
        else
        {
          export.Export1.Index = 1;
          export.Export1.CheckSize();

          if (IsEmpty(export.Export1.Item.DetailProgram.Code))
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            goto Test8;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          if (AsChar(local.NoAps.Flag) == 'Y')
          {
            ExitState = "NO_APS_ON_A_CASE";
          }

          if (AsChar(export.ActiveChild.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_INACTIVE_CHILD";
          }

          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }

        for(export.Export1.Index = 1; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          // 09/06/00 M.L Start
          export.LastUpdatedBy.Index = export.Export1.Index;
          export.LastUpdatedBy.CheckSize();

          if (IsEmpty(export.Export1.Item.DetailPersonProgram.LastUpdatedBy))
          {
            export.LastUpdatedBy.Update.UserId.Userid =
              export.Export1.Item.DetailPersonProgram.CreatedBy;

            // 02/02/2001 M.L. Start
            export.Export1.Update.DetailPersonProgram.LastUpdatedBy =
              export.LastUpdatedBy.Item.UserId.Userid;

            // 02/02/2001 M.L. End
          }
          else
          {
            export.LastUpdatedBy.Update.UserId.Userid =
              export.Export1.Item.DetailPersonProgram.LastUpdatedBy ?? Spaces
              (8);
          }

          // 09/06/00 M.L End
          var field1 = GetField(export.Export1.Item.DetailProgram, "code");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Export1.Item.DetailPrompt, "flag");

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
                // ************************************************
                // H27207 - Protect the Program end date, if it has a value.
                // **************************************************
                var field =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field.Color = "cyan";
                field.Protected = true;
              }
            }
            else
            {
              // 09/06/00 M.L Start
              if (Equal(export.Export1.Item.DetailProgram.Code, "NC") || Equal
                (export.Export1.Item.DetailProgram.Code, "NF") || Equal
                (export.Export1.Item.DetailProgram.Code, "FC"))
              {
                if (Lt(local.ClearPersonProgram.DiscontinueDate,
                  export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
                  (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                  local.Max.Date))
                {
                  var field6 =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "effectiveDate");

                  field6.Color = "cyan";
                  field6.Protected = false;
                }
                else
                {
                  var field6 =
                    GetField(export.Export1.Item.DetailProgram, "code");

                  field6.Protected = false;

                  var field7 =
                    GetField(export.Export1.Item.DetailPrompt, "flag");

                  field7.Protected = false;
                }

                var field =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field.Color = "cyan";
                field.Protected = false;

                continue;
              }

              // 09/06/00 M.L End
              var field4 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field5.Color = "cyan";
              field5.Protected = true;
            }
          }
          else
          {
            var field4 =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          // 12/19/00 M.L Start
          export.LastUpdatedBy.Index = export.Export1.Index;
          export.LastUpdatedBy.CheckSize();

          if (ReadServiceProvider())
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Protected = false;
          }

          // 12/19/00 M.L End
        }

        export.Export1.CheckIndex();

        // 09/06/00 M.L Start
        for(export.Export1.Index = 1; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          export.Original.Index = export.Export1.Index;
          export.Original.CheckSize();

          MoveProgram(export.Export1.Item.DetailProgram,
            export.Original.Update.OriginalDetail);
          export.Original.Update.OriginalDetails.Assign(
            export.Export1.Item.DetailPersonProgram);
        }

        export.Export1.CheckIndex();

        // 09/06/00 M.L End
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

        // 08/20/02 GVandy  WR # 020138  Highlight gaps in person program 
        // coverage.
        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState
          ("NO_APS_ON_A_CASE") || IsExitState("DISPLAY_OK_FOR_INACTIVE_CHILD"))
        {
          // -- Determine if there are any gaps in the program effective dates.
          if (export.HiddenStandard.PageNumber == 1)
          {
            // -- If this is the first page of data, seed the smallest effective
            // date with the
            // effective date from the repeating group plus one day.
            export.Export1.Index = 1;
            export.Export1.CheckSize();

            local.Smallest.EffectiveDate =
              AddDays(export.Export1.Item.DetailPersonProgram.EffectiveDate, 1);
              
          }
          else
          {
            // -- Determine the smallest effective date that would have been 
            // displayed on any
            // previous pages.
            export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
            export.HiddenPageKeys.CheckSize();

            ReadPersonProgram1();

            if (Equal(local.Smallest.EffectiveDate,
              local.ClearPersonProgram.EffectiveDate))
            {
              export.Export1.Index = 1;
              export.Export1.CheckSize();

              local.Smallest.EffectiveDate =
                export.Export1.Item.DetailPersonProgram.EffectiveDate;
            }
          }

          for(export.Export1.Index = 1; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (IsEmpty(export.Export1.Item.DetailProgram.Code))
            {
              continue;
            }

            // -- Determine if there is a gap between the discontinue date of 
            // this entry and
            // the smallest effective date encountered to this point.
            if (Lt(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
              AddDays(local.Smallest.EffectiveDate, -1)) && !
              Equal(export.Export1.Item.DetailPersonProgram.DiscontinueDate,
              local.ClearPersonProgram.DiscontinueDate))
            {
              // -- A gap exists.  Set a flag to indicate that we need to 
              // highlight this
              // discontinue date and the effective date of the previous row.
              local.HighlightsForGaps.Index = export.Export1.Index;
              local.HighlightsForGaps.CheckSize();

              local.HighlightsForGaps.Update.GlocalHighlightDiscontinueDt.Flag =
                "Y";
              local.GapFound.Flag = "Y";
            }

            if (Lt(export.Export1.Item.DetailPersonProgram.EffectiveDate,
              local.Smallest.EffectiveDate))
            {
              // -- The effective date of this row is the smallest effective 
              // date yet
              // encountered.  Move this effective date to the smallest 
              // effective date view.
              local.Smallest.EffectiveDate =
                export.Export1.Item.DetailPersonProgram.EffectiveDate;

              // Check if there is another program effective on the day before 
              // this program started.
              // If not then we will highlight this effective date to identify 
              // the gap.
              local.SmallestMinusOneDay.EffectiveDate =
                AddDays(local.Smallest.EffectiveDate, -1);

              if (ReadPersonProgram2())
              {
                // -- Another program was effective on the previous day.  No gap
                // exists.  Continue.
              }
              else
              {
                // -- Make sure there is at least one earlier program.  If not, 
                // we are simply at the
                // end of the list and no gap exists.
                if (ReadPersonProgram3())
                {
                  // -- A gap exists.  Highlight this effective date.
                  local.HighlightsForGaps.Index = export.Export1.Index;
                  local.HighlightsForGaps.CheckSize();

                  local.HighlightsForGaps.Update.GlocalHighlightEffectiveDt.
                    Flag = "Y";
                  local.GapFound.Flag = "Y";
                }
                else
                {
                  // -- We are at the end of the list.  There are no other 
                  // person programs.  No gap exists.  Continue.
                }
              }
            }
          }

          export.Export1.CheckIndex();

          if (AsChar(local.GapFound.Flag) == 'Y')
          {
            if (IsExitState("DISPLAY_OK_FOR_INACTIVE_CHILD"))
            {
              ExitState = "DISPLAY_OK_FOR_INACTIVE_CHILD_2";
            }
            else if (IsExitState("NO_APS_ON_A_CASE"))
            {
              ExitState = "NO_APS_ON_CASE_2";
            }
            else if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
            {
              ExitState = "SI0000_PROGRAM_GAP_EXISTS";
            }
            else
            {
            }
          }
        }
      }
      else
      {
        if (export.Export1.IsEmpty)
        {
          goto Test8;
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

          var field2 = GetField(export.Export1.Item.DetailPrompt, "flag");

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
              // 09/06/00 M.L Start
              // Remove condition group_export_detail program_code IS EQUAL TO '
              // NC'
              // 09/06/00 M.L End
              if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
                (export.Export1.Item.DetailProgram.Code, "NA"))
              {
                if (Lt(local.ClearPersonProgram.DiscontinueDate,
                  export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
                  (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                  local.Max.Date))
                {
                  // ************************************************
                  // H27207 - Protect the Program end date, if it has a value.
                  // **************************************************
                  var field =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field.Color = "cyan";
                  field.Protected = true;
                }
              }
              else
              {
                if (Equal(export.Export1.Item.DetailProgram.Code, "NC") || Equal
                  (export.Export1.Item.DetailProgram.Code, "NF") || Equal
                  (export.Export1.Item.DetailProgram.Code, "FC"))
                {
                  if (Lt(local.ClearPersonProgram.DiscontinueDate,
                    export.Export1.Item.DetailPersonProgram.DiscontinueDate) &&
                    Lt
                    (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
                    local.Max.Date))
                  {
                    var field6 =
                      GetField(export.Export1.Item.DetailPersonProgram,
                      "effectiveDate");

                    field6.Color = "cyan";
                    field6.Protected = false;
                  }
                  else
                  {
                    var field6 =
                      GetField(export.Export1.Item.DetailProgram, "code");

                    field6.Protected = false;

                    var field7 =
                      GetField(export.Export1.Item.DetailPrompt, "flag");

                    field7.Protected = false;
                  }

                  var field =
                    GetField(export.Export1.Item.DetailPersonProgram,
                    "discontinueDate");

                  field.Color = "cyan";
                  field.Protected = false;

                  continue;
                }

                var field4 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field4.Color = "cyan";
                field4.Protected = true;

                var field5 =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "discontinueDate");

                field5.Color = "cyan";
                field5.Protected = true;
              }

              // 12/19/00 M.L Start
              export.LastUpdatedBy.Index = export.Export1.Index;
              export.LastUpdatedBy.CheckSize();

              if (ReadServiceProvider())
              {
                var field =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field.Protected = false;
              }

              // 12/19/00 M.L End
            }
            else if (IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
            {
              var field4 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field5.Color = "cyan";
              field5.Protected = true;
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

Test8:

      // 09/06/00 M.L Start
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        export.Original.Index = export.Export1.Index;
        export.Original.CheckSize();

        export.Original.Update.OriginalDetails.Assign(
          export.Export1.Item.DetailPersonProgram);
        MoveProgram(export.Export1.Item.DetailProgram,
          export.Original.Update.OriginalDetail);
      }

      export.Export1.CheckIndex();

      // 09/06/00 M.L End
    }

Test9:

    for(export.Export1.Index = 1; export.Export1.Index < Export
      .ExportGroup.Capacity; ++export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      local.HighlightsForGaps.Index = export.Export1.Index;
      local.HighlightsForGaps.CheckSize();

      // 09/06/00 M.L Start
      if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S')
      {
        var field1 = GetField(export.Export1.Item.DetailProgram, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.DetailPrompt, "flag");

        field2.Color = "cyan";
        field2.Protected = true;

        if (AsChar(local.HighlightsForGaps.Item.GlocalHighlightEffectiveDt.Flag) ==
          'Y')
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Color = "cyan";
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }
        else
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Color = "cyan";
          field.Protected = true;
        }
      }

      // 09/06/00 M.L End
      // M.L 04/18/00 Start
      if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
      {
        // 09/06/00 M.L Start
        if (Equal(export.Export1.Item.DetailProgram.Code, "NC") || Equal
          (export.Export1.Item.DetailProgram.Code, "NF") || Equal
          (export.Export1.Item.DetailProgram.Code, "FC"))
        {
          export.Original.Index = export.Export1.Index;
          export.Original.CheckSize();

          if (Lt(local.ClearPersonProgram.DiscontinueDate,
            export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
            (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
            local.Max.Date))
          {
            var field1 =
              GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");
              

            field1.Protected = false;

            var field2 = GetField(export.Export1.Item.DetailProgram, "code");

            field2.Color = "cyan";
            field2.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Export1.Item.DetailProgram, "code");

            field2.Protected = false;

            var field3 = GetField(export.Export1.Item.DetailPrompt, "flag");

            field3.Protected = false;
          }

          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field.Protected = false;
        }
        else
        {
          // 12/19/00 M.L Start
          if (IsExitState("SI0000_SELECTED_PROGRAM_INVALID"))
          {
            var field1 = GetField(export.Export1.Item.DetailPrompt, "flag");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");
              

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field3.Color = "cyan";
            field3.Intensity = Intensity.Normal;
            field3.Highlighting = Highlighting.Normal;
            field3.Protected = true;

            var field4 = GetField(export.Export1.Item.DetailProgram, "code");

            field4.Color = "red";
            field4.Intensity = Intensity.High;
            field4.Highlighting = Highlighting.ReverseVideo;
            field4.Protected = true;
          }

          // 12/19/00 M.L End
        }

        // 09/06/00 M.L End
        continue;
      }

      // M.L 04/18/00 End
      if (!IsEmpty(export.Export1.Item.DetailProgram.Code))
      {
        // ------------------------------------------------------------
        // Protect fields  of existing items
        // ------------------------------------------------------------
        // 09/06/00 M.L Start
        // Remove condition group_export_detail program code IS EQUAL TO 'NC'
        // 09/06/00 M.L End
        if (CharAt(export.Export1.Item.DetailProgram.Code, 3) == 'I' || Equal
          (export.Export1.Item.DetailProgram.Code, "NA"))
        {
          if (Lt(local.ClearPersonProgram.DiscontinueDate,
            export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
            (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
            local.Max.Date))
          {
            // ************************************************
            // H27207 - Protect the Program end date, if it has a value.
            // **************************************************
            if (AsChar(local.HighlightsForGaps.Item.
              GlocalHighlightDiscontinueDt.Flag) == 'Y')
            {
              var field =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field.Color = "cyan";
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
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
        }
        else
        {
          // 09/06/00 M.L Start
          export.Original.Index = export.Export1.Index;
          export.Original.CheckSize();

          if (Equal(export.Export1.Item.DetailProgram.Code, "NC") || Equal
            (export.Export1.Item.DetailProgram.Code, "NF") || Equal
            (export.Export1.Item.DetailProgram.Code, "FC"))
          {
            if (Lt(local.ClearPersonProgram.DiscontinueDate,
              export.Export1.Item.DetailPersonProgram.DiscontinueDate) && Lt
              (export.Export1.Item.DetailPersonProgram.DiscontinueDate,
              local.Max.Date))
            {
              if (AsChar(local.HighlightsForGaps.Item.
                GlocalHighlightEffectiveDt.Flag) == 'Y')
              {
                var field1 =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "effectiveDate");

                field1.Highlighting = Highlighting.ReverseVideo;
                field1.Protected = false;
              }
              else
              {
                var field1 =
                  GetField(export.Export1.Item.DetailPersonProgram,
                  "effectiveDate");

                field1.Color = "";
                field1.Protected = false;
              }
            }
            else
            {
              var field1 = GetField(export.Export1.Item.DetailProgram, "code");

              field1.Protected = false;

              var field2 = GetField(export.Export1.Item.DetailPrompt, "flag");

              field2.Protected = false;
            }

            if (AsChar(local.HighlightsForGaps.Item.
              GlocalHighlightDiscontinueDt.Flag) == 'Y')
            {
              var field1 =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = false;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.DetailPersonProgram,
                "discontinueDate");

              field1.Color = "";
              field1.Protected = false;
            }

            continue;
          }

          // 09/06/00 M.L End
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Color = "cyan";
          field.Protected = true;

          if (AsChar(local.HighlightsForGaps.Item.GlocalHighlightDiscontinueDt.
            Flag) == 'Y')
          {
            var field1 =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field1.Color = "cyan";
            field1.Highlighting = Highlighting.ReverseVideo;
            field1.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.DetailPersonProgram,
              "discontinueDate");

            field1.Color = "cyan";
            field1.Protected = true;
          }
        }

        // 12/19/00 M.L Start
        export.LastUpdatedBy.Index = export.Export1.Index;
        export.LastUpdatedBy.CheckSize();

        if (ReadServiceProvider())
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Protected = false;
        }

        // 12/19/00 M.L End
      }
      else
      {
        var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

        field.Color = "cyan";
        field.Protected = true;

        if (AsChar(local.HighlightsForGaps.Item.GlocalHighlightDiscontinueDt.
          Flag) == 'Y')
        {
          var field1 =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field1.Color = "cyan";
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = true;
        }
        else
        {
          var field1 =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field1.Color = "cyan";
          field1.Protected = true;
        }
      }
    }

    export.Export1.CheckIndex();

    // 09/06/00 M.L Start
    for(export.Export1.Index = 1; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
      {
        if (AsChar(local.ErrorDiscontinueDate.Flag) == 'Y')
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "discontinueDate");
            

          field.Error = true;
        }

        if (AsChar(local.ErrorEffectiveDate.Flag) == 'Y')
        {
          var field =
            GetField(export.Export1.Item.DetailPersonProgram, "effectiveDate");

          field.Error = true;
        }

        if (AsChar(local.ErrorProgram.Flag) == 'Y')
        {
          var field = GetField(export.Export1.Item.DetailProgram, "code");

          field.Error = true;
        }

        return;
      }
    }

    export.Export1.CheckIndex();

    // 09/06/00 M.L End
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
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

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
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
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MovePersonProgram2(PersonProgram source,
    PersonProgram target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePersonProgram3(PersonProgram source,
    PersonProgram target)
  {
    target.ClosureReason = source.ClosureReason;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePersonProgram4(PersonProgram source,
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

  private void UseEabCopyAePrograms()
  {
    var useImport = new EabCopyAePrograms.Import();
    var useExport = new EabCopyAePrograms.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.ReturnCode.NumericReturnCode = local.ReturnCode.NumericReturnCode;
    useExport.AbendData.Assign(local.AbendData);

    Call(EabCopyAePrograms.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.ReturnCode.NumericReturnCode;
    local.AbendData.Assign(useExport.AbendData);
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

  private void UseSiChangePgmInPersonPgm()
  {
    var useImport = new SiChangePgmInPersonPgm.Import();
    var useExport = new SiChangePgmInPersonPgm.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveProgram(export.Original.Item.OriginalDetail, useImport.OldProgram);
    useImport.OldPersonProgram.CreatedTimestamp =
      export.Original.Item.OriginalDetails.CreatedTimestamp;
    MoveProgram(export.Export1.Item.DetailProgram, useImport.NewProgram);
    MovePersonProgram1(export.Export1.Item.DetailPersonProgram,
      useImport.NewPersonProgram);

    Call(SiChangePgmInPersonPgm.Execute, useImport, useExport);
  }

  private void UseSiCheckCaseToApAndChild()
  {
    var useImport = new SiCheckCaseToApAndChild.Import();
    var useExport = new SiCheckCaseToApAndChild.Export();

    useImport.Case1.Number = export.Next.Number;
    useImport.Ap.Number = export.Ap.Number;
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useImport.Child);
    useImport.HiddenAp.Number = export.HiddenAp.Number;
    useImport.HiddenChild.Number = export.HiddenCsePersonsWorkSet.Number;

    Call(SiCheckCaseToApAndChild.Execute, useImport, useExport);

    export.Ap.Number = useExport.Ap.Number;
    MoveCsePersonsWorkSet(useExport.Child, export.CsePersonsWorkSet);
  }

  private void UseSiPeprCreatePersonProgram()
  {
    var useImport = new SiPeprCreatePersonProgram.Import();
    var useExport = new SiPeprCreatePersonProgram.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Program.Code = export.Export1.Item.DetailProgram.Code;
    MovePersonProgram3(export.Export1.Item.DetailPersonProgram,
      useImport.PersonProgram);
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiPeprCreatePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprDeletePersonProgram()
  {
    var useImport = new SiPeprDeletePersonProgram.Import();
    var useExport = new SiPeprDeletePersonProgram.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveProgram(export.Export1.Item.DetailProgram, useImport.Program);
    useImport.PersonProgram.Assign(export.Export1.Item.DetailPersonProgram);
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiPeprDeletePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprReadPersonPrograms()
  {
    var useImport = new SiPeprReadPersonPrograms.Import();
    var useExport = new SiPeprReadPersonPrograms.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    MovePersonProgram4(export.HiddenPageKeys.Item.HiddenPageKeyPersonProgram,
      useImport.PagePersonProgram);
    useImport.PageProgram.Code =
      export.HiddenPageKeys.Item.HiddenPageKeyProgram.Code;

    Call(SiPeprReadPersonPrograms.Execute, useImport, useExport);

    MovePersonProgram4(useExport.PagePersonProgram, local.PagePersonProgram);
    local.PageProgram.Code = useExport.PageProgram.Code;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseSiPeprUpdatePersonProgram()
  {
    var useImport = new SiPeprUpdatePersonProgram.Import();
    var useExport = new SiPeprUpdatePersonProgram.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveProgram(export.Export1.Item.DetailProgram, useImport.Program);
    useImport.PersonProgram.Assign(export.Export1.Item.DetailPersonProgram);
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiPeprUpdatePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprValidateMinMaxDates()
  {
    var useImport = new SiPeprValidateMinMaxDates.Import();
    var useExport = new SiPeprValidateMinMaxDates.Export();

    useImport.ChProgram.Code = export.Export1.Item.DetailProgram.Code;
    MovePersonProgram4(export.Export1.Item.DetailPersonProgram,
      useImport.ChPersonProgram);

    Call(SiPeprValidateMinMaxDates.Execute, useImport, useExport);

    local.ValidDiscontinueDate.Flag = useExport.ValidDiscontinueDate.Flag;
    local.ValidEffectiveDate.Flag = useExport.ValidEffectiveDate.Flag;
  }

  private void UseSiPeprValidatePersPgmPeriod1()
  {
    var useImport = new SiPeprValidatePersPgmPeriod.Import();
    var useExport = new SiPeprValidatePersPgmPeriod.Export();

    useImport.ChCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.ChProgram.Code = export.Export1.Item.DetailProgram.Code;
    MovePersonProgram2(export.Export1.Item.DetailPersonProgram,
      useImport.ChPersonProgram);

    Call(SiPeprValidatePersPgmPeriod.Execute, useImport, useExport);
  }

  private void UseSiPeprValidatePersPgmPeriod2()
  {
    var useImport = new SiPeprValidatePersPgmPeriod.Import();
    var useExport = new SiPeprValidatePersPgmPeriod.Export();

    useImport.ChCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.ChProgram.Code = export.Export1.Item.DetailProgram.Code;
    MovePersonProgram2(export.Export1.Item.DetailPersonProgram,
      useImport.ChPersonProgram);

    Call(SiPeprValidatePersPgmPeriod.Execute, useImport, useExport);

    MovePersonProgram4(useExport.Ch, export.Export1.Update.DetailPersonProgram);
  }

  private void UseSiPersPgmCreateCsenetTran()
  {
    var useImport = new SiPersPgmCreateCsenetTran.Import();
    var useExport = new SiPersPgmCreateCsenetTran.Export();

    useImport.CsePerson.Number = local.CsenetPerson.Number;
    useImport.Program.Code = export.Export1.Item.DetailProgram.Code;

    Call(SiPersPgmCreateCsenetTran.Execute, useImport, useExport);
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
    export.Ar.Assign(useExport.Ar);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
  }

  private void UseSiRetrieveChildForCase()
  {
    var useImport = new SiRetrieveChildForCase.Import();
    var useExport = new SiRetrieveChildForCase.Export();

    useImport.CaseOpen.Flag = export.CaseOpen.Flag;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiRetrieveChildForCase.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleChildren.Flag;
    MoveCsePersonsWorkSet(useExport.Child, export.CsePersonsWorkSet);
    export.ActiveChild.Flag = useExport.ActiveChild.Flag;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, export.Infrastructure);
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
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
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
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadPersonProgram1()
  {
    local.Smallest.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          export.HiddenPageKeys.Item.HiddenPageKeyPersonProgram.DiscontinueDate.
            GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          export.HiddenPageKeys.Item.HiddenPageKeyPersonProgram.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "code",
          export.HiddenPageKeys.Item.HiddenPageKeyProgram.Code);
      },
      (db, reader) =>
      {
        local.Smallest.EffectiveDate = db.GetDate(reader, 0);
        local.Smallest.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.Other.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
        db.SetDate(
          command, "effectiveDate",
          local.SmallestMinusOneDay.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.CspNumber = db.GetString(reader, 0);
        entities.Other.EffectiveDate = db.GetDate(reader, 1);
        entities.Other.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Other.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Other.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Other.Populated = true;
      });
  }

  private bool ReadPersonProgram3()
  {
    entities.Other.Populated = false;

    return Read("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.Smallest.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.CspNumber = db.GetString(reader, 0);
        entities.Other.EffectiveDate = db.GetDate(reader, 1);
        entities.Other.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Other.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Other.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Other.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetString(command, "code", export.Export1.Item.DetailProgram.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Title = db.GetString(reader, 2);
        entities.Program.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "userId", export.LastUpdatedBy.Item.UserId.Userid);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A OriginalGroup group.</summary>
    [Serializable]
    public class OriginalGroup
    {
      /// <summary>
      /// A value of OriginalDetailProgram.
      /// </summary>
      [JsonPropertyName("originalDetailProgram")]
      public Program OriginalDetailProgram
      {
        get => originalDetailProgram ??= new();
        set => originalDetailProgram = value;
      }

      /// <summary>
      /// A value of OriginalDetailPersonProgram.
      /// </summary>
      [JsonPropertyName("originalDetailPersonProgram")]
      public PersonProgram OriginalDetailPersonProgram
      {
        get => originalDetailPersonProgram ??= new();
        set => originalDetailPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Program originalDetailProgram;
      private PersonProgram originalDetailPersonProgram;
    }

    /// <summary>A LastUpdatedByGroup group.</summary>
    [Serializable]
    public class LastUpdatedByGroup
    {
      /// <summary>
      /// A value of UserId.
      /// </summary>
      [JsonPropertyName("userId")]
      public Security2 UserId
      {
        get => userId ??= new();
        set => userId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Security2 userId;
    }

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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// Gets a value of Original.
    /// </summary>
    [JsonIgnore]
    public Array<OriginalGroup> Original => original ??= new(
      OriginalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Original for json serialization.
    /// </summary>
    [JsonPropertyName("original")]
    [Computed]
    public IList<OriginalGroup> Original_Json
    {
      get => original;
      set => Original.Assign(value);
    }

    /// <summary>
    /// Gets a value of LastUpdatedBy.
    /// </summary>
    [JsonIgnore]
    public Array<LastUpdatedByGroup> LastUpdatedBy => lastUpdatedBy ??= new(
      LastUpdatedByGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LastUpdatedBy for json serialization.
    /// </summary>
    [JsonPropertyName("lastUpdatedBy")]
    [Computed]
    public IList<LastUpdatedByGroup> LastUpdatedBy_Json
    {
      get => lastUpdatedBy;
      set => LastUpdatedBy.Assign(value);
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

    private WorkArea headerLine;
    private Common previous;
    private Array<OriginalGroup> original;
    private Array<LastUpdatedByGroup> lastUpdatedBy;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A OriginalGroup group.</summary>
    [Serializable]
    public class OriginalGroup
    {
      /// <summary>
      /// A value of OriginalDetail.
      /// </summary>
      [JsonPropertyName("originalDetail")]
      public Program OriginalDetail
      {
        get => originalDetail ??= new();
        set => originalDetail = value;
      }

      /// <summary>
      /// A value of OriginalDetails.
      /// </summary>
      [JsonPropertyName("originalDetails")]
      public PersonProgram OriginalDetails
      {
        get => originalDetails ??= new();
        set => originalDetails = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Program originalDetail;
      private PersonProgram originalDetails;
    }

    /// <summary>A LastUpdatedByGroup group.</summary>
    [Serializable]
    public class LastUpdatedByGroup
    {
      /// <summary>
      /// A value of UserId.
      /// </summary>
      [JsonPropertyName("userId")]
      public Security2 UserId
      {
        get => userId ??= new();
        set => userId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Security2 userId;
    }

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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of ExternalEvent.
    /// </summary>
    [JsonPropertyName("externalEvent")]
    public Infrastructure ExternalEvent
    {
      get => externalEvent ??= new();
      set => externalEvent = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// Gets a value of Original.
    /// </summary>
    [JsonIgnore]
    public Array<OriginalGroup> Original => original ??= new(
      OriginalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Original for json serialization.
    /// </summary>
    [JsonPropertyName("original")]
    [Computed]
    public IList<OriginalGroup> Original_Json
    {
      get => original;
      set => Original.Assign(value);
    }

    /// <summary>
    /// Gets a value of LastUpdatedBy.
    /// </summary>
    [JsonIgnore]
    public Array<LastUpdatedByGroup> LastUpdatedBy => lastUpdatedBy ??= new(
      LastUpdatedByGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LastUpdatedBy for json serialization.
    /// </summary>
    [JsonPropertyName("lastUpdatedBy")]
    [Computed]
    public IList<LastUpdatedByGroup> LastUpdatedBy_Json
    {
      get => lastUpdatedBy;
      set => LastUpdatedBy.Assign(value);
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

    private WorkArea headerLine;
    private Infrastructure externalEvent;
    private Common previous;
    private Infrastructure infrastructure;
    private Array<OriginalGroup> original;
    private Array<LastUpdatedByGroup> lastUpdatedBy;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A HighlightsForGapsGroup group.</summary>
    [Serializable]
    public class HighlightsForGapsGroup
    {
      /// <summary>
      /// A value of GlocalHighlightDiscontinueDt.
      /// </summary>
      [JsonPropertyName("glocalHighlightDiscontinueDt")]
      public Common GlocalHighlightDiscontinueDt
      {
        get => glocalHighlightDiscontinueDt ??= new();
        set => glocalHighlightDiscontinueDt = value;
      }

      /// <summary>
      /// A value of GlocalHighlightEffectiveDt.
      /// </summary>
      [JsonPropertyName("glocalHighlightEffectiveDt")]
      public Common GlocalHighlightEffectiveDt
      {
        get => glocalHighlightEffectiveDt ??= new();
        set => glocalHighlightEffectiveDt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common glocalHighlightDiscontinueDt;
      private Common glocalHighlightEffectiveDt;
    }

    /// <summary>
    /// A value of SmallestMinusOneDay.
    /// </summary>
    [JsonPropertyName("smallestMinusOneDay")]
    public PersonProgram SmallestMinusOneDay
    {
      get => smallestMinusOneDay ??= new();
      set => smallestMinusOneDay = value;
    }

    /// <summary>
    /// A value of GapFound.
    /// </summary>
    [JsonPropertyName("gapFound")]
    public Common GapFound
    {
      get => gapFound ??= new();
      set => gapFound = value;
    }

    /// <summary>
    /// Gets a value of HighlightsForGaps.
    /// </summary>
    [JsonIgnore]
    public Array<HighlightsForGapsGroup> HighlightsForGaps =>
      highlightsForGaps ??= new(HighlightsForGapsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HighlightsForGaps for json serialization.
    /// </summary>
    [JsonPropertyName("highlightsForGaps")]
    [Computed]
    public IList<HighlightsForGapsGroup> HighlightsForGaps_Json
    {
      get => highlightsForGaps;
      set => HighlightsForGaps.Assign(value);
    }

    /// <summary>
    /// A value of Largest.
    /// </summary>
    [JsonPropertyName("largest")]
    public PersonProgram Largest
    {
      get => largest ??= new();
      set => largest = value;
    }

    /// <summary>
    /// A value of Smallest.
    /// </summary>
    [JsonPropertyName("smallest")]
    public PersonProgram Smallest
    {
      get => smallest ??= new();
      set => smallest = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of SelectionIndicatorCounte.
    /// </summary>
    [JsonPropertyName("selectionIndicatorCounte")]
    public Common SelectionIndicatorCounte
    {
      get => selectionIndicatorCounte ??= new();
      set => selectionIndicatorCounte = value;
    }

    /// <summary>
    /// A value of ErrorDiscontinueDate.
    /// </summary>
    [JsonPropertyName("errorDiscontinueDate")]
    public Common ErrorDiscontinueDate
    {
      get => errorDiscontinueDate ??= new();
      set => errorDiscontinueDate = value;
    }

    /// <summary>
    /// A value of ErrorEffectiveDate.
    /// </summary>
    [JsonPropertyName("errorEffectiveDate")]
    public Common ErrorEffectiveDate
    {
      get => errorEffectiveDate ??= new();
      set => errorEffectiveDate = value;
    }

    /// <summary>
    /// A value of ErrorProgram.
    /// </summary>
    [JsonPropertyName("errorProgram")]
    public Common ErrorProgram
    {
      get => errorProgram ??= new();
      set => errorProgram = value;
    }

    /// <summary>
    /// A value of ReturnFromNate.
    /// </summary>
    [JsonPropertyName("returnFromNate")]
    public Common ReturnFromNate
    {
      get => returnFromNate ??= new();
      set => returnFromNate = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    /// <summary>
    /// A value of ValidDiscontinueDate.
    /// </summary>
    [JsonPropertyName("validDiscontinueDate")]
    public Common ValidDiscontinueDate
    {
      get => validDiscontinueDate ??= new();
      set => validDiscontinueDate = value;
    }

    /// <summary>
    /// A value of ValidEffectiveDate.
    /// </summary>
    [JsonPropertyName("validEffectiveDate")]
    public Common ValidEffectiveDate
    {
      get => validEffectiveDate ??= new();
      set => validEffectiveDate = value;
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

    /// <summary>
    /// A value of CsenetPerson.
    /// </summary>
    [JsonPropertyName("csenetPerson")]
    public CsePerson CsenetPerson
    {
      get => csenetPerson ??= new();
      set => csenetPerson = value;
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

    private PersonProgram smallestMinusOneDay;
    private Common gapFound;
    private Array<HighlightsForGapsGroup> highlightsForGaps;
    private PersonProgram largest;
    private PersonProgram smallest;
    private CsePersonsWorkSet csePersonsWorkSet;
    private External returnCode;
    private Common selectionIndicatorCounte;
    private Common errorDiscontinueDate;
    private Common errorEffectiveDate;
    private Common errorProgram;
    private Common returnFromNate;
    private Infrastructure infrastructure;
    private CsePerson csePerson;
    private Security2 security;
    private Common validDiscontinueDate;
    private Common validEffectiveDate;
    private Common recomputeDistribution;
    private CsePerson csenetPerson;
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
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public PersonProgram Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    private PersonProgram other;
    private PersonProgram personProgram;
    private ServiceProvider serviceProvider;
    private Program program;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole child;
  }
#endregion
}

// Program: OE_IMHH_LIST_HH_INFO, ID: 374456911, model: 746.
// Short name: SWEIMHHP
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
/// A program: OE_IMHH_LIST_HH_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeImhhListHhInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_IMHH_LIST_HH_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeImhhListHhInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeImhhListHhInfo.
  /// </summary>
  public OeImhhListHhInfo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // Govind    	12/27/96	Initial Code.
    // Sid		06/24/97	Changes.
    // MK		10/98		REDESIGN
    // Mike Fangman         04/00         Redesign for PRWORA.
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ****  Move imports to exports  ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.SearchCriteria.SearchCase.Number =
      import.SearchCriteria.SearchCase.Number;

    if (IsEmpty(import.SearchCriteria.SearchCsePersonsWorkSet.Number) && !
      IsEmpty(import.SearchCriteria.SearchCsePerson.Number))
    {
      export.SearchCriteria.SearchCsePersonsWorkSet.Number =
        import.SearchCriteria.SearchCsePerson.Number;
    }
    else
    {
      MoveCsePersonsWorkSet(import.SearchCriteria.SearchCsePersonsWorkSet,
        export.SearchCriteria.SearchCsePersonsWorkSet);
    }

    export.SearchCriteria.SearchLegalAction.StandardNumber =
      import.SearchCriteria.SearchLegalAction.StandardNumber;
    export.SearchCriteria.SearchImHousehold.AeCaseNo =
      import.SearchCriteria.SearchImHousehold.AeCaseNo;
    export.Prompt.CseCase.PromptField = import.Prompt.CseCaseNumber.PromptField;
    export.Prompt.CsePerson.PromptField = import.Prompt.CsePerson.PromptField;
    export.Prompt.CourtOrder.PromptField = import.Prompt.CourtOrder.PromptField;

    if (Equal(global.Command, "RETLACS"))
    {
      if (!IsEmpty(import.FromLacsCase.Number))
      {
        export.SearchCriteria.SearchCase.Number = import.FromLacsCase.Number;
      }

      if (!IsEmpty(import.FromLacsLegalAction.StandardNumber))
      {
        export.SearchCriteria.SearchLegalAction.StandardNumber =
          import.FromLacsLegalAction.StandardNumber ?? "";
      }

      global.Command = "DISPLAY";
    }

    if (!import.Grp.IsEmpty)
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        goto Test;
      }

      export.SearchCriteria.SearchImHousehold.Assign(
        import.SearchCriteria.SearchImHousehold);
      export.Hidden.HiddenCase.Number = import.Hidden.HiddenCase.Number;
      export.Hidden.HiddenCsePersonsWorkSet.Number =
        import.Hidden.HiddenCsePersonsWorkSet.Number;
      export.Hidden.HiddenLegalAction.StandardNumber =
        import.Hidden.HiddenLegalAction.StandardNumber;
      export.Hidden.HiddenImHousehold.AeCaseNo =
        import.Hidden.HiddenImHousehold.AeCaseNo;

      export.Grp.Index = 0;
      export.Grp.Clear();

      for(import.Grp.Index = 0; import.Grp.Index < import.Grp.Count; ++
        import.Grp.Index)
      {
        if (export.Grp.IsFull)
        {
          break;
        }

        export.Grp.Update.DtlCommon.SelectChar =
          import.Grp.Item.DtlCommon.SelectChar;
        export.Grp.Update.DtlImHousehold.AeCaseNo =
          import.Grp.Item.DtlImHousehold.AeCaseNo;
        export.Grp.Update.DtlImHouseholdMbrMnthlySum.Relationship =
          import.Grp.Item.DtlImHouseholdMbrMnthlySum.Relationship;
        export.Grp.Update.DtlCsePerson.Number =
          import.Grp.Item.DtlCsePerson.Number;
        export.Grp.Update.DtlCsePersonsWorkSet.FormattedName =
          import.Grp.Item.DtlCsePersonsWorkSet.FormattedName;
        export.Grp.Update.DtlFrom.TextMonthYear =
          import.Grp.Item.DtlFrom.TextMonthYear;
        export.Grp.Update.DtlTo.TextMonthYear =
          import.Grp.Item.DtlTo.TextMonthYear;
        export.Grp.Update.DtlLegalAction.StandardNumber =
          import.Grp.Item.DtlLegalAction.StandardNumber;
        export.Grp.Update.DtlMultCtOrdMsg.Text30 =
          import.Grp.Item.DtlMultCtOrdMsg.Text30;
        export.Grp.Next();
      }
    }

Test:

    if (!IsEmpty(export.SearchCriteria.SearchCase.Number))
    {
      // --- Pad cse case number left with 0's
      local.TextWorkArea.Text10 = export.SearchCriteria.SearchCase.Number;
      UseEabPadLeftWithZeros();
      export.SearchCriteria.SearchCase.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.SearchCriteria.SearchCsePersonsWorkSet.Number))
    {
      // --- Pad cse person number left with 0's
      local.TextWorkArea.Text10 =
        export.SearchCriteria.SearchCsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.SearchCriteria.SearchCsePersonsWorkSet.Number =
        local.TextWorkArea.Text10;
      local.CsePerson.Number = local.TextWorkArea.Text10;
    }
    else
    {
      export.SearchCriteria.SearchCsePersonsWorkSet.FormattedName = "";
    }

    if (!IsEmpty(export.SearchCriteria.SearchImHousehold.AeCaseNo))
    {
      // --- Pad AE case number left with 0's
      local.TextWorkArea.Text10 =
        export.SearchCriteria.SearchImHousehold.AeCaseNo;
      UseEabPadLeftWithZeros();

      if (TextWorkArea.Text10_MaxLength > 9)
      {
        export.SearchCriteria.SearchImHousehold.AeCaseNo =
          Substring(local.TextWorkArea.Text10, 3, 8);
      }
      else
      {
        export.SearchCriteria.SearchImHousehold.AeCaseNo =
          local.TextWorkArea.Text10;
      }
    }

    // check for  NEXTRAN
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      local.NextTranInfo.CaseNumber = export.SearchCriteria.SearchCase.Number;
      local.NextTranInfo.CsePersonNumber =
        export.SearchCriteria.SearchCsePersonsWorkSet.Number;
      local.NextTranInfo.StandardCrtOrdNumber =
        export.SearchCriteria.SearchLegalAction.StandardNumber ?? "";
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.SearchCriteria.SearchCase.Number =
        local.NextTranInfo.CaseNumber ?? Spaces(10);
      export.SearchCriteria.SearchCsePersonsWorkSet.Number =
        local.NextTranInfo.CsePersonNumber ?? Spaces(10);
      ExitState = "ACO_NN0000_ALL_OK";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "PART") || Equal(global.Command, "URAH") || Equal
      (global.Command, "URAC") || Equal(global.Command, "CURA") || Equal
      (global.Command, "URAA") || Equal(global.Command, "URAL") || Equal
      (global.Command, "UCOL") || Equal(global.Command, "UHMM"))
    {
      export.DialogFlow.DialogFlowCase.Number =
        export.SearchCriteria.SearchCase.Number;
      MoveCsePersonsWorkSet(export.SearchCriteria.SearchCsePersonsWorkSet,
        export.DialogFlow.DialogFlowCsePersonsWorkSet);
      export.DialogFlow.DialogFlowCsePerson.Number =
        export.SearchCriteria.SearchCsePersonsWorkSet.Number;
      export.DialogFlow.DialogFlowLegalAction.StandardNumber =
        export.SearchCriteria.SearchLegalAction.StandardNumber ?? "";
      export.DialogFlow.DialogFlowImHousehold.AeCaseNo =
        export.SearchCriteria.SearchImHousehold.AeCaseNo;
      local.NoOfEntriesSelected.Count = 0;

      for(export.Grp.Index = 0; export.Grp.Index < export.Grp.Count; ++
        export.Grp.Index)
      {
        switch(AsChar(export.Grp.Item.DtlCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.NoOfEntriesSelected.Count;

            if (local.NoOfEntriesSelected.Count > 1)
            {
              var field1 = GetField(export.Grp.Item.DtlCommon, "selectChar");

              field1.Error = true;

              ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTION";

              return;
            }

            export.DialogFlow.DialogFlowCsePersonsWorkSet.Number =
              export.Grp.Item.DtlCsePerson.Number;
            export.DialogFlow.DialogFlowCsePersonsWorkSet.FormattedName =
              export.Grp.Item.DtlCsePersonsWorkSet.FormattedName;
            export.DialogFlow.DialogFlowCsePerson.Number =
              export.Grp.Item.DtlCsePerson.Number;
            export.DialogFlow.DialogFlowLegalAction.StandardNumber =
              export.Grp.Item.DtlLegalAction.StandardNumber ?? "";
            export.DialogFlow.DialogFlowImHousehold.AeCaseNo =
              export.Grp.Item.DtlImHousehold.AeCaseNo;
            export.DialogFlow.DialogFlowFromDateWorkAttributes.TextMonthYear =
              export.Grp.Item.DtlFrom.TextMonthYear;
            export.DialogFlow.DialogFlowFromImHouseholdMbrMnthlySum.Month =
              (int)StringToNumber(Substring(
                export.Grp.Item.DtlFrom.TextMonthYear,
              DateWorkAttributes.TextMonthYear_MaxLength, 1, 2));
            export.DialogFlow.DialogFlowFromImHouseholdMbrMnthlySum.Year =
              (int)StringToNumber(Substring(
                export.Grp.Item.DtlFrom.TextMonthYear,
              DateWorkAttributes.TextMonthYear_MaxLength, 3, 4));
            export.DialogFlow.DialogFlowFromDateWorkArea.Month =
              export.DialogFlow.DialogFlowFromImHouseholdMbrMnthlySum.Month;
            export.DialogFlow.DialogFlowFromDateWorkArea.Year =
              export.DialogFlow.DialogFlowFromImHouseholdMbrMnthlySum.Year;
            export.DialogFlow.DialogFlowToDateWorkAttributes.TextMonthYear =
              export.Grp.Item.DtlTo.TextMonthYear;
            export.DialogFlow.DialogFlowToImHouseholdMbrMnthlySum.Month =
              (int)StringToNumber(Substring(
                export.Grp.Item.DtlTo.TextMonthYear,
              DateWorkAttributes.TextMonthYear_MaxLength, 1, 2));
            export.DialogFlow.DialogFlowToImHouseholdMbrMnthlySum.Year =
              (int)StringToNumber(Substring(
                export.Grp.Item.DtlTo.TextMonthYear,
              DateWorkAttributes.TextMonthYear_MaxLength, 3, 4));
            export.DialogFlow.DialogFlowToDateWorkArea.Month =
              export.DialogFlow.DialogFlowToImHouseholdMbrMnthlySum.Month;
            export.DialogFlow.DialogFlowToDateWorkArea.Year =
              export.DialogFlow.DialogFlowToImHouseholdMbrMnthlySum.Year;
            export.DialogFlow.DialogFlowToDateWorkArea.Year =
              export.DialogFlow.DialogFlowToImHouseholdMbrMnthlySum.Year;

            break;
          default:
            var field = GetField(export.Grp.Item.DtlCommon, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";

            return;
        }
      }

      if (Equal(global.Command, "URAA") && (
        IsEmpty(export.DialogFlow.DialogFlowImHousehold.AeCaseNo) || IsEmpty
        (export.DialogFlow.DialogFlowCsePersonsWorkSet.Number)))
      {
        ExitState = "OE0000_AT_LEAST_1_ENTRY_REQD";

        return;
      }

      switch(TrimEnd(global.Command))
      {
        case "PART":
          ExitState = "ECO_LNK_TO_PART";

          break;
        case "URAH":
          ExitState = "ECO_LNK_TO_URAH";

          break;
        case "URAC":
          ExitState = "ECO_LNK_TO_URAC";

          break;
        case "CURA":
          ExitState = "ECO_LNK_TO_CURA";

          break;
        case "URAA":
          ExitState = "ECO_LNK_TO_URAA";

          break;
        case "URAL":
          ExitState = "ECO_LNK_TO_URAL";

          break;
        case "UCOL":
          ExitState = "ECO_LNK_TO_UCOL";

          break;
        case "UHMM":
          ExitState = "ECO_LNK_TO_UHMM";

          break;
        default:
          break;
      }

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.SearchCriteria.SearchCase.Number) && IsEmpty
          (export.SearchCriteria.SearchImHousehold.AeCaseNo) && IsEmpty
          (export.SearchCriteria.SearchLegalAction.StandardNumber) && IsEmpty
          (export.SearchCriteria.SearchCsePersonsWorkSet.Number))
        {
          export.SearchCriteria.SearchCsePersonsWorkSet.FormattedName = "";
          ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

          return;
        }

        if (!IsEmpty(export.SearchCriteria.SearchImHousehold.AeCaseNo))
        {
          if (!ReadImHousehold1())
          {
            var field =
              GetField(export.SearchCriteria.SearchImHousehold, "aeCaseNo");

            field.Error = true;

            ExitState = "OE0000_AE_CASE_NUMBER_NF";

            return;
          }
        }

        if (!IsEmpty(export.SearchCriteria.SearchCsePersonsWorkSet.Number))
        {
          if (ReadCsePerson1())
          {
            if (AsChar(local.TraceMode.Flag) != 'Y')
            {
              UseSiReadCsePerson();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.SearchCriteria.SearchCsePersonsWorkSet.FormattedName =
                  local.ForFormattedName.FormattedName;
              }
              else
              {
                export.SearchCriteria.SearchCsePersonsWorkSet.FormattedName =
                  "Not able to get name from ADABAS.";
                ExitState = "ACO_NN0000_ALL_OK";
              }
            }
          }
          else
          {
            var field =
              GetField(export.SearchCriteria.SearchCsePersonsWorkSet, "number");
              

            field.Error = true;

            export.SearchCriteria.SearchCsePersonsWorkSet.FormattedName = "";
            ExitState = "CSE_PERSON_NF";

            return;
          }
        }

        if (!IsEmpty(export.SearchCriteria.SearchCase.Number))
        {
          if (ReadCase())
          {
            if (!IsEmpty(export.SearchCriteria.SearchCsePersonsWorkSet.Number))
            {
              if (!ReadCaseRole())
              {
                ExitState = "OE0000_PERSON_NOT_ON_CASE_2";

                return;
              }
            }
            else
            {
              local.PersOnCase.Index = 0;
              local.PersOnCase.Clear();

              foreach(var item in ReadCsePerson2())
              {
                local.PersOnCase.Update.PersOnCaseDtl.Number =
                  entities.CsePerson.Number;
                local.PersOnCase.Next();
              }

              if (local.PersOnCase.IsEmpty)
              {
                ExitState = "OE0000_NO_PERSONS_FOUND_ON_CASE";

                return;
              }
            }
          }
          else
          {
            var field = GetField(export.SearchCriteria.SearchCase, "number");

            field.Error = true;

            ExitState = "CASE_NF";

            return;
          }
        }

        if (!IsEmpty(export.SearchCriteria.SearchLegalAction.StandardNumber))
        {
          if (ReadLegalAction())
          {
            if (!IsEmpty(export.SearchCriteria.SearchCsePersonsWorkSet.Number))
            {
              if (!ReadLegalActionPerson())
              {
                ExitState = "OE0000_PERSON_NOT_ON_CT_ORDER";

                return;
              }
            }
            else
            {
              local.PersOnCtOrder.Index = 0;
              local.PersOnCtOrder.Clear();

              foreach(var item in ReadCsePerson3())
              {
                local.PersOnCtOrder.Update.PersOnCtOrderDtl.Number =
                  entities.CsePerson.Number;
                local.PersOnCtOrder.Next();
              }

              if (local.PersOnCtOrder.IsEmpty)
              {
                ExitState = "OE0000_NO_PERSON_FOUND_ON_CT_ORD";

                return;
              }
            }
          }
          else
          {
            var field =
              GetField(export.SearchCriteria.SearchLegalAction, "standardNumber");
              

            field.Error = true;

            ExitState = "LEGAL_ACTION_NF";

            return;
          }
        }

        if (!IsEmpty(export.SearchCriteria.SearchImHousehold.AeCaseNo))
        {
          if (!IsEmpty(export.SearchCriteria.SearchCsePersonsWorkSet.Number))
          {
            UseOeImhhByPerHh();
          }
          else
          {
            // Pass the 2 group views for filtering.
            UseOeImhhByHh();
          }
        }
        else if (!IsEmpty(export.SearchCriteria.SearchCsePersonsWorkSet.Number))
        {
          UseOeImhhByPerson();
        }
        else if (!IsEmpty(export.SearchCriteria.SearchCase.Number))
        {
          if (!IsEmpty(export.SearchCriteria.SearchLegalAction.StandardNumber))
          {
            // Pass the people on the case (1st grp) and the  people on the 
            // court order (2nd grp) so that the 2nd grp can be used to filter
            // the first.
            UseOeImhhByPersonGroup2();
          }
          else
          {
            // Pass the people on the case only.
            UseOeImhhByPersonGroup3();
          }
        }
        else
        {
          // Pass the people on the court order only.
          UseOeImhhByPersonGroup1();
        }

        if (!local.Grp.IsEmpty)
        {
          UseOeImhhSortDtl();
          local.FirstTimeThru.Flag = "Y";

          export.Grp.Index = 0;
          export.Grp.Clear();

          for(local.Grp.Index = 0; local.Grp.Index < local.Grp.Count; ++
            local.Grp.Index)
          {
            if (export.Grp.IsFull)
            {
              break;
            }

            export.Grp.Update.DtlCommon.SelectChar = "";
            export.Grp.Update.DtlImHousehold.AeCaseNo =
              local.Grp.Item.DtlImHousehold.AeCaseNo;
            export.Grp.Update.DtlImHouseholdMbrMnthlySum.Relationship =
              local.Grp.Item.DtlImHouseholdMbrMnthlySum.Relationship;
            export.Grp.Update.DtlCsePerson.Number =
              local.Grp.Item.DtlCsePerson.Number;
            export.Grp.Update.DtlCsePersonsWorkSet.FormattedName =
              local.Grp.Item.DtlCsePersonsWorkSet.FormattedName;
            local.Work.Text15 =
              NumberToString(local.Grp.Item.DtlFrom.YearMonth, 15);
            export.Grp.Update.DtlFrom.TextMonthYear =
              Substring(local.Work.Text15, WorkArea.Text15_MaxLength, 14, 2) + Substring
              (local.Work.Text15, WorkArea.Text15_MaxLength, 10, 4);
            local.Work.Text15 =
              NumberToString(local.Grp.Item.DtlTo.YearMonth, 15);
            export.Grp.Update.DtlTo.TextMonthYear =
              Substring(local.Work.Text15, WorkArea.Text15_MaxLength, 14, 2) + Substring
              (local.Work.Text15, WorkArea.Text15_MaxLength, 10, 4);
            export.Grp.Update.DtlLegalAction.StandardNumber =
              local.Grp.Item.DtlLegalAction.StandardNumber;
            export.Grp.Update.DtlMultCtOrdMsg.Text30 =
              local.Grp.Item.DtlMultCtOrdMsg.Text30;

            if (AsChar(local.FirstTimeThru.Flag) == 'Y')
            {
              local.FirstTimeThru.Flag = "N";

              if (ReadImHousehold2())
              {
                export.SearchCriteria.SearchImHousehold.FirstBenefitDate =
                  entities.ImHousehold.FirstBenefitDate;
              }
              else
              {
                ExitState = "OE0000_AE_CASE_NUMBER_NF";
                export.Grp.Next();

                return;
              }

              if (CharAt(local.Grp.Item.DtlImHousehold.AeCaseNo, 1) == 'F')
              {
                // **** Skip the "F" cases because they do not exist on AE.  ***
                // *
              }
              else
              {
                local.ImHousehold.AeCaseNo =
                  local.Grp.Item.DtlImHousehold.AeCaseNo;
                UseOeEabReadCaseBasicAda();

                switch(TrimEnd(local.ExecResults.Text5))
                {
                  case "NOTFD":
                    ExitState = "OE0000_AE_CASE_NBR_NOT_KNOWN";

                    break;
                  case "COMPL":
                    // Completed OK
                    export.SearchCriteria.SearchImHousehold.CaseStatus =
                      local.FromAdabase.CaseStatus;
                    export.SearchCriteria.SearchImHousehold.StatusDate =
                      local.FromAdabase.StatusDate;

                    break;
                  default:
                    export.SearchCriteria.SearchImHousehold.CaseStatus = "?";
                    ExitState = "OE0000_URA_ADABAS_ERROR_HH";

                    break;
                }
              }
            }

            export.Grp.Next();
          }
        }

        export.Hidden.HiddenCase.Number =
          export.SearchCriteria.SearchCase.Number;
        export.Hidden.HiddenCsePersonsWorkSet.Number =
          export.SearchCriteria.SearchCsePersonsWorkSet.Number;
        export.Hidden.HiddenLegalAction.StandardNumber =
          export.SearchCriteria.SearchLegalAction.StandardNumber;
        export.Hidden.HiddenImHousehold.AeCaseNo =
          export.SearchCriteria.SearchImHousehold.AeCaseNo;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Grp.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else if (export.Grp.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "LIST":
        // --- Prompt for CSE Case, CSE Person number or Court Order number
        if (!IsEmpty(export.Prompt.CseCase.PromptField))
        {
          if (AsChar(export.Prompt.CseCase.PromptField) == 'S')
          {
            if (!IsEmpty(export.Prompt.CsePerson.PromptField))
            {
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              var field1 = GetField(export.Prompt.CsePerson, "promptField");

              field1.Error = true;

              var field2 = GetField(export.Prompt.CseCase, "promptField");

              field2.Error = true;

              if (!IsEmpty(export.Prompt.CourtOrder.PromptField))
              {
                var field = GetField(export.Prompt.CourtOrder, "promptField");

                field.Error = true;
              }

              return;
            }
            else if (!IsEmpty(export.Prompt.CourtOrder.PromptField))
            {
              var field1 = GetField(export.Prompt.CseCase, "promptField");

              field1.Error = true;

              var field2 = GetField(export.Prompt.CourtOrder, "promptField");

              field2.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
            }

            export.Prompt.CseCase.PromptField = "";
            ExitState = "ECO_LNK_TO_COMN";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field = GetField(export.Prompt.CseCase, "promptField");

            field.Error = true;
          }
        }
        else if (!IsEmpty(export.Prompt.CsePerson.PromptField))
        {
          if (AsChar(export.Prompt.CsePerson.PromptField) == 'S')
          {
            if (!IsEmpty(export.Prompt.CourtOrder.PromptField))
            {
              var field1 = GetField(export.Prompt.CsePerson, "promptField");

              field1.Error = true;

              var field2 = GetField(export.Prompt.CourtOrder, "promptField");

              field2.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
            }

            export.Prompt.CsePerson.PromptField = "";
            ExitState = "ECO_LNK_TO_NAME";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field = GetField(export.Prompt.CsePerson, "promptField");

            field.Error = true;
          }
        }
        else if (!IsEmpty(export.Prompt.CourtOrder.PromptField))
        {
          if (AsChar(export.Prompt.CourtOrder.PromptField) == 'S')
          {
            export.Prompt.CourtOrder.PromptField = "";
            ExitState = "ECO_LNK_TO_LACS";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field = GetField(export.Prompt.CourtOrder, "promptField");

            field.Error = true;
          }
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGrp(OeImhhByPerHh.Export.GrpGroup source,
    Local.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.DtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.DtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber = source.DtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveGrpToTot(Local.GrpGroup source,
    OeImhhSortDtl.Import.TotGroup target)
  {
    target.TotGrpDtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.TotGrpDtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.TotGrpDtlCsePerson.Number = source.DtlCsePerson.Number;
    target.TotGrpDtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.TotGrpDtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.TotGrpDtlTo.YearMonth = source.DtlTo.YearMonth;
    target.TotGrpDtlLegalAction.StandardNumber =
      source.DtlLegalAction.StandardNumber;
    target.TotGrpDtlMultCtOrdMsg.Text30 = source.DtlMultCtOrdMsg.Text30;
  }

  private static void MoveHhPersonToGrp1(OeImhhByHh.Export.HhPersonGroup source,
    Local.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.DtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.DtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber = source.DtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveHhPersonToGrp2(OeImhhByPerson.Export.
    HhPersonGroup source, Local.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.DtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.DtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber = source.DtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveHhPersonToGrp3(OeImhhByPersonGroup.Export.
    HhPersonGroup source, Local.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.DtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.DtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.DtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.DtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.DtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.DtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber = source.DtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.DtlMultCtOrderMsg.Text30;
  }

  private static void MoveImHousehold(ImHousehold source, ImHousehold target)
  {
    target.CaseStatus = source.CaseStatus;
    target.StatusDate = source.StatusDate;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
  }

  private static void MovePersOnCaseToPersonsGrp1(Local.PersOnCaseGroup source,
    OeImhhByPersonGroup.Import.PersonsGrp1Group target)
  {
    target.PersonsDtl1.Number = source.PersOnCaseDtl.Number;
  }

  private static void MovePersOnCaseToPersonsOnCase(Local.
    PersOnCaseGroup source, OeImhhByHh.Import.PersonsOnCaseGroup target)
  {
    target.PersonsOnCaseDtl.Number = source.PersOnCaseDtl.Number;
  }

  private static void MovePersOnCtOrderToPersonsGrp1(Local.
    PersOnCtOrderGroup source,
    OeImhhByPersonGroup.Import.PersonsGrp1Group target)
  {
    target.PersonsDtl1.Number = source.PersOnCtOrderDtl.Number;
  }

  private static void MovePersOnCtOrderToPersonsGrp2(Local.
    PersOnCtOrderGroup source,
    OeImhhByPersonGroup.Import.PersonsGrp2Group target)
  {
    target.PersonsDtl2.Number = source.PersOnCtOrderDtl.Number;
  }

  private static void MovePersOnCtOrderToPersonsOnCtOrder(Local.
    PersOnCtOrderGroup source, OeImhhByHh.Import.PersonsOnCtOrderGroup target)
  {
    target.PersonsOnCtOrderDtl.Number = source.PersOnCtOrderDtl.Number;
  }

  private static void MoveTotToGrp(OeImhhSortDtl.Import.TotGroup source,
    Local.GrpGroup target)
  {
    target.DtlImHousehold.AeCaseNo = source.TotGrpDtlImHousehold.AeCaseNo;
    target.DtlImHouseholdMbrMnthlySum.Relationship =
      source.TotGrpDtlImHouseholdMbrMnthlySum.Relationship;
    target.DtlCsePerson.Number = source.TotGrpDtlCsePerson.Number;
    target.DtlCsePersonsWorkSet.FormattedName =
      source.TotGrpDtlCsePersonsWorkSet.FormattedName;
    target.DtlFrom.YearMonth = source.TotGrpDtlFrom.YearMonth;
    target.DtlTo.YearMonth = source.TotGrpDtlTo.YearMonth;
    target.DtlLegalAction.StandardNumber =
      source.TotGrpDtlLegalAction.StandardNumber;
    target.DtlMultCtOrdMsg.Text30 = source.TotGrpDtlMultCtOrdMsg.Text30;
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text80 = source.Text80;
    target.Text5 = source.Text5;
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

  private void UseOeEabReadCaseBasicAda()
  {
    var useImport = new OeEabReadCaseBasicAda.Import();
    var useExport = new OeEabReadCaseBasicAda.Export();

    useImport.ImHousehold.AeCaseNo = local.ImHousehold.AeCaseNo;
    MoveImHousehold(local.FromAdabase, useExport.ImHousehold);
    MoveWorkArea(local.ExecResults, useExport.ExecResults);

    Call(OeEabReadCaseBasicAda.Execute, useImport, useExport);

    MoveImHousehold(useExport.ImHousehold, local.FromAdabase);
    MoveWorkArea(useExport.ExecResults, local.ExecResults);
  }

  private void UseOeImhhByHh()
  {
    var useImport = new OeImhhByHh.Import();
    var useExport = new OeImhhByHh.Export();

    useImport.TraceMode.Flag = local.TraceMode.Flag;
    local.PersOnCtOrder.CopyTo(
      useImport.PersonsOnCtOrder, MovePersOnCtOrderToPersonsOnCtOrder);
    local.PersOnCase.CopyTo(
      useImport.PersonsOnCase, MovePersOnCaseToPersonsOnCase);
    useImport.ImHousehold.AeCaseNo =
      export.SearchCriteria.SearchImHousehold.AeCaseNo;

    Call(OeImhhByHh.Execute, useImport, useExport);

    useExport.HhPerson.CopyTo(local.Grp, MoveHhPersonToGrp1);
  }

  private void UseOeImhhByPerHh()
  {
    var useImport = new OeImhhByPerHh.Import();
    var useExport = new OeImhhByPerHh.Export();

    useImport.TraceMode.Flag = local.TraceMode.Flag;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ImHousehold.AeCaseNo =
      export.SearchCriteria.SearchImHousehold.AeCaseNo;

    Call(OeImhhByPerHh.Execute, useImport, useExport);

    useExport.Grp.CopyTo(local.Grp, MoveGrp);
  }

  private void UseOeImhhByPerson()
  {
    var useImport = new OeImhhByPerson.Import();
    var useExport = new OeImhhByPerson.Export();

    useImport.TraceMode.Flag = local.TraceMode.Flag;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(OeImhhByPerson.Execute, useImport, useExport);

    useExport.HhPerson.CopyTo(local.Grp, MoveHhPersonToGrp2);
  }

  private void UseOeImhhByPersonGroup1()
  {
    var useImport = new OeImhhByPersonGroup.Import();
    var useExport = new OeImhhByPersonGroup.Export();

    useImport.TraceMode.Flag = local.TraceMode.Flag;
    local.PersOnCtOrder.CopyTo(
      useImport.PersonsGrp1, MovePersOnCtOrderToPersonsGrp1);

    Call(OeImhhByPersonGroup.Execute, useImport, useExport);

    useExport.HhPerson.CopyTo(local.Grp, MoveHhPersonToGrp3);
  }

  private void UseOeImhhByPersonGroup2()
  {
    var useImport = new OeImhhByPersonGroup.Import();
    var useExport = new OeImhhByPersonGroup.Export();

    useImport.TraceMode.Flag = local.TraceMode.Flag;
    local.PersOnCtOrder.CopyTo(
      useImport.PersonsGrp2, MovePersOnCtOrderToPersonsGrp2);
    local.PersOnCase.CopyTo(useImport.PersonsGrp1, MovePersOnCaseToPersonsGrp1);

    Call(OeImhhByPersonGroup.Execute, useImport, useExport);

    useExport.HhPerson.CopyTo(local.Grp, MoveHhPersonToGrp3);
  }

  private void UseOeImhhByPersonGroup3()
  {
    var useImport = new OeImhhByPersonGroup.Import();
    var useExport = new OeImhhByPersonGroup.Export();

    useImport.TraceMode.Flag = local.TraceMode.Flag;
    local.PersOnCase.CopyTo(useImport.PersonsGrp1, MovePersOnCaseToPersonsGrp1);

    Call(OeImhhByPersonGroup.Execute, useImport, useExport);

    useExport.HhPerson.CopyTo(local.Grp, MoveHhPersonToGrp3);
  }

  private void UseOeImhhSortDtl()
  {
    var useImport = new OeImhhSortDtl.Import();
    var useExport = new OeImhhSortDtl.Export();

    local.Grp.CopyTo(useImport.Tot, MoveGrpToTot);

    Call(OeImhhSortDtl.Execute, useImport, useExport);

    useImport.Tot.CopyTo(local.Grp, MoveTotToGrp);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    MoveNextTranInfo(useExport.NextTranInfo, local.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

    useImport.CsePersonsWorkSet.Number =
      import.SearchCriteria.SearchCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.SearchCriteria.SearchCase.Number;
    useImport.LegalAction.StandardNumber =
      export.SearchCriteria.SearchLegalAction.StandardNumber;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.SearchCriteria.SearchCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ForFormattedName.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SearchCriteria.SearchCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          export.SearchCriteria.SearchCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        if (local.PersOnCase.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          export.SearchCriteria.SearchLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (local.PersOnCtOrder.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadImHousehold1()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold1",
      (db, command) =>
      {
        db.SetString(
          command, "aeCaseNo",
          export.SearchCriteria.SearchImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.CaseStatus = db.GetString(reader, 1);
        entities.ImHousehold.StatusDate = db.GetDate(reader, 2);
        entities.ImHousehold.FirstBenefitDate = db.GetNullableDate(reader, 3);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadImHousehold2()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold2",
      (db, command) =>
      {
        db.
          SetString(command, "aeCaseNo", local.Grp.Item.DtlImHousehold.AeCaseNo);
          
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.CaseStatus = db.GetString(reader, 1);
        entities.ImHousehold.StatusDate = db.GetDate(reader, 2);
        entities.ImHousehold.FirstBenefitDate = db.GetNullableDate(reader, 3);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          export.SearchCriteria.SearchLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.Populated = true;
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
    /// <summary>A SearchCriteriaGroup group.</summary>
    [Serializable]
    public class SearchCriteriaGroup
    {
      /// <summary>
      /// A value of SearchCase.
      /// </summary>
      [JsonPropertyName("searchCase")]
      public Case1 SearchCase
      {
        get => searchCase ??= new();
        set => searchCase = value;
      }

      /// <summary>
      /// A value of SearchCsePerson.
      /// </summary>
      [JsonPropertyName("searchCsePerson")]
      public CsePerson SearchCsePerson
      {
        get => searchCsePerson ??= new();
        set => searchCsePerson = value;
      }

      /// <summary>
      /// A value of SearchCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("searchCsePersonsWorkSet")]
      public CsePersonsWorkSet SearchCsePersonsWorkSet
      {
        get => searchCsePersonsWorkSet ??= new();
        set => searchCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of SearchLegalAction.
      /// </summary>
      [JsonPropertyName("searchLegalAction")]
      public LegalAction SearchLegalAction
      {
        get => searchLegalAction ??= new();
        set => searchLegalAction = value;
      }

      /// <summary>
      /// A value of SearchImHousehold.
      /// </summary>
      [JsonPropertyName("searchImHousehold")]
      public ImHousehold SearchImHousehold
      {
        get => searchImHousehold ??= new();
        set => searchImHousehold = value;
      }

      private Case1 searchCase;
      private CsePerson searchCsePerson;
      private CsePersonsWorkSet searchCsePersonsWorkSet;
      private LegalAction searchLegalAction;
      private ImHousehold searchImHousehold;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenCase.
      /// </summary>
      [JsonPropertyName("hiddenCase")]
      public Case1 HiddenCase
      {
        get => hiddenCase ??= new();
        set => hiddenCase = value;
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
      /// A value of HiddenLegalAction.
      /// </summary>
      [JsonPropertyName("hiddenLegalAction")]
      public LegalAction HiddenLegalAction
      {
        get => hiddenLegalAction ??= new();
        set => hiddenLegalAction = value;
      }

      /// <summary>
      /// A value of HiddenImHousehold.
      /// </summary>
      [JsonPropertyName("hiddenImHousehold")]
      public ImHousehold HiddenImHousehold
      {
        get => hiddenImHousehold ??= new();
        set => hiddenImHousehold = value;
      }

      private Case1 hiddenCase;
      private CsePersonsWorkSet hiddenCsePersonsWorkSet;
      private LegalAction hiddenLegalAction;
      private ImHousehold hiddenImHousehold;
    }

    /// <summary>A PromptGroup group.</summary>
    [Serializable]
    public class PromptGroup
    {
      /// <summary>
      /// A value of CseCaseNumber.
      /// </summary>
      [JsonPropertyName("cseCaseNumber")]
      public Standard CseCaseNumber
      {
        get => cseCaseNumber ??= new();
        set => cseCaseNumber = value;
      }

      /// <summary>
      /// A value of CsePerson.
      /// </summary>
      [JsonPropertyName("csePerson")]
      public Standard CsePerson
      {
        get => csePerson ??= new();
        set => csePerson = value;
      }

      /// <summary>
      /// A value of CourtOrder.
      /// </summary>
      [JsonPropertyName("courtOrder")]
      public Standard CourtOrder
      {
        get => courtOrder ??= new();
        set => courtOrder = value;
      }

      private Standard cseCaseNumber;
      private Standard csePerson;
      private Standard courtOrder;
    }

    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlImHousehold.
      /// </summary>
      [JsonPropertyName("dtlImHousehold")]
      public ImHousehold DtlImHousehold
      {
        get => dtlImHousehold ??= new();
        set => dtlImHousehold = value;
      }

      /// <summary>
      /// A value of DtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlImHouseholdMbrMnthlySum
      {
        get => dtlImHouseholdMbrMnthlySum ??= new();
        set => dtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCsePerson")]
      public CsePerson DtlCsePerson
      {
        get => dtlCsePerson ??= new();
        set => dtlCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlCsePersonsWorkSet
      {
        get => dtlCsePersonsWorkSet ??= new();
        set => dtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlFrom.
      /// </summary>
      [JsonPropertyName("dtlFrom")]
      public DateWorkAttributes DtlFrom
      {
        get => dtlFrom ??= new();
        set => dtlFrom = value;
      }

      /// <summary>
      /// A value of DtlTo.
      /// </summary>
      [JsonPropertyName("dtlTo")]
      public DateWorkAttributes DtlTo
      {
        get => dtlTo ??= new();
        set => dtlTo = value;
      }

      /// <summary>
      /// A value of DtlLegalAction.
      /// </summary>
      [JsonPropertyName("dtlLegalAction")]
      public LegalAction DtlLegalAction
      {
        get => dtlLegalAction ??= new();
        set => dtlLegalAction = value;
      }

      /// <summary>
      /// A value of DtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("dtlMultCtOrdMsg")]
      public TextWorkArea DtlMultCtOrdMsg
      {
        get => dtlMultCtOrdMsg ??= new();
        set => dtlMultCtOrdMsg = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private Common dtlCommon;
      private ImHousehold dtlImHousehold;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private CsePerson dtlCsePerson;
      private CsePersonsWorkSet dtlCsePersonsWorkSet;
      private DateWorkAttributes dtlFrom;
      private DateWorkAttributes dtlTo;
      private LegalAction dtlLegalAction;
      private TextWorkArea dtlMultCtOrdMsg;
    }

    /// <summary>
    /// A value of FromLacsCase.
    /// </summary>
    [JsonPropertyName("fromLacsCase")]
    public Case1 FromLacsCase
    {
      get => fromLacsCase ??= new();
      set => fromLacsCase = value;
    }

    /// <summary>
    /// A value of FromLacsLegalAction.
    /// </summary>
    [JsonPropertyName("fromLacsLegalAction")]
    public LegalAction FromLacsLegalAction
    {
      get => fromLacsLegalAction ??= new();
      set => fromLacsLegalAction = value;
    }

    /// <summary>
    /// Gets a value of SearchCriteria.
    /// </summary>
    [JsonPropertyName("searchCriteria")]
    public SearchCriteriaGroup SearchCriteria
    {
      get => searchCriteria ?? (searchCriteria = new());
      set => searchCriteria = value;
    }

    /// <summary>
    /// A value of ZdelMeXzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zdelMeXzzzzzzzzzzzzzzzzzzzzzzz")]
    public ImHousehold ZdelMeXzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zdelMeXzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zdelMeXzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public HiddenGroup Hidden
    {
      get => hidden ?? (hidden = new());
      set => hidden = value;
    }

    /// <summary>
    /// Gets a value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public PromptGroup Prompt
    {
      get => prompt ?? (prompt = new());
      set => prompt = value;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
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

    private Case1 fromLacsCase;
    private LegalAction fromLacsLegalAction;
    private SearchCriteriaGroup searchCriteria;
    private ImHousehold zdelMeXzzzzzzzzzzzzzzzzzzzzzzz;
    private HiddenGroup hidden;
    private PromptGroup prompt;
    private Array<GrpGroup> grp;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DialogFlowGroup group.</summary>
    [Serializable]
    public class DialogFlowGroup
    {
      /// <summary>
      /// A value of DialogFlowCase.
      /// </summary>
      [JsonPropertyName("dialogFlowCase")]
      public Case1 DialogFlowCase
      {
        get => dialogFlowCase ??= new();
        set => dialogFlowCase = value;
      }

      /// <summary>
      /// A value of DialogFlowCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dialogFlowCsePersonsWorkSet")]
      public CsePersonsWorkSet DialogFlowCsePersonsWorkSet
      {
        get => dialogFlowCsePersonsWorkSet ??= new();
        set => dialogFlowCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DialogFlowCsePerson.
      /// </summary>
      [JsonPropertyName("dialogFlowCsePerson")]
      public CsePerson DialogFlowCsePerson
      {
        get => dialogFlowCsePerson ??= new();
        set => dialogFlowCsePerson = value;
      }

      /// <summary>
      /// A value of DialogFlowLegalAction.
      /// </summary>
      [JsonPropertyName("dialogFlowLegalAction")]
      public LegalAction DialogFlowLegalAction
      {
        get => dialogFlowLegalAction ??= new();
        set => dialogFlowLegalAction = value;
      }

      /// <summary>
      /// A value of DialogFlowImHousehold.
      /// </summary>
      [JsonPropertyName("dialogFlowImHousehold")]
      public ImHousehold DialogFlowImHousehold
      {
        get => dialogFlowImHousehold ??= new();
        set => dialogFlowImHousehold = value;
      }

      /// <summary>
      /// A value of DialogFlowFromDateWorkAttributes.
      /// </summary>
      [JsonPropertyName("dialogFlowFromDateWorkAttributes")]
      public DateWorkAttributes DialogFlowFromDateWorkAttributes
      {
        get => dialogFlowFromDateWorkAttributes ??= new();
        set => dialogFlowFromDateWorkAttributes = value;
      }

      /// <summary>
      /// A value of DialogFlowFromImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dialogFlowFromImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DialogFlowFromImHouseholdMbrMnthlySum
      {
        get => dialogFlowFromImHouseholdMbrMnthlySum ??= new();
        set => dialogFlowFromImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DialogFlowFromDateWorkArea.
      /// </summary>
      [JsonPropertyName("dialogFlowFromDateWorkArea")]
      public DateWorkArea DialogFlowFromDateWorkArea
      {
        get => dialogFlowFromDateWorkArea ??= new();
        set => dialogFlowFromDateWorkArea = value;
      }

      /// <summary>
      /// A value of DialogFlowToDateWorkAttributes.
      /// </summary>
      [JsonPropertyName("dialogFlowToDateWorkAttributes")]
      public DateWorkAttributes DialogFlowToDateWorkAttributes
      {
        get => dialogFlowToDateWorkAttributes ??= new();
        set => dialogFlowToDateWorkAttributes = value;
      }

      /// <summary>
      /// A value of DialogFlowToImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dialogFlowToImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DialogFlowToImHouseholdMbrMnthlySum
      {
        get => dialogFlowToImHouseholdMbrMnthlySum ??= new();
        set => dialogFlowToImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DialogFlowToDateWorkArea.
      /// </summary>
      [JsonPropertyName("dialogFlowToDateWorkArea")]
      public DateWorkArea DialogFlowToDateWorkArea
      {
        get => dialogFlowToDateWorkArea ??= new();
        set => dialogFlowToDateWorkArea = value;
      }

      private Case1 dialogFlowCase;
      private CsePersonsWorkSet dialogFlowCsePersonsWorkSet;
      private CsePerson dialogFlowCsePerson;
      private LegalAction dialogFlowLegalAction;
      private ImHousehold dialogFlowImHousehold;
      private DateWorkAttributes dialogFlowFromDateWorkAttributes;
      private ImHouseholdMbrMnthlySum dialogFlowFromImHouseholdMbrMnthlySum;
      private DateWorkArea dialogFlowFromDateWorkArea;
      private DateWorkAttributes dialogFlowToDateWorkAttributes;
      private ImHouseholdMbrMnthlySum dialogFlowToImHouseholdMbrMnthlySum;
      private DateWorkArea dialogFlowToDateWorkArea;
    }

    /// <summary>A SearchCriteriaGroup group.</summary>
    [Serializable]
    public class SearchCriteriaGroup
    {
      /// <summary>
      /// A value of SearchCase.
      /// </summary>
      [JsonPropertyName("searchCase")]
      public Case1 SearchCase
      {
        get => searchCase ??= new();
        set => searchCase = value;
      }

      /// <summary>
      /// A value of SearchCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("searchCsePersonsWorkSet")]
      public CsePersonsWorkSet SearchCsePersonsWorkSet
      {
        get => searchCsePersonsWorkSet ??= new();
        set => searchCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of SearchLegalAction.
      /// </summary>
      [JsonPropertyName("searchLegalAction")]
      public LegalAction SearchLegalAction
      {
        get => searchLegalAction ??= new();
        set => searchLegalAction = value;
      }

      /// <summary>
      /// A value of SearchImHousehold.
      /// </summary>
      [JsonPropertyName("searchImHousehold")]
      public ImHousehold SearchImHousehold
      {
        get => searchImHousehold ??= new();
        set => searchImHousehold = value;
      }

      private Case1 searchCase;
      private CsePersonsWorkSet searchCsePersonsWorkSet;
      private LegalAction searchLegalAction;
      private ImHousehold searchImHousehold;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenCase.
      /// </summary>
      [JsonPropertyName("hiddenCase")]
      public Case1 HiddenCase
      {
        get => hiddenCase ??= new();
        set => hiddenCase = value;
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
      /// A value of HiddenImHousehold.
      /// </summary>
      [JsonPropertyName("hiddenImHousehold")]
      public ImHousehold HiddenImHousehold
      {
        get => hiddenImHousehold ??= new();
        set => hiddenImHousehold = value;
      }

      /// <summary>
      /// A value of HiddenLegalAction.
      /// </summary>
      [JsonPropertyName("hiddenLegalAction")]
      public LegalAction HiddenLegalAction
      {
        get => hiddenLegalAction ??= new();
        set => hiddenLegalAction = value;
      }

      private Case1 hiddenCase;
      private CsePersonsWorkSet hiddenCsePersonsWorkSet;
      private ImHousehold hiddenImHousehold;
      private LegalAction hiddenLegalAction;
    }

    /// <summary>A PromptGroup group.</summary>
    [Serializable]
    public class PromptGroup
    {
      /// <summary>
      /// A value of CseCase.
      /// </summary>
      [JsonPropertyName("cseCase")]
      public Standard CseCase
      {
        get => cseCase ??= new();
        set => cseCase = value;
      }

      /// <summary>
      /// A value of CsePerson.
      /// </summary>
      [JsonPropertyName("csePerson")]
      public Standard CsePerson
      {
        get => csePerson ??= new();
        set => csePerson = value;
      }

      /// <summary>
      /// A value of CourtOrder.
      /// </summary>
      [JsonPropertyName("courtOrder")]
      public Standard CourtOrder
      {
        get => courtOrder ??= new();
        set => courtOrder = value;
      }

      private Standard cseCase;
      private Standard csePerson;
      private Standard courtOrder;
    }

    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlImHousehold.
      /// </summary>
      [JsonPropertyName("dtlImHousehold")]
      public ImHousehold DtlImHousehold
      {
        get => dtlImHousehold ??= new();
        set => dtlImHousehold = value;
      }

      /// <summary>
      /// A value of DtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlImHouseholdMbrMnthlySum
      {
        get => dtlImHouseholdMbrMnthlySum ??= new();
        set => dtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCsePerson")]
      public CsePerson DtlCsePerson
      {
        get => dtlCsePerson ??= new();
        set => dtlCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlCsePersonsWorkSet
      {
        get => dtlCsePersonsWorkSet ??= new();
        set => dtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlFrom.
      /// </summary>
      [JsonPropertyName("dtlFrom")]
      public DateWorkAttributes DtlFrom
      {
        get => dtlFrom ??= new();
        set => dtlFrom = value;
      }

      /// <summary>
      /// A value of DtlTo.
      /// </summary>
      [JsonPropertyName("dtlTo")]
      public DateWorkAttributes DtlTo
      {
        get => dtlTo ??= new();
        set => dtlTo = value;
      }

      /// <summary>
      /// A value of DtlLegalAction.
      /// </summary>
      [JsonPropertyName("dtlLegalAction")]
      public LegalAction DtlLegalAction
      {
        get => dtlLegalAction ??= new();
        set => dtlLegalAction = value;
      }

      /// <summary>
      /// A value of DtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("dtlMultCtOrdMsg")]
      public TextWorkArea DtlMultCtOrdMsg
      {
        get => dtlMultCtOrdMsg ??= new();
        set => dtlMultCtOrdMsg = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private Common dtlCommon;
      private ImHousehold dtlImHousehold;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private CsePerson dtlCsePerson;
      private CsePersonsWorkSet dtlCsePersonsWorkSet;
      private DateWorkAttributes dtlFrom;
      private DateWorkAttributes dtlTo;
      private LegalAction dtlLegalAction;
      private TextWorkArea dtlMultCtOrdMsg;
    }

    /// <summary>
    /// Gets a value of DialogFlow.
    /// </summary>
    [JsonPropertyName("dialogFlow")]
    public DialogFlowGroup DialogFlow
    {
      get => dialogFlow ?? (dialogFlow = new());
      set => dialogFlow = value;
    }

    /// <summary>
    /// Gets a value of SearchCriteria.
    /// </summary>
    [JsonPropertyName("searchCriteria")]
    public SearchCriteriaGroup SearchCriteria
    {
      get => searchCriteria ?? (searchCriteria = new());
      set => searchCriteria = value;
    }

    /// <summary>
    /// A value of ZdelMeZzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zdelMeZzzzzzzzzzzzzzzzzzzzzzzz")]
    public ImHousehold ZdelMeZzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zdelMeZzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zdelMeZzzzzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public HiddenGroup Hidden
    {
      get => hidden ?? (hidden = new());
      set => hidden = value;
    }

    /// <summary>
    /// Gets a value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public PromptGroup Prompt
    {
      get => prompt ?? (prompt = new());
      set => prompt = value;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
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

    private DialogFlowGroup dialogFlow;
    private SearchCriteriaGroup searchCriteria;
    private ImHousehold zdelMeZzzzzzzzzzzzzzzzzzzzzzzz;
    private HiddenGroup hidden;
    private PromptGroup prompt;
    private Array<GrpGroup> grp;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlImHousehold.
      /// </summary>
      [JsonPropertyName("dtlImHousehold")]
      public ImHousehold DtlImHousehold
      {
        get => dtlImHousehold ??= new();
        set => dtlImHousehold = value;
      }

      /// <summary>
      /// A value of DtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("dtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum DtlImHouseholdMbrMnthlySum
      {
        get => dtlImHouseholdMbrMnthlySum ??= new();
        set => dtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>
      /// A value of DtlCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCsePerson")]
      public CsePerson DtlCsePerson
      {
        get => dtlCsePerson ??= new();
        set => dtlCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("dtlCsePersonsWorkSet")]
      public CsePersonsWorkSet DtlCsePersonsWorkSet
      {
        get => dtlCsePersonsWorkSet ??= new();
        set => dtlCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DtlFrom.
      /// </summary>
      [JsonPropertyName("dtlFrom")]
      public DateWorkArea DtlFrom
      {
        get => dtlFrom ??= new();
        set => dtlFrom = value;
      }

      /// <summary>
      /// A value of DtlTo.
      /// </summary>
      [JsonPropertyName("dtlTo")]
      public DateWorkArea DtlTo
      {
        get => dtlTo ??= new();
        set => dtlTo = value;
      }

      /// <summary>
      /// A value of DtlLegalAction.
      /// </summary>
      [JsonPropertyName("dtlLegalAction")]
      public LegalAction DtlLegalAction
      {
        get => dtlLegalAction ??= new();
        set => dtlLegalAction = value;
      }

      /// <summary>
      /// A value of DtlMultCtOrdMsg.
      /// </summary>
      [JsonPropertyName("dtlMultCtOrdMsg")]
      public TextWorkArea DtlMultCtOrdMsg
      {
        get => dtlMultCtOrdMsg ??= new();
        set => dtlMultCtOrdMsg = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private ImHousehold dtlImHousehold;
      private ImHouseholdMbrMnthlySum dtlImHouseholdMbrMnthlySum;
      private CsePerson dtlCsePerson;
      private CsePersonsWorkSet dtlCsePersonsWorkSet;
      private DateWorkArea dtlFrom;
      private DateWorkArea dtlTo;
      private LegalAction dtlLegalAction;
      private TextWorkArea dtlMultCtOrdMsg;
    }

    /// <summary>A PersOnCtOrderGroup group.</summary>
    [Serializable]
    public class PersOnCtOrderGroup
    {
      /// <summary>
      /// A value of PersOnCtOrderDtl.
      /// </summary>
      [JsonPropertyName("persOnCtOrderDtl")]
      public CsePerson PersOnCtOrderDtl
      {
        get => persOnCtOrderDtl ??= new();
        set => persOnCtOrderDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson persOnCtOrderDtl;
    }

    /// <summary>A PersOnCaseGroup group.</summary>
    [Serializable]
    public class PersOnCaseGroup
    {
      /// <summary>
      /// A value of PersOnCaseDtl.
      /// </summary>
      [JsonPropertyName("persOnCaseDtl")]
      public CsePerson PersOnCaseDtl
      {
        get => persOnCaseDtl ??= new();
        set => persOnCaseDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson persOnCaseDtl;
    }

    /// <summary>
    /// A value of TraceMode.
    /// </summary>
    [JsonPropertyName("traceMode")]
    public Common TraceMode
    {
      get => traceMode ??= new();
      set => traceMode = value;
    }

    /// <summary>
    /// A value of ForFormattedName.
    /// </summary>
    [JsonPropertyName("forFormattedName")]
    public CsePersonsWorkSet ForFormattedName
    {
      get => forFormattedName ??= new();
      set => forFormattedName = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of FromAdabase.
    /// </summary>
    [JsonPropertyName("fromAdabase")]
    public ImHousehold FromAdabase
    {
      get => fromAdabase ??= new();
      set => fromAdabase = value;
    }

    /// <summary>
    /// A value of ExecResults.
    /// </summary>
    [JsonPropertyName("execResults")]
    public WorkArea ExecResults
    {
      get => execResults ??= new();
      set => execResults = value;
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
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    /// <summary>
    /// Gets a value of PersOnCtOrder.
    /// </summary>
    [JsonIgnore]
    public Array<PersOnCtOrderGroup> PersOnCtOrder => persOnCtOrder ??= new(
      PersOnCtOrderGroup.Capacity);

    /// <summary>
    /// Gets a value of PersOnCtOrder for json serialization.
    /// </summary>
    [JsonPropertyName("persOnCtOrder")]
    [Computed]
    public IList<PersOnCtOrderGroup> PersOnCtOrder_Json
    {
      get => persOnCtOrder;
      set => PersOnCtOrder.Assign(value);
    }

    /// <summary>
    /// Gets a value of PersOnCase.
    /// </summary>
    [JsonIgnore]
    public Array<PersOnCaseGroup> PersOnCase => persOnCase ??= new(
      PersOnCaseGroup.Capacity);

    /// <summary>
    /// Gets a value of PersOnCase for json serialization.
    /// </summary>
    [JsonPropertyName("persOnCase")]
    [Computed]
    public IList<PersOnCaseGroup> PersOnCase_Json
    {
      get => persOnCase;
      set => PersOnCase.Assign(value);
    }

    /// <summary>
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public WorkArea Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of NoOfEntriesSelected.
    /// </summary>
    [JsonPropertyName("noOfEntriesSelected")]
    public Common NoOfEntriesSelected
    {
      get => noOfEntriesSelected ??= new();
      set => noOfEntriesSelected = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common traceMode;
    private CsePersonsWorkSet forFormattedName;
    private ImHousehold imHousehold;
    private ImHousehold fromAdabase;
    private WorkArea execResults;
    private CsePerson csePerson;
    private Array<GrpGroup> grp;
    private Array<PersOnCtOrderGroup> persOnCtOrder;
    private Array<PersOnCaseGroup> persOnCase;
    private Common firstTimeThru;
    private TextWorkArea textWorkArea;
    private WorkArea work;
    private Common noOfEntriesSelected;
    private NextTranInfo nextTranInfo;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private ImHousehold imHousehold;
  }
#endregion
}

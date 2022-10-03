// Program: OE_UHMM_URA_HOUSEHOLD_MBR_MAINT, ID: 374452694, model: 746.
// Short name: SWEUHMMP
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
/// A program: OE_UHMM_URA_HOUSEHOLD_MBR_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeUhmmUraHouseholdMbrMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UHMM_URA_HOUSEHOLD_MBR_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUhmmUraHouseholdMbrMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUhmmUraHouseholdMbrMaint.
  /// </summary>
  public OeUhmmUraHouseholdMbrMaint(IContext context, Import import,
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
    // *********************** MAINTENANCE LOG *************************
    // AUTHOR         DATE  	  CHG REQ#    DESCRIPTION
    // ------      ----------    ----------  ---------------------------
    // Ed Lyman    06/01/2000	              Initial Code
    // Ed Lyman    08/07/2000                Specification Changes
    // Ed Lyman    10/16/2000    WR# 226     Auto Create C Case Numbers
    // Ed Lyman    01/12/2001    PR# 110432  Remove First Benefit Date,
    //                                       
    // Status, and Status Date.
    // P. Phinney  06/11/2001    WR10342     Allow DEVELOPER profile to bypass 
    // Grant Date Check
    // *********************** END MAINTENANCE LOG *********************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenCommon.Subscript = import.HiddenCommon.Subscript;
    export.HiddenConfirmAdd.Flag = import.HiddenConfirmAdd.Flag;
    export.HiddenConfirmCreate.Flag = import.HiddenConfirmCreate.Flag;

    // P. Phinney  06/11/2001    WR10342     Allow DEVELOPER profile to bypass 
    // Grant Date Check
    export.HiddenDeveloper.Flag = import.HiddenDeveloper.Flag;

    if (!Equal(global.Command, "ADD"))
    {
      export.HiddenConfirmAdd.Flag = "";
      export.HiddenConfirmCreate.Flag = "";
    }

    if (Equal(global.Command, "CLEAR"))
    {
      export.Export1.Count = Export.ExportGroup.Capacity;

      var field = GetField(export.ImHousehold, "aeCaseNo");

      field.Protected = false;
      field.Focused = true;

      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // -----------------------------------------
    // Set Hardcoded Values
    // -----------------------------------------
    local.DateBoundary.Date = new DateTime(1989, 10, 1);
    local.DateMinimum.Date = new DateTime(1960, 1, 1);
    local.Code.CodeName = "AE CASE ROLE";

    // -----------------------------------------
    // Move Imports to Exports.
    // -----------------------------------------
    export.ImHousehold.Assign(import.ImHousehold);
    export.Previous.AeCaseNo = import.Previous.AeCaseNo;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.GcsePerson.Number =
        import.Import1.Item.GcsePerson.Number;
      export.Export1.Update.GcsePersonsWorkSet.FormattedName =
        import.Import1.Item.GcsePersonsWorkSet.FormattedName;
      export.Export1.Update.Gcommon.SelectChar =
        import.Import1.Item.Gcommon.SelectChar;
      MoveImHouseholdMbrMnthlySum(import.Import1.Item.GimHouseholdMbrMnthlySum,
        export.Export1.Update.GimHouseholdMbrMnthlySum);
      export.Export1.Update.GexportBegin.
        Assign(import.Import1.Item.GimportBegin);
      export.Export1.Update.GexportEnd.Assign(import.Import1.Item.GimportEnd);
      export.Export1.Update.GexportPart.Relationship =
        import.Import1.Item.GimportPart.Relationship;
      export.Export1.Update.GexportScreenPrompt.PromptField =
        import.Import1.Item.GimportScreenPrompt.PromptField;
    }

    import.Import1.CheckIndex();
    export.Export1.Count = Export.ExportGroup.Capacity;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ************************************************
    //  Next Tran START
    // ************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      // ***  take the first person selected, if a selection has been made ***
      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
        {
          export.HiddenNextTranInfo.CsePersonNumber =
            export.Export1.Item.GcsePerson.Number;

          break;
        }
      }

      export.Export1.CheckIndex();
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
      // The user is comming into this procedure on a next tran action.
      UseScCabNextTranGet();

      return;
    }

    // ************************************************
    //  Next Tran END
    // ************************************************
    if (Equal(global.Command, "XXFMMENU"))
    {
      // Flow from the menu
      return;
    }

    // ----------------------------------------------
    // SECURITY
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "CURA"))
    {
      // ***  validate that only one selection has been made
      local.OnlyOneSelected.Flag = "N";

      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
        {
          if (AsChar(local.OnlyOneSelected.Flag) == 'Y')
          {
            var field = GetField(export.Export1.Item.Gcommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
          }
          else
          {
            local.OnlyOneSelected.Flag = "Y";
            export.SearchCsePersonsWorkSet.Number =
              export.Export1.Item.GcsePerson.Number;
            export.SearchCsePerson.Number =
              export.Export1.Item.GcsePerson.Number;
            export.SearchFromDateWorkAttributes.TextMonthYear =
              NumberToString(export.Export1.Item.GexportBegin.Month, 14, 2) + NumberToString
              (export.Export1.Item.GexportBegin.Year, 12, 4);
            export.SearchToDateWorkAttributes.TextMonthYear =
              NumberToString(export.Export1.Item.GexportEnd.Month, 14, 2) + NumberToString
              (export.Export1.Item.GexportEnd.Year, 12, 4);
            export.SearchFromDateWorkArea.Month =
              export.Export1.Item.GexportBegin.Month;
            export.SearchFromDateWorkArea.Year =
              export.Export1.Item.GexportBegin.Year;
            export.SearchFromDateWorkArea.YearMonth =
              export.Export1.Item.GexportBegin.Year * 100 + export
              .Export1.Item.GexportBegin.Month;
            export.SearchToDateWorkArea.Month =
              export.Export1.Item.GexportEnd.Month;
            export.SearchToDateWorkArea.Year =
              export.Export1.Item.GexportEnd.Year;
            export.SearchToDateWorkArea.YearMonth =
              export.Export1.Item.GexportEnd.Year * 100 + export
              .Export1.Item.GexportEnd.Month;
            export.HiddenCommon.Subscript = export.Export1.Index + 1;
          }
        }
      }

      export.Export1.CheckIndex();

      if (AsChar(local.OnlyOneSelected.Flag) != 'Y')
      {
        ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";

        return;
      }
    }

    if (Equal(global.Command, "URAA"))
    {
      if (!IsEmpty(export.ImHousehold.AeCaseNo))
      {
        if (Equal(export.ImHousehold.AeCaseNo, export.Previous.AeCaseNo))
        {
          var field = GetField(export.ImHousehold, "aeCaseNo");

          field.Color = "cyan";
          field.Protected = true;
        }
      }
      else
      {
        var field = GetField(export.ImHousehold, "aeCaseNo");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      // ***  validate that only one selection has been made
      local.OnlyOneSelected.Flag = "N";

      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
        {
          if (AsChar(local.OnlyOneSelected.Flag) == 'Y')
          {
            var field = GetField(export.Export1.Item.Gcommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
          }
          else
          {
            local.OnlyOneSelected.Flag = "Y";

            if (!IsEmpty(export.Export1.Item.GcsePerson.Number))
            {
              local.TextWorkArea.Text10 = export.Export1.Item.GcsePerson.Number;
              UseEabPadLeftWithZeros();
              export.Export1.Update.GcsePerson.Number =
                local.TextWorkArea.Text10;
            }
            else
            {
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";

              var field = GetField(export.Export1.Item.GcsePerson, "number");

              field.Error = true;

              return;
            }

            export.SearchCsePersonsWorkSet.Number =
              export.Export1.Item.GcsePerson.Number;
            export.SearchCsePerson.Number =
              export.Export1.Item.GcsePerson.Number;
            export.SearchFromDateWorkAttributes.TextMonthYear =
              NumberToString(export.Export1.Item.GexportBegin.Month, 14, 2) + NumberToString
              (export.Export1.Item.GexportBegin.Year, 12, 4);
            export.SearchToDateWorkAttributes.TextMonthYear =
              NumberToString(export.Export1.Item.GexportEnd.Month, 14, 2) + NumberToString
              (export.Export1.Item.GexportEnd.Year, 12, 4);
            export.SearchFromDateWorkArea.Month =
              export.Export1.Item.GexportBegin.Month;
            export.SearchFromDateWorkArea.Year =
              export.Export1.Item.GexportBegin.Year;
            export.SearchFromDateWorkArea.YearMonth =
              export.Export1.Item.GexportBegin.Year * 100 + export
              .Export1.Item.GexportBegin.Month;
            export.SearchToDateWorkArea.Month =
              export.Export1.Item.GexportEnd.Month;
            export.SearchToDateWorkArea.Year =
              export.Export1.Item.GexportEnd.Year;
            export.SearchToDateWorkArea.YearMonth =
              export.Export1.Item.GexportEnd.Year * 100 + export
              .Export1.Item.GexportEnd.Month;
            export.HiddenCommon.Subscript = export.Export1.Index + 1;
          }
        }
      }

      export.Export1.CheckIndex();

      if (AsChar(local.OnlyOneSelected.Flag) != 'Y')
      {
        ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";

        return;
      }
    }

    if (Equal(global.Command, "IMHH") || Equal(global.Command, "UCOL") || Equal
      (global.Command, "URAC") || Equal(global.Command, "URAL") || Equal
      (global.Command, "URAH"))
    {
      // ***  take the first person selected, if a selection has been made ***
      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
        {
          export.SearchCsePersonsWorkSet.Number =
            export.Export1.Item.GcsePerson.Number;
          export.SearchCsePerson.Number = export.Export1.Item.GcsePerson.Number;
          export.SearchFromDateWorkAttributes.TextMonthYear =
            NumberToString(export.Export1.Item.GexportBegin.Month, 14, 2) + NumberToString
            (export.Export1.Item.GexportBegin.Year, 12, 4);
          export.SearchToDateWorkAttributes.TextMonthYear =
            NumberToString(export.Export1.Item.GexportEnd.Month, 14, 2) + NumberToString
            (export.Export1.Item.GexportEnd.Year, 12, 4);
          export.SearchFromDateWorkArea.Month =
            export.Export1.Item.GexportBegin.Month;
          export.SearchFromDateWorkArea.Year =
            export.Export1.Item.GexportBegin.Year;
          export.SearchFromDateWorkArea.YearMonth =
            export.Export1.Item.GexportBegin.Year * 100 + export
            .Export1.Item.GexportBegin.Month;
          export.SearchToDateWorkArea.Month =
            export.Export1.Item.GexportEnd.Month;
          export.SearchToDateWorkArea.Year =
            export.Export1.Item.GexportEnd.Year;
          export.SearchToDateWorkArea.YearMonth =
            export.Export1.Item.GexportEnd.Year * 100 + export
            .Export1.Item.GexportEnd.Month;
          export.HiddenCommon.Subscript = export.Export1.Index + 1;

          break;
        }
      }

      export.Export1.CheckIndex();
    }

    // mjr
    // -------------------------------------------------------------
    // Next tran and Security end here
    // ----------------------------------------------------------------
    // mjr
    // -------------------------------------------------------------
    //                C A S E   O F   C O M M A N D
    // ----------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.GexportScreenPrompt.PromptField) == 'S'
            )
          {
            export.HiddenCommon.Subscript = export.Export1.Index + 1;
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          }
          else if (AsChar(export.Export1.Item.GexportScreenPrompt.PromptField) !=
            '+' && !
            IsEmpty(export.Export1.Item.GexportScreenPrompt.PromptField))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field =
              GetField(export.Export1.Item.GexportScreenPrompt, "promptField");

            field.Error = true;

            return;
          }
        }

        export.Export1.CheckIndex();

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field =
              GetField(export.Export1.Item.GexportScreenPrompt, "promptField");

            field.Protected = false;
            field.Focused = true;

            return;
          }
        }

        export.Export1.CheckIndex();

        export.Export1.Index = 0;
        export.Export1.CheckSize();

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        var field1 =
          GetField(export.Export1.Item.GexportScreenPrompt, "promptField");

        field1.Protected = false;
        field1.Focused = true;

        break;
      case "ADD":
        if (!IsEmpty(export.ImHousehold.AeCaseNo))
        {
          if (!Equal(export.ImHousehold.AeCaseNo, export.Previous.AeCaseNo))
          {
            if (Lt("00000000", export.ImHousehold.AeCaseNo) && Lt
              (export.ImHousehold.AeCaseNo, "99999999"))
            {
              // -----------------------------------------------------------------
              // Pad AE case number with leading zeros
              // -----------------------------------------------------------------
              local.TextWorkArea.Text10 = export.ImHousehold.AeCaseNo;
              UseEabPadLeftWithZeros();
              export.ImHousehold.AeCaseNo =
                Substring(local.TextWorkArea.Text10, 3, 8);
            }

            if (ReadImHousehold())
            {
              export.ImHousehold.Assign(entities.ImHousehold);
              export.Previous.AeCaseNo = export.ImHousehold.AeCaseNo;

              var field = GetField(export.ImHousehold, "aeCaseNo");

              field.Protected = true;

              UseOeEabReadCaseBasicAda();

              switch(TrimEnd(local.AdabasResults.Text5))
              {
                case "NOTFD":
                  ExitState = "OE0000_AE_CASE_NBR_NOT_IN_ADABAS";

                  break;
                case "COMPL":
                  break;
                default:
                  break;
              }
            }
            else
            {
              if (Lt(export.ImHousehold.AeCaseNo, "01000000") || Lt
                ("03999999", export.ImHousehold.AeCaseNo))
              {
                var field = GetField(export.ImHousehold, "aeCaseNo");

                field.Error = true;

                ExitState = "OE0000_AE_CASE_NUMBER_INVALID";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              local.Action.ActionEntry = "AD";
              UseOeMaintainImHousehold();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
              else
              {
                ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

                var field = GetField(export.ImHousehold, "aeCaseNo");

                field.Color = "cyan";
                field.Protected = true;
              }
            }
          }
          else
          {
            var field = GetField(export.ImHousehold, "aeCaseNo");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
        else
        {
          var field = GetField(export.ImHousehold, "aeCaseNo");

          field.Protected = true;

          if (IsEmpty(export.HiddenConfirmCreate.Flag))
          {
            export.HiddenConfirmCreate.Flag = "Y";
            ExitState = "OE0000_CONFIRM_CREATE_PF5";

            return;
          }

          export.HiddenConfirmCreate.Flag = "";
          UseOeCreateConversionAeCase();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

            var field4 = GetField(export.ImHousehold, "aeCaseNo");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          return;
        }

        // ***  validate that a selection has been made
        local.OneOrMoreSelected.Flag = "N";

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            local.OneOrMoreSelected.Flag = "Y";
          }
        }

        export.Export1.CheckIndex();

        if (AsChar(local.OneOrMoreSelected.Flag) != 'Y')
        {
          if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            ExitState = "ACO_NN0000_NO_SELECTION_MADE";
          }

          return;
        }

        // ***  validate remaining fields in the group view
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            // ****** VALIDATE AMOUNTS *****
            if (export.Export1.Item.GimHouseholdMbrMnthlySum.GrantAmount.
              GetValueOrDefault() == 0 && export
              .Export1.Item.GimHouseholdMbrMnthlySum.GrantMedicalAmount.
                GetValueOrDefault() == 0)
            {
              var field4 =
                GetField(export.Export1.Item.GimHouseholdMbrMnthlySum,
                "grantMedicalAmount");

              field4.Error = true;

              var field5 =
                GetField(export.Export1.Item.GimHouseholdMbrMnthlySum,
                "grantAmount");

              field5.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }

            if (export.Export1.Item.GimHouseholdMbrMnthlySum.GrantAmount.
              GetValueOrDefault() > 0 && export
              .Export1.Item.GimHouseholdMbrMnthlySum.GrantMedicalAmount.
                GetValueOrDefault() > 0)
            {
              var field4 =
                GetField(export.Export1.Item.GimHouseholdMbrMnthlySum,
                "grantMedicalAmount");

              field4.Error = true;

              var field5 =
                GetField(export.Export1.Item.GimHouseholdMbrMnthlySum,
                "grantAmount");

              field5.Error = true;

              ExitState = "OE0000_ENTER_ONE_AMOUNT_ONLY";
            }

            // ****** ENDING DATE RANGE *****
            if (export.Export1.Item.GexportEnd.Month < 1 || export
              .Export1.Item.GexportEnd.Month > 12)
            {
              var field = GetField(export.Export1.Item.GexportEnd, "month");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATE";
            }

            if (export.Export1.Item.GexportEnd.Year < 1960)
            {
              var field = GetField(export.Export1.Item.GexportEnd, "year");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATE";
            }

            if (!IsExitState("ACO_NE0000_INVALID_DATE"))
            {
              export.Export1.Update.GexportEnd.Date =
                IntToDate(export.Export1.Item.GexportEnd.Year * 10000 + export
                .Export1.Item.GexportEnd.Month * 100 + 1);

              if (Lt(Now().Date, export.Export1.Item.GexportEnd.Date))
              {
                var field4 = GetField(export.Export1.Item.GexportEnd, "month");

                field4.Error = true;

                var field5 = GetField(export.Export1.Item.GexportEnd, "year");

                field5.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
              }
            }

            // ****** BEGINNING DATE RANGE *****
            if (export.Export1.Item.GexportBegin.Month < 1 || export
              .Export1.Item.GexportBegin.Month > 12)
            {
              var field = GetField(export.Export1.Item.GexportBegin, "month");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATE";
            }

            if (export.Export1.Item.GexportBegin.Year < 1960)
            {
              var field = GetField(export.Export1.Item.GexportBegin, "year");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATE";
            }

            if (!IsExitState("ACO_NE0000_INVALID_DATE"))
            {
              export.Export1.Update.GexportBegin.Date =
                IntToDate(export.Export1.Item.GexportBegin.Year * 10000 + export
                .Export1.Item.GexportBegin.Month * 100 + 1);

              if (Lt(export.Export1.Item.GexportEnd.Date,
                export.Export1.Item.GexportBegin.Date))
              {
                var field4 = GetField(export.Export1.Item.GexportEnd, "month");

                field4.Error = true;

                var field5 = GetField(export.Export1.Item.GexportEnd, "year");

                field5.Error = true;

                var field6 =
                  GetField(export.Export1.Item.GexportBegin, "month");

                field6.Error = true;

                var field7 = GetField(export.Export1.Item.GexportBegin, "year");

                field7.Error = true;

                ExitState = "ACO_NE0000_DATE_RANGE_ERROR";
              }

              if (Lt(Now().Date, export.Export1.Item.GexportBegin.Date))
              {
                var field4 =
                  GetField(export.Export1.Item.GexportBegin, "month");

                field4.Error = true;

                var field5 = GetField(export.Export1.Item.GexportBegin, "year");

                field5.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
              }
              else
              {
                // ** Date range can not span Oct 1, 1989, which is
                //    the boundary for checking person program.  Prior
                //    to that date any grant can be added by the user. ****
                // P. Phinney  06/11/2001    WR10342     Allow DEVELOPER profile
                // to bypass Grant Date Check
                if (AsChar(export.HiddenDeveloper.Flag) != 'Y' && AsChar
                  (export.HiddenDeveloper.Flag) != 'N')
                {
                  export.HiddenDeveloper.Flag = "N";

                  if (ReadServiceProviderProfileServiceProviderProfile())
                  {
                    export.HiddenDeveloper.Flag = "Y";
                  }
                  else
                  {
                    export.HiddenDeveloper.Flag = "N";
                  }
                }

                if (Lt(export.Export1.Item.GexportBegin.Date,
                  local.DateBoundary.Date))
                {
                  // P. Phinney  06/11/2001    WR10342     Allow DEVELOPER 
                  // profile to bypass Grant Date Check
                  if (AsChar(export.HiddenDeveloper.Flag) == 'Y')
                  {
                    goto Test;
                  }

                  if (!Lt(export.Export1.Item.GexportEnd.Date,
                    local.DateBoundary.Date))
                  {
                    var field =
                      GetField(export.Export1.Item.GexportEnd, "year");

                    field.Error = true;

                    ExitState = "ACO_NE0000_DATE_RANGE_ERROR";
                  }
                }
              }
            }

Test:

            if (!IsEmpty(export.Export1.Item.GcsePerson.Number))
            {
              local.TextWorkArea.Text10 = export.Export1.Item.GcsePerson.Number;
              UseEabPadLeftWithZeros();
              export.Export1.Update.GcsePerson.Number =
                local.TextWorkArea.Text10;

              if (ReadCsePerson())
              {
                export.SearchCsePersonsWorkSet.Number =
                  entities.CsePerson.Number;
                export.SearchCsePerson.Number = entities.CsePerson.Number;
                UseSiReadCsePerson();

                if (IsExitState("CSE_PERSON_NF"))
                {
                  var field =
                    GetField(export.Export1.Item.GcsePerson, "number");

                  field.Error = true;
                }
                else
                {
                  export.Export1.Update.GcsePersonsWorkSet.FormattedName =
                    export.SearchCsePersonsWorkSet.FormattedName;
                }
              }
              else
              {
                ExitState = "FN0000_CSE_PERSON_UNKNOWN";

                var field = GetField(export.Export1.Item.GcsePerson, "number");

                field.Error = true;
              }
            }
            else
            {
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";

              var field = GetField(export.Export1.Item.GcsePerson, "number");

              field.Error = true;
            }

            if (IsEmpty(export.Export1.Item.GexportPart.Relationship))
            {
              var field =
                GetField(export.Export1.Item.GexportPart, "relationship");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
            else
            {
              local.CodeValue.Cdvalue =
                export.Export1.Item.GexportPart.Relationship;
              UseCabValidateCodeValue();

              if (AsChar(local.ValidCode.Flag) == 'Y')
              {
              }
              else
              {
                var field =
                  GetField(export.Export1.Item.GexportPart, "relationship");

                field.Error = true;

                ExitState = "OE0000_INV_AE_CASE_ROLE";
              }
            }

            if (Equal(export.Export1.Item.GexportPart.Relationship, "PI"))
            {
              if (ReadImHouseholdMbrMnthlySum())
              {
                var field =
                  GetField(export.Export1.Item.GexportPart, "relationship");

                field.Error = true;

                ExitState = "OE0000_IM_CASE_WITH_MULTIPLE_PI";
              }
            }
          }
        }

        export.Export1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (IsEmpty(export.HiddenConfirmAdd.Flag))
        {
          export.HiddenConfirmAdd.Flag = "Y";
          ExitState = "OE0000_CONFIRM_ADD_PF5";

          return;
        }

        export.HiddenConfirmAdd.Flag = "";

        // ***** Begin adding summaries ********************
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            if (Equal(export.Export1.Item.GexportPart.Relationship, "PI"))
            {
              if (ReadImHouseholdMbrMnthlySum())
              {
                var field =
                  GetField(export.Export1.Item.GexportPart, "relationship");

                field.Error = true;

                ExitState = "OE0000_IM_CASE_WITH_MULTIPLE_PI";

                return;
              }
            }

            local.ImHouseholdMbrMnthlySum.GrantAmount =
              export.Export1.Item.GimHouseholdMbrMnthlySum.GrantAmount.
                GetValueOrDefault();
            local.ImHouseholdMbrMnthlySum.GrantMedicalAmount =
              export.Export1.Item.GimHouseholdMbrMnthlySum.GrantMedicalAmount.
                GetValueOrDefault();
            local.ImHouseholdMbrMnthlySum.Relationship =
              export.Export1.Item.GexportPart.Relationship;
            local.Summary.Date = export.Export1.Item.GexportBegin.Date;

            while(!Lt(export.Export1.Item.GexportEnd.Date, local.Summary.Date))
            {
              local.ImHouseholdMbrMnthlySum.Month = Month(local.Summary.Date);
              local.ImHouseholdMbrMnthlySum.Year = Year(local.Summary.Date);
              UseOeMaintainHouseholdMember();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field = GetField(export.Export1.Item.Gcommon, "selectChar");

                field.Error = true;

                UseEabRollbackCics();

                return;
              }

              local.Summary.Date = AddMonths(local.Summary.Date, 1);
            }

            export.Export1.Update.Gcommon.SelectChar = "*";
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

            for(export.Export1.Index = 0; export.Export1.Index < Export
              .ExportGroup.Capacity; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (IsEmpty(export.Export1.Item.Gcommon.SelectChar))
              {
                var field = GetField(export.Export1.Item.Gcommon, "selectChar");

                field.Protected = false;
                field.Focused = true;

                break;
              }
            }

            export.Export1.CheckIndex();

            return;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (IsEmpty(export.Export1.Item.Gcommon.SelectChar))
          {
            var field = GetField(export.Export1.Item.Gcommon, "selectChar");

            field.Protected = false;
            field.Focused = true;

            return;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        if (!IsEmpty(export.ImHousehold.AeCaseNo))
        {
          if (!Equal(export.ImHousehold.AeCaseNo, export.Previous.AeCaseNo))
          {
            if (Lt("00000000", export.ImHousehold.AeCaseNo) && Lt
              (export.ImHousehold.AeCaseNo, "99999999"))
            {
              // -----------------------------------------------------------------
              // Pad AE case number with leading zeros
              // -----------------------------------------------------------------
              local.TextWorkArea.Text10 = export.ImHousehold.AeCaseNo;
              UseEabPadLeftWithZeros();
              export.ImHousehold.AeCaseNo =
                Substring(local.TextWorkArea.Text10, 3, 8);
            }

            if (ReadImHousehold())
            {
              export.ImHousehold.Assign(entities.ImHousehold);
              export.Previous.AeCaseNo = export.ImHousehold.AeCaseNo;

              var field4 = GetField(export.ImHousehold, "aeCaseNo");

              field4.Color = "cyan";
              field4.Protected = true;

              UseOeEabReadCaseBasicAda();

              switch(TrimEnd(local.AdabasResults.Text5))
              {
                case "NOTFD":
                  ExitState = "OE0000_AE_CASE_NBR_NOT_IN_ADABAS";

                  return;
                case "COMPL":
                  break;
                default:
                  break;
              }

              export.Export1.Index = 0;
              export.Export1.CheckSize();

              var field5 = GetField(export.Export1.Item.Gcommon, "selectChar");

              field5.Protected = false;
              field5.Focused = true;

              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            }
            else
            {
              ExitState = "OE0000_IM_HOUSEHOLD_NF";

              var field = GetField(export.ImHousehold, "aeCaseNo");

              field.Error = true;
            }
          }
          else
          {
            var field = GetField(export.ImHousehold, "aeCaseNo");

            field.Color = "cyan";
            field.Protected = true;

            for(export.Export1.Index = 0; export.Export1.Index < Export
              .ExportGroup.Capacity; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
              {
                var field4 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field4.Protected = false;
                field4.Focused = true;

                break;
              }
            }

            export.Export1.CheckIndex();
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }
        else
        {
          var field = GetField(export.ImHousehold, "aeCaseNo");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        break;
      case "IMHH":
        // *** PF15 ***
        ExitState = "ECO_LNK_TO_IMHH";

        break;
      case "URAH":
        // *** PF16 ***
        ExitState = "ECO_LNK_TO_URAH";

        break;
      case "URAC":
        // *** PF17 ***
        ExitState = "ECO_LNK_TO_URAC";

        break;
      case "CURA":
        // *** PF18 ***
        ExitState = "ECO_LNK_TO_CURA";

        break;
      case "URAA":
        // *** PF19 ***
        ExitState = "ECO_LNK_TO_URAA";

        break;
      case "URAL":
        // *** PF20 ***
        ExitState = "ECO_LNK_TO_URAL";

        break;
      case "UCOL":
        // *** PF21 ***
        ExitState = "ECO_LNK_TO_UCOL";

        break;
      case "PEPR":
        // *** PF22 ***
        ExitState = "ECO_LNK_TO_PEPR";

        break;
      case "RETLINK":
        // *** Returning from name list ***
        var field2 = GetField(export.ImHousehold, "aeCaseNo");

        field2.Color = "cyan";
        field2.Protected = true;

        export.Export1.Index = import.HiddenCommon.Subscript - 1;
        export.Export1.CheckSize();

        export.Export1.Update.GcsePerson.Number = import.Search.Number;
        export.Export1.Update.GcsePersonsWorkSet.FormattedName =
          import.Search.FormattedName;
        export.Export1.Update.GexportScreenPrompt.PromptField = "+";

        var field3 = GetField(export.Export1.Item.GexportBegin, "month");

        field3.Protected = false;
        field3.Focused = true;

        break;
      case "RETURN":
        // *** Returning to calling screen ***
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        if (!IsEmpty(export.ImHousehold.AeCaseNo))
        {
          if (Equal(export.ImHousehold.AeCaseNo, export.Previous.AeCaseNo))
          {
            var field = GetField(export.ImHousehold, "aeCaseNo");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveImHousehold(ImHousehold source, ImHousehold target)
  {
    target.CaseStatus = source.CaseStatus;
    target.StatusDate = source.StatusDate;
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.GrantAmount = source.GrantAmount;
    target.GrantMedicalAmount = source.GrantMedicalAmount;
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

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text80 = source.Text80;
    target.Text5 = source.Text5;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Return1.Assign(useExport.CodeValue);
    local.ReturnCode.Count = useExport.ReturnCode.Count;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCreateConversionAeCase()
  {
    var useImport = new OeCreateConversionAeCase.Import();
    var useExport = new OeCreateConversionAeCase.Export();

    useImport.ImHousehold.FirstBenefitDate =
      export.ImHousehold.FirstBenefitDate;

    Call(OeCreateConversionAeCase.Execute, useImport, useExport);

    export.ImHousehold.Assign(useExport.ImHousehold);
  }

  private void UseOeEabReadCaseBasicAda()
  {
    var useImport = new OeEabReadCaseBasicAda.Import();
    var useExport = new OeEabReadCaseBasicAda.Export();

    useImport.ImHousehold.AeCaseNo = export.ImHousehold.AeCaseNo;
    MoveWorkArea(local.AdabasResults, useExport.ExecResults);
    MoveImHousehold(export.ImHousehold, useExport.ImHousehold);

    Call(OeEabReadCaseBasicAda.Execute, useImport, useExport);

    MoveWorkArea(useExport.ExecResults, local.AdabasResults);
    MoveImHousehold(useExport.ImHousehold, export.ImHousehold);
  }

  private void UseOeMaintainHouseholdMember()
  {
    var useImport = new OeMaintainHouseholdMember.Import();
    var useExport = new OeMaintainHouseholdMember.Export();

    useImport.ImHousehold.AeCaseNo = import.ImHousehold.AeCaseNo;
    useImport.ImHouseholdMbrMnthlySum.Assign(local.ImHouseholdMbrMnthlySum);
    useImport.Boundary.Date = local.DateBoundary.Date;
    useImport.Summary.Date = local.Summary.Date;
    useImport.CsePerson.Number = export.Export1.Item.GcsePerson.Number;

    Call(OeMaintainHouseholdMember.Execute, useImport, useExport);
  }

  private void UseOeMaintainImHousehold()
  {
    var useImport = new OeMaintainImHousehold.Import();
    var useExport = new OeMaintainImHousehold.Export();

    useImport.Action.ActionEntry = local.Action.ActionEntry;
    useImport.ImHousehold.Assign(export.ImHousehold);

    Call(OeMaintainImHousehold.Execute, useImport, useExport);
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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.SearchCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.SearchCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Export1.Item.GcsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", export.ImHousehold.AeCaseNo);
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

  private bool ReadImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", export.ImHousehold.AeCaseNo);
        db.
          SetString(command, "cspNumber", export.Export1.Item.GcsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 3);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private bool ReadServiceProviderProfileServiceProviderProfile()
  {
    entities.ServiceProvider.Populated = false;
    entities.Profile.Populated = false;
    entities.ServiceProviderProfile.Populated = false;
    entities.ProfileAuthorization.Populated = false;

    return Read("ReadServiceProviderProfileServiceProviderProfile",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "userId", global.UserId);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderProfile.SpdGenId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.Profile.Name = db.GetString(reader, 2);
        entities.ServiceProviderProfile.ProName = db.GetString(reader, 2);
        entities.ServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ServiceProviderProfile.EffectiveDate = db.GetDate(reader, 4);
        entities.ServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.ProfileAuthorization.ActiveInd =
          db.GetNullableString(reader, 7);
        entities.ProfileAuthorization.FkProName = db.GetString(reader, 8);
        entities.ProfileAuthorization.FkTrnTrancode = db.GetString(reader, 9);
        entities.ProfileAuthorization.FkTrnScreenid = db.GetString(reader, 10);
        entities.ProfileAuthorization.FkCmdValue = db.GetString(reader, 11);
        entities.ServiceProvider.Populated = true;
        entities.Profile.Populated = true;
        entities.ServiceProviderProfile.Populated = true;
        entities.ProfileAuthorization.Populated = true;
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
      /// A value of GimportPart.
      /// </summary>
      [JsonPropertyName("gimportPart")]
      public ImHouseholdMbrMnthlySum GimportPart
      {
        get => gimportPart ??= new();
        set => gimportPart = value;
      }

      /// <summary>
      /// A value of GcsePerson.
      /// </summary>
      [JsonPropertyName("gcsePerson")]
      public CsePerson GcsePerson
      {
        get => gcsePerson ??= new();
        set => gcsePerson = value;
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
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GimportBegin.
      /// </summary>
      [JsonPropertyName("gimportBegin")]
      public DateWorkArea GimportBegin
      {
        get => gimportBegin ??= new();
        set => gimportBegin = value;
      }

      /// <summary>
      /// A value of GimportEnd.
      /// </summary>
      [JsonPropertyName("gimportEnd")]
      public DateWorkArea GimportEnd
      {
        get => gimportEnd ??= new();
        set => gimportEnd = value;
      }

      /// <summary>
      /// A value of GimHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum GimHouseholdMbrMnthlySum
      {
        get => gimHouseholdMbrMnthlySum ??= new();
        set => gimHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Common gcommon;
      private ImHouseholdMbrMnthlySum gimportPart;
      private CsePerson gcsePerson;
      private Standard gimportScreenPrompt;
      private CsePersonsWorkSet gcsePersonsWorkSet;
      private DateWorkArea gimportBegin;
      private DateWorkArea gimportEnd;
      private ImHouseholdMbrMnthlySum gimHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// A value of HiddenConfirmCreate.
    /// </summary>
    [JsonPropertyName("hiddenConfirmCreate")]
    public Common HiddenConfirmCreate
    {
      get => hiddenConfirmCreate ??= new();
      set => hiddenConfirmCreate = value;
    }

    /// <summary>
    /// A value of HiddenConfirmAdd.
    /// </summary>
    [JsonPropertyName("hiddenConfirmAdd")]
    public Common HiddenConfirmAdd
    {
      get => hiddenConfirmAdd ??= new();
      set => hiddenConfirmAdd = value;
    }

    /// <summary>
    /// A value of HiddenCommon.
    /// </summary>
    [JsonPropertyName("hiddenCommon")]
    public Common HiddenCommon
    {
      get => hiddenCommon ??= new();
      set => hiddenCommon = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public ImHousehold Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of HiddenDeveloper.
    /// </summary>
    [JsonPropertyName("hiddenDeveloper")]
    public Common HiddenDeveloper
    {
      get => hiddenDeveloper ??= new();
      set => hiddenDeveloper = value;
    }

    private Common hiddenConfirmCreate;
    private Common hiddenConfirmAdd;
    private Common hiddenCommon;
    private CsePersonsWorkSet search;
    private ImHousehold previous;
    private ImHousehold imHousehold;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ImportGroup> import1;
    private Common hiddenDeveloper;
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
      /// A value of GexportPart.
      /// </summary>
      [JsonPropertyName("gexportPart")]
      public ImHouseholdMbrMnthlySum GexportPart
      {
        get => gexportPart ??= new();
        set => gexportPart = value;
      }

      /// <summary>
      /// A value of GcsePerson.
      /// </summary>
      [JsonPropertyName("gcsePerson")]
      public CsePerson GcsePerson
      {
        get => gcsePerson ??= new();
        set => gcsePerson = value;
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
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GexportBegin.
      /// </summary>
      [JsonPropertyName("gexportBegin")]
      public DateWorkArea GexportBegin
      {
        get => gexportBegin ??= new();
        set => gexportBegin = value;
      }

      /// <summary>
      /// A value of GexportEnd.
      /// </summary>
      [JsonPropertyName("gexportEnd")]
      public DateWorkArea GexportEnd
      {
        get => gexportEnd ??= new();
        set => gexportEnd = value;
      }

      /// <summary>
      /// A value of GimHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("gimHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum GimHouseholdMbrMnthlySum
      {
        get => gimHouseholdMbrMnthlySum ??= new();
        set => gimHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Common gcommon;
      private ImHouseholdMbrMnthlySum gexportPart;
      private CsePerson gcsePerson;
      private Standard gexportScreenPrompt;
      private CsePersonsWorkSet gcsePersonsWorkSet;
      private DateWorkArea gexportBegin;
      private DateWorkArea gexportEnd;
      private ImHouseholdMbrMnthlySum gimHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// A value of HiddenConfirmCreate.
    /// </summary>
    [JsonPropertyName("hiddenConfirmCreate")]
    public Common HiddenConfirmCreate
    {
      get => hiddenConfirmCreate ??= new();
      set => hiddenConfirmCreate = value;
    }

    /// <summary>
    /// A value of HiddenConfirmAdd.
    /// </summary>
    [JsonPropertyName("hiddenConfirmAdd")]
    public Common HiddenConfirmAdd
    {
      get => hiddenConfirmAdd ??= new();
      set => hiddenConfirmAdd = value;
    }

    /// <summary>
    /// A value of HiddenCommon.
    /// </summary>
    [JsonPropertyName("hiddenCommon")]
    public Common HiddenCommon
    {
      get => hiddenCommon ??= new();
      set => hiddenCommon = value;
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
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public ImHousehold Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of SearchToDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("searchToDateWorkAttributes")]
    public DateWorkAttributes SearchToDateWorkAttributes
    {
      get => searchToDateWorkAttributes ??= new();
      set => searchToDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of SearchFromDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("searchFromDateWorkAttributes")]
    public DateWorkAttributes SearchFromDateWorkAttributes
    {
      get => searchFromDateWorkAttributes ??= new();
      set => searchFromDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of SearchToDateWorkArea.
    /// </summary>
    [JsonPropertyName("searchToDateWorkArea")]
    public DateWorkArea SearchToDateWorkArea
    {
      get => searchToDateWorkArea ??= new();
      set => searchToDateWorkArea = value;
    }

    /// <summary>
    /// A value of SearchFromDateWorkArea.
    /// </summary>
    [JsonPropertyName("searchFromDateWorkArea")]
    public DateWorkArea SearchFromDateWorkArea
    {
      get => searchFromDateWorkArea ??= new();
      set => searchFromDateWorkArea = value;
    }

    /// <summary>
    /// A value of HiddenDeveloper.
    /// </summary>
    [JsonPropertyName("hiddenDeveloper")]
    public Common HiddenDeveloper
    {
      get => hiddenDeveloper ??= new();
      set => hiddenDeveloper = value;
    }

    private Common hiddenConfirmCreate;
    private Common hiddenConfirmAdd;
    private Common hiddenCommon;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private CsePerson searchCsePerson;
    private Array<ExportGroup> export1;
    private ImHousehold previous;
    private ImHousehold imHousehold;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private DateWorkAttributes searchToDateWorkAttributes;
    private DateWorkAttributes searchFromDateWorkAttributes;
    private DateWorkArea searchToDateWorkArea;
    private DateWorkArea searchFromDateWorkArea;
    private Common hiddenDeveloper;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AdabasResults.
    /// </summary>
    [JsonPropertyName("adabasResults")]
    public WorkArea AdabasResults
    {
      get => adabasResults ??= new();
      set => adabasResults = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public CodeValue Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Action.
    /// </summary>
    [JsonPropertyName("action")]
    public Common Action
    {
      get => action ??= new();
      set => action = value;
    }

    /// <summary>
    /// A value of OneOrMorePersonError.
    /// </summary>
    [JsonPropertyName("oneOrMorePersonError")]
    public Common OneOrMorePersonError
    {
      get => oneOrMorePersonError ??= new();
      set => oneOrMorePersonError = value;
    }

    /// <summary>
    /// A value of OneOrMoreSelected.
    /// </summary>
    [JsonPropertyName("oneOrMoreSelected")]
    public Common OneOrMoreSelected
    {
      get => oneOrMoreSelected ??= new();
      set => oneOrMoreSelected = value;
    }

    /// <summary>
    /// A value of OnlyOneSelected.
    /// </summary>
    [JsonPropertyName("onlyOneSelected")]
    public Common OnlyOneSelected
    {
      get => onlyOneSelected ??= new();
      set => onlyOneSelected = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of DateBoundary.
    /// </summary>
    [JsonPropertyName("dateBoundary")]
    public DateWorkArea DateBoundary
    {
      get => dateBoundary ??= new();
      set => dateBoundary = value;
    }

    /// <summary>
    /// A value of DateMinimum.
    /// </summary>
    [JsonPropertyName("dateMinimum")]
    public DateWorkArea DateMinimum
    {
      get => dateMinimum ??= new();
      set => dateMinimum = value;
    }

    /// <summary>
    /// A value of Summary.
    /// </summary>
    [JsonPropertyName("summary")]
    public DateWorkArea Summary
    {
      get => summary ??= new();
      set => summary = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    private WorkArea adabasResults;
    private CodeValue return1;
    private Common returnCode;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private Common action;
    private Common oneOrMorePersonError;
    private Common oneOrMoreSelected;
    private Common onlyOneSelected;
    private External external;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private DateWorkArea dateBoundary;
    private DateWorkArea dateMinimum;
    private DateWorkArea summary;
    private DateWorkArea null1;
    private TextWorkArea textWorkArea;
    private NextTranInfo nextTranInfo;
    private Common select;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private Program program;
    private PersonProgram personProgram;
    private CsePerson csePerson;
    private ImHousehold imHousehold;
    private ServiceProvider serviceProvider;
    private Profile profile;
    private ServiceProviderProfile serviceProviderProfile;
    private ProfileAuthorization profileAuthorization;
  }
#endregion
}

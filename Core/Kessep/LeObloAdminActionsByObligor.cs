// Program: LE_OBLO_ADMIN_ACTIONS_BY_OBLIGOR, ID: 372601434, model: 746.
// Short name: SWEOBLOP
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
/// A program: LE_OBLO_ADMIN_ACTIONS_BY_OBLIGOR.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step action block lists the Obligor
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeObloAdminActionsByObligor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_OBLO_ADMIN_ACTIONS_BY_OBLIGOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeObloAdminActionsByObligor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeObloAdminActionsByObligor.
  /// </summary>
  public LeObloAdminActionsByObligor(IContext context, Import import,
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
    // *******************************************************************
    // Date			Developer	Request #
    // 09-13-95		Govind
    // Initial development
    // 01/07/95		T.O.Redmond
    // Add Supporting data for Obligation Type
    // 09/24/98		P. Sharp
    // Clean up based on Phase II assessment. Made sel char part
    // of group view. Removed code for CSE person prompt.  If
    // option is spaces will include both auto and manual action in
    // group view.
    // Modified exit states.
    // 10/20/99		R.Jean
    // PR74995 - Dun administrative act certifications display
    // 000000 as date.
    // 03/22/01   Madhu Kumar           PR#112209.
    // Made changes to the screen display so that it does not display a zero on 
    // the ADC amount but displays blank.
    // *******************************************************************
    // 12/18/01   PPhinney     WR020119 - Complete ReStructure of Display Logic 
    // and Screen
    // 09/24/02   PPhinney     I00158491 - FIX Amount being BLANKed by PF7 PF8
    // 06/20/08   MFan    CQ567/PR301286 - Deleted the literal 'Manual' for the 
    // manual exemption column heading
    //                                     
    // from the Pstep's screen.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      // -------------
      // begin group F
      // -------------
      UseScCabSignoff();

      return;

      // -------------
      // end   group F
      // -------------
    }

    export.Case1.Number = import.Case1.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.HiddenPrev.Number = import.HiddenPrev.Number;

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    export.Required.Type1 = import.Required.Type1;
    export.StartDate.Date = import.StartDate.Date;
    export.PromptAdminActType.PromptField =
      import.PromptAdminActType.PromptField;
    export.ListOptSystManualActs.OneChar = import.ListOptSystManualActs.OneChar;
    export.One.Number = import.One.Number;
    export.Two.Number = import.Two.Number;
    export.Three.Number = import.Three.Number;
    export.Four.Number = import.Four.Number;
    export.Five.Number = import.Five.Number;
    export.Six.Number = import.Six.Number;
    export.Seven.Number = import.Seven.Number;
    export.Eight.Number = import.Eight.Number;
    export.Nine.Number = import.Nine.Number;
    export.Ten.Number = import.Ten.Number;
    export.Eleven.Number = import.Eleven.Number;
    export.Twelve.Number = import.Twelve.Number;

    if (AsChar(import.ShowAll.Flag) != 'N')
    {
      export.ShowAll.Flag = "Y";
    }
    else
    {
      export.ShowAll.Flag = import.ShowAll.Flag;
    }

    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    switch(AsChar(export.ListOptSystManualActs.OneChar))
    {
      case 'M':
        if (AsChar(import.ShowAll.Flag) == 'N')
        {
          var field1 = GetField(export.ListOptSystManualActs, "oneChar");

          field1.Error = true;

          var field2 = GetField(export.ShowAll, "flag");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_ACTION";

          return;
        }

        break;
      case 'A':
        break;
      case ' ':
        break;
      default:
        var field = GetField(export.ListOptSystManualActs, "oneChar");

        field.Error = true;

        ExitState = "LE0000_INVALID_OPT_MANUAL_AUTO";

        return;
    }

    if (AsChar(import.ShowAll.Flag) == 'N' && !
      Equal(import.StartDate.Date, local.BlankStartDate.Date))
    {
      var field1 = GetField(export.StartDate, "date");

      field1.Error = true;

      var field2 = GetField(export.ShowAll, "flag");

      field2.Error = true;

      ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (!import.AdminAction.IsEmpty)
      {
        export.AdminAction.Index = -1;

        for(import.AdminAction.Index = 0; import.AdminAction.Index < import
          .AdminAction.Count; ++import.AdminAction.Index)
        {
          ++export.AdminAction.Index;
          export.AdminAction.CheckSize();

          export.AdminAction.Update.DetailHiddenOblgr.Number =
            import.AdminAction.Item.DetailHiddenOblgr.Number;
          export.AdminAction.Update.DetailHiddenCrtfd.SelectChar =
            import.AdminAction.Item.DetailHiddenCrtfd.SelectChar;
          export.AdminAction.Update.DetailHidden.SystemGeneratedIdentifier =
            import.AdminAction.Item.DetailHidden.SystemGeneratedIdentifier;
          export.AdminAction.Update.SelectOption.SelectChar =
            import.AdminAction.Item.SelectOption.SelectChar;
          export.AdminAction.Update.DetailAdministrativeActCertification.Assign(
            import.AdminAction.Item.DetailAdministrativeActCertification);
          MoveAdministrativeActCertification(import.AdminAction.Item.
            DetailFederalDebtSetoff,
            export.AdminAction.Update.DetailFederalDebtSetoff);
          export.AdminAction.Update.DetailAdministrativeAction.Type1 =
            import.AdminAction.Item.DetailAdministrativeAction.Type1;
          export.AdminAction.Update.DetailObligationAdministrativeAction.
            TakenDate =
              import.AdminAction.Item.DetailObligationAdministrativeAction.
              TakenDate;
          MoveObligationType(import.AdminAction.Item.DetailObligationType,
            export.AdminAction.Update.DetailObligationType);
          export.AdminAction.Update.SsnChangeFlag.Flag =
            import.AdminAction.Item.SsnChangeFlag.Flag;
          export.AdminAction.Update.FdsoDecertFlag.Flag =
            import.AdminAction.Item.FdsoDecertFlag.Flag;
          export.AdminAction.Update.ManAutoFlag.Flag =
            import.AdminAction.Item.ManAutoFlag.Flag;
          export.AdminAction.Update.FlowExmp.SelectChar =
            import.AdminAction.Item.FlowExmp.SelectChar;
          export.AdminAction.Update.Pass.Type1 =
            import.AdminAction.Item.Pass.Type1;

          if (!Equal(export.AdminAction.Item.
            DetailAdministrativeActCertification.Type1, "FDSO"))
          {
            var field =
              GetField(export.AdminAction.Item.DetailFederalDebtSetoff,
              "nonAdcAmount");

            field.Intensity = Intensity.Dark;
            field.Protected = true;
          }

          // * * * *   PR on the Way  09/18/2002
          // 09/24/02   PPhinney     I00158491 - FIX Amount being BLANKed by PF7
          // PF8
          // -  Was checking WRONG TYPE code to Darken field - fixed
          // * * * *
          if (Equal(export.AdminAction.Item.DetailAdministrativeAction.Type1,
            "CRED") || Equal
            (export.AdminAction.Item.DetailAdministrativeAction.Type1, "FDSO") ||
            Equal
            (export.AdminAction.Item.DetailAdministrativeAction.Type1, "SDSO") ||
            Equal
            (export.AdminAction.Item.DetailAdministrativeAction.Type1, "KDWP") ||
            Equal
            (export.AdminAction.Item.DetailAdministrativeAction.Type1, "KDMV"))
          {
          }
          else
          {
            var field =
              GetField(export.AdminAction.Item.
                DetailAdministrativeActCertification, "currentAmount");

            field.Intensity = Intensity.Dark;
            field.Protected = true;
          }

          if (AsChar(export.AdminAction.Item.ManAutoFlag.Flag) == 'Y')
          {
          }
          else
          {
            var field =
              GetField(export.AdminAction.Item.FlowExmp, "selectChar");

            field.Color = "";
            field.Intensity = Intensity.Dark;
            field.Protected = true;
          }
        }
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ----------------------------------------------------------
    // The following statements must be placed after MOVE imports
    // to exports
    // ----------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------------------
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // -------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // -------------------------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "RETADAA"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETADAA"))
    {
      export.PromptAdminActType.PromptField = "";
    }

    if (Equal(global.Command, "DETAIL"))
    {
      local.FlowToDetail.Flag = "Y";
      global.Command = "RETURN";
    }
    else
    {
      local.FlowToDetail.Flag = "N";
    }

    // PR 155008  Separated so it will DISPLAY upon return from List
    if (Equal(global.Command, "RETADAA"))
    {
      export.PromptAdminActType.PromptField = "";

      if (IsEmpty(export.Required.Type1))
      {
        var field = GetField(export.Required, "type1");

        field.Protected = false;
        field.Focused = true;
      }
      else
      {
        var field = GetField(export.StartDate, "date");

        field.Protected = false;
        field.Focused = true;
      }

      global.Command = "DISPLAY";
    }
    else
    {
    }

    switch(TrimEnd(global.Command))
    {
      case "RETADAA":
        // * * *   SHOULD NEVER GET IN HERE  * * *
        // PR 155008  Separated so it will DISPLAY upon return from List
        break;
      case "LIST":
        if (!IsEmpty(export.PromptAdminActType.PromptField) && AsChar
          (export.PromptAdminActType.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptAdminActType, "promptField");

          field.Error = true;

          return;
        }

        local.CountExmp.Count = 0;
        export.AdminAction.Index = -1;

        for(import.AdminAction.Index = 0; import.AdminAction.Index < import
          .AdminAction.Count; ++import.AdminAction.Index)
        {
          ++export.AdminAction.Index;
          export.AdminAction.CheckSize();

          if (AsChar(export.AdminAction.Item.FlowExmp.SelectChar) != 'S' && !
            IsEmpty(export.AdminAction.Item.FlowExmp.SelectChar))
          {
            var field =
              GetField(export.AdminAction.Item.FlowExmp, "selectChar");

            field.Error = true;

            ++local.CountExmp.Count;
          }
        }

        if (local.CountExmp.Count > 0)
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }

        local.CountExmp.Count = 0;
        export.AdminAction.Index = -1;

        for(import.AdminAction.Index = 0; import.AdminAction.Index < import
          .AdminAction.Count; ++import.AdminAction.Index)
        {
          ++export.AdminAction.Index;
          export.AdminAction.CheckSize();

          if (AsChar(export.AdminAction.Item.FlowExmp.SelectChar) == 'S')
          {
            var field =
              GetField(export.AdminAction.Item.FlowExmp, "selectChar");

            field.Error = true;

            ++local.CountExmp.Count;
          }
        }

        if (local.CountExmp.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        if (AsChar(export.PromptAdminActType.PromptField) == 'S' && local
          .CountExmp.Count > 0)
        {
          var field = GetField(export.PromptAdminActType, "promptField");

          field.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.PromptAdminActType.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";

          return;
        }

        export.AdminAction.Index = -1;

        for(import.AdminAction.Index = 0; import.AdminAction.Index < import
          .AdminAction.Count; ++import.AdminAction.Index)
        {
          ++export.AdminAction.Index;
          export.AdminAction.CheckSize();

          if (AsChar(export.AdminAction.Item.FlowExmp.SelectChar) == 'S')
          {
            export.AdminAction.Update.FlowExmp.SelectChar = "";
            MoveObligationType(export.AdminAction.Item.DetailObligationType,
              export.SelectedObligationType);
            export.AllObligations.Flag = "N";
            export.SelectedObligation.SystemGeneratedIdentifier =
              export.AdminAction.Item.DetailHidden.SystemGeneratedIdentifier;
            export.SelectedAdministrativeAction.Type1 =
              export.AdminAction.Item.Pass.Type1;
            ExitState = "ECO_LNK_TO_EXMP_ADM_ACT_EXEMPTN";

            return;
          }
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "DISPLAY":
        MoveCsePersonsWorkSet1(export.CsePersonsWorkSet, local.Saved);

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          if (IsEmpty(export.CsePersonsWorkSet.Ssn))
          {
            // PR 154979  Make SSN and CSE Person Number ERROR
            var field1 = GetField(export.CsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

            field2.Error = true;

            var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

            field3.Error = true;

            var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

            field4.Error = true;

            ExitState = "LE0000_CSE_NO_OR_SSN_REQD";
            MoveCsePersonsWorkSet4(local.InitialisedToSpaces,
              export.CsePersonsWorkSet);

            return;
          }
          else
          {
            local.SearchOption.Flag = "1";
            UseCabMatchCsePerson();

            local.MatchCsePersons.Index = 0;
            local.MatchCsePersons.CheckSize();

            MoveCsePersonsWorkSet2(local.MatchCsePersons.Item.DetailMatchCsePer,
              export.CsePersonsWorkSet);

            if (IsEmpty(export.CsePersonsWorkSet.Number))
            {
              ExitState = "CSE_PERSON_NF";
            }

            UseSiFormatCsePersonName();
          }
        }
        else
        {
          UseSiReadCsePerson();
        }

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          // ---------------------------------------------
          // CSE person not found. So move the import value back
          // ---------------------------------------------
          MoveCsePersonsWorkSet1(local.Saved, export.CsePersonsWorkSet);
          MoveCsePersonsWorkSet4(local.InitialisedToSpaces,
            export.CsePersonsWorkSet);

          if (!IsEmpty(export.CsePersonsWorkSet.Number))
          {
            export.CsePersonsWorkSet.Ssn = "";
          }

          return;
        }

        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeListAdminActionsByObligor();

        if (IsExitState("LE0000_LIST_NOT_MATCH_TYPE"))
        {
          var field1 = GetField(export.Required, "type1");

          field1.Error = true;

          var field2 = GetField(export.ListOptSystManualActs, "oneChar");

          field2.Error = true;

          return;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }
        else if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }
        else if (IsExitState("LE0000_INVALID_ADMIN_ACTION_TYPE"))
        {
          var field = GetField(export.Required, "type1");

          field.Error = true;

          return;
        }
        else
        {
          // ****************************************************************
          // 10/20/99	R.Jean	PR74995 - Move the taken date to the
          // curr amt date (displayed on screen) if the type is not FDSO,
          // SDSO, CRED.
          // ****************************************************************
          for(export.AdminAction.Index = 0; export.AdminAction.Index < export
            .AdminAction.Count; ++export.AdminAction.Index)
          {
            if (!export.AdminAction.CheckSize())
            {
              break;
            }

            export.AdminAction.Update.DetailAdministrativeActCertification.
              CurrentAmountDate =
                export.AdminAction.Item.DetailAdministrativeActCertification.
                TakenDate;

            // * * Depending on Type of CERTIFICATION - Different fields will be
            // AVAILIABLE
            if (AsChar(export.AdminAction.Item.ManAutoFlag.Flag) == 'Y')
            {
              if (Equal(export.AdminAction.Item.DetailAdministrativeAction.
                Type1, "CRED") || Equal
                (export.AdminAction.Item.DetailAdministrativeAction.Type1,
                "FDSO") || Equal
                (export.AdminAction.Item.DetailAdministrativeAction.Type1,
                "SDSO") || Equal
                (export.AdminAction.Item.DetailAdministrativeAction.Type1,
                "KDWP") || Equal
                (export.AdminAction.Item.DetailAdministrativeAction.Type1,
                "KDMV"))
              {
                // * *  AUTOMATIC Certification
                // * DO Display Current Amount
                var field1 =
                  GetField(export.AdminAction.Item.
                    DetailAdministrativeActCertification, "currentAmount");

                field1.Color = "cyan";
                field1.Protected = true;

                // * DO Display PROMPT to Flow to EXMP Screen
                var field2 =
                  GetField(export.AdminAction.Item.FlowExmp, "selectChar");

                field2.Color = "green";
                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;

                if (!Equal(export.AdminAction.Item.
                  DetailAdministrativeActCertification.Type1, "FDSO"))
                {
                  // * *  AUTOMATIC  *NOT*  FDSO Certification
                  // * Do NOT Display Non_ADC_Amount
                  var field =
                    GetField(export.AdminAction.Item.DetailFederalDebtSetoff,
                    "nonAdcAmount");

                  field.Intensity = Intensity.Dark;
                  field.Protected = true;
                }
                else
                {
                  // * *  AUTOMATIC FDSO Certification
                  // * Do Display Non_ADC_Amount
                  var field =
                    GetField(export.AdminAction.Item.DetailFederalDebtSetoff,
                    "nonAdcAmount");

                  field.Color = "cyan";
                  field.Protected = true;
                }
              }
              else
              {
                // * *  MANUAL Certification
                // * Do NOT Display Current Amount OR Non_ADC_Amount
                var field1 =
                  GetField(export.AdminAction.Item.
                    DetailAdministrativeActCertification, "currentAmount");

                field1.Intensity = Intensity.Dark;
                field1.Protected = true;

                var field2 =
                  GetField(export.AdminAction.Item.DetailFederalDebtSetoff,
                  "nonAdcAmount");

                field2.Intensity = Intensity.Dark;
                field2.Protected = true;

                // * DO Display PROMPT to Flow to EXMP Screen
                var field3 =
                  GetField(export.AdminAction.Item.FlowExmp, "selectChar");

                field3.Color = "green";
                field3.Intensity = Intensity.Normal;
                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;
              }
            }
            else if (Equal(export.AdminAction.Item.DetailAdministrativeAction.
              Type1, "CRED") || Equal
              (export.AdminAction.Item.DetailAdministrativeAction.Type1, "FDSO") ||
              Equal
              (export.AdminAction.Item.DetailAdministrativeAction.Type1, "SDSO") ||
              Equal
              (export.AdminAction.Item.DetailAdministrativeAction.Type1, "KDWP") ||
              Equal
              (export.AdminAction.Item.DetailAdministrativeAction.Type1, "KDMV"))
              
            {
              // * *  AUTOMATIC Certification
              // * DO Display Current Amount
              var field1 =
                GetField(export.AdminAction.Item.
                  DetailAdministrativeActCertification, "currentAmount");

              field1.Color = "cyan";
              field1.Protected = true;

              // * DO NOT Display PROMPT to Flow to EXMP Screen
              var field2 =
                GetField(export.AdminAction.Item.FlowExmp, "selectChar");

              field2.Color = "";
              field2.Intensity = Intensity.Dark;
              field2.Highlighting = Highlighting.Normal;
              field2.Protected = true;

              if (!Equal(export.AdminAction.Item.
                DetailAdministrativeActCertification.Type1, "FDSO"))
              {
                // * *  AUTOMATIC  *NOT*  FDSO Certification
                // * Do NOT Display Non_ADC_Amount
                var field =
                  GetField(export.AdminAction.Item.DetailFederalDebtSetoff,
                  "nonAdcAmount");

                field.Intensity = Intensity.Dark;
                field.Protected = true;
              }
              else
              {
                // * *  AUTOMATIC FDSO Certification
                // * Do Display Non_ADC_Amount
                var field =
                  GetField(export.AdminAction.Item.DetailFederalDebtSetoff,
                  "nonAdcAmount");

                field.Color = "cyan";
                field.Protected = true;
              }
            }
            else
            {
              // * *  MANUAL Certification
              // * Do NOT Display Current Amount OR Non_ADC_Amount
              var field1 =
                GetField(export.AdminAction.Item.
                  DetailAdministrativeActCertification, "currentAmount");

              field1.Intensity = Intensity.Dark;
              field1.Protected = true;

              var field2 =
                GetField(export.AdminAction.Item.DetailFederalDebtSetoff,
                "nonAdcAmount");

              field2.Intensity = Intensity.Dark;
              field2.Protected = true;

              // * DO NOT  Display PROMPT to Flow to EXMP Screen
              var field3 =
                GetField(export.AdminAction.Item.FlowExmp, "selectChar");

              field3.Color = "";
              field3.Intensity = Intensity.Dark;
              field3.Highlighting = Highlighting.Normal;
              field3.Protected = true;
            }
          }

          export.AdminAction.CheckIndex();

          if (export.AdminAction.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
          }
          else if (export.AdminAction.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }

        export.HiddenPrev.Number = export.CsePersonsWorkSet.Number;

        break;
      case "RETURN":
        local.Selected.Count = 0;

        for(export.AdminAction.Index = 0; export.AdminAction.Index < export
          .AdminAction.Count; ++export.AdminAction.Index)
        {
          if (!export.AdminAction.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.AdminAction.Item.SelectOption.SelectChar) && AsChar
            (export.AdminAction.Item.SelectOption.SelectChar) != 'S')
          {
            var field =
              GetField(export.AdminAction.Item.SelectOption, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }

          if (AsChar(export.AdminAction.Item.SelectOption.SelectChar) == 'S')
          {
            ++local.Selected.Count;

            if (local.Selected.Count > 1)
            {
              var field =
                GetField(export.AdminAction.Item.SelectOption, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

              return;
            }

            export.SelectedCsePerson.Number =
              export.AdminAction.Item.DetailHiddenOblgr.Number;
            export.SelectedObligation.SystemGeneratedIdentifier =
              export.AdminAction.Item.DetailHidden.SystemGeneratedIdentifier;
            export.SelectedAdministrativeAction.Type1 =
              export.AdminAction.Item.DetailAdministrativeAction.Type1;
            MoveObligationType(export.AdminAction.Item.DetailObligationType,
              export.SelectedObligationType);

            if (AsChar(export.AdminAction.Item.DetailHiddenCrtfd.SelectChar) ==
              'Y')
            {
              export.SelectedAdministrativeActCertification.Assign(
                export.AdminAction.Item.DetailAdministrativeActCertification);
              local.CertifiedAction.Flag = "Y";
            }
            else
            {
              local.CertifiedAction.Flag = "N";
              export.SelectedObligationAdministrativeAction.TakenDate =
                export.AdminAction.Item.DetailAdministrativeActCertification.
                  TakenDate;
            }

            break;
          }
        }

        export.AdminAction.CheckIndex();

        if (AsChar(local.FlowToDetail.Flag) == 'Y')
        {
          if (local.Selected.Count != 1)
          {
            ExitState = "LE0000_ADMIN_ACT_MUST_BE_SEL";

            return;
          }

          if (AsChar(local.CertifiedAction.Flag) == 'Y')
          {
            // -------------------------------------------------------
            // Depending on the type, flow to different detail screens
            // -------------------------------------------------------
            UseLeSetExitstateForListObliga();
          }
          else
          {
            export.AllObligations.Flag = "N";
            ExitState = "ECO_LNK_TO_IADA_IDENTIFY_ADM_ACT";
          }

          return;
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
        }
        else
        {
          export.FlowKdmv.Number = export.CsePersonsWorkSet.Number;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "NO_MORE_ITEMS_TO_SCROLL";

        break;
      case "NEXT":
        ExitState = "NO_MORE_ITEMS_TO_SCROLL";

        break;
      case "DETAIL":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveAdminActions(LeListAdminActionsByObligor.Export.
    AdminActionsGroup source, Export.AdminActionGroup target)
  {
    target.SelectOption.SelectChar = source.SelectOption.SelectChar;
    MoveObligationType(source.ObligationType, target.DetailObligationType);
    target.DetailHiddenCrtfd.SelectChar = source.DetailCertified.SelectChar;
    target.DetailHiddenOblgr.Number = source.DetailObligor.Number;
    target.DetailHidden.SystemGeneratedIdentifier =
      source.DetailObligation.SystemGeneratedIdentifier;
    target.DetailAdministrativeAction.Type1 =
      source.DetailAdministrativeAction.Type1;
    target.DetailAdministrativeActCertification.Assign(
      source.DetailAdministrativeActCertification);
    MoveAdministrativeActCertification(source.DetailFederalDebtSetoff,
      target.DetailFederalDebtSetoff);
    target.DetailObligationAdministrativeAction.TakenDate =
      source.DetailObligationAdministrativeAction.TakenDate;
    target.SsnChangeFlag.Flag = source.SsnChangeFlag.Flag;
    target.FdsoDecertFlag.Flag = source.DecertFlag.Flag;
    target.ManAutoFlag.Flag = source.ManAutoSw.Flag;
    target.FlowExmp.SelectChar = source.SelManOption.SelectChar;
    target.Pass.Type1 = source.Pass.Type1;
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Percentage = source.Percentage;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveExport1ToMatchCsePersons(CabMatchCsePerson.Export.
    ExportGroup source, Local.MatchCsePersonsGroup target)
  {
    target.DetailMatchCsePer.Assign(source.Detail);
    target.DetailMatchAlt.Flag = source.Ae.Flag;
    target.MatchAe.Flag = source.Cse.Flag;
    target.MatchCse.Flag = source.Kanpay.Flag;
    target.MatchKanpay.Flag = source.Kscares.Flag;
    target.MatchKscares.Flag = source.Alt.Flag;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    MoveCommon(local.SearchOption, useImport.Search);
    MoveCsePersonsWorkSet3(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.
      CopyTo(local.MatchCsePersons, MoveExport1ToMatchCsePersons);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
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

  private void UseLeListAdminActionsByObligor()
  {
    var useImport = new LeListAdminActionsByObligor.Import();
    var useExport = new LeListAdminActionsByObligor.Export();

    useImport.ListOptManualAutoActs.OneChar =
      export.ListOptSystManualActs.OneChar;
    useImport.Required.Type1 = export.Required.Type1;
    useImport.StartDate.Date = export.StartDate.Date;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.SelectAll.Flag = export.ShowAll.Flag;

    Call(LeListAdminActionsByObligor.Execute, useImport, useExport);

    useExport.AdminActions.CopyTo(export.AdminAction, MoveAdminActions);
    export.One.Number = useExport.One.Number;
    export.Two.Number = useExport.Two.Number;
    export.Three.Number = useExport.Three.Number;
    export.Four.Number = useExport.Four.Number;
    export.Five.Number = useExport.Five.Number;
    export.Six.Number = useExport.Six.Number;
    export.Seven.Number = useExport.Seven.Number;
    export.Eight.Number = useExport.Eight.Number;
    export.Nine.Number = useExport.Nine.Number;
    export.Ten.Number = useExport.Ten.Number;
    export.Eleven.Number = useExport.Eleven.Number;
    export.Twelve.Number = useExport.Twelve.Number;
  }

  private void UseLeSetExitstateForListObliga()
  {
    var useImport = new LeSetExitstateForListObliga.Import();
    var useExport = new LeSetExitstateForListObliga.Export();

    useImport.AdministrativeActCertification.Type1 =
      export.SelectedAdministrativeActCertification.Type1;

    Call(LeSetExitstateForListObliga.Execute, useImport, useExport);

    local.ErrorSettingExitState.Flag = useExport.Error.Flag;
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

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
    /// <summary>A AdminActionGroup group.</summary>
    [Serializable]
    public class AdminActionGroup
    {
      /// <summary>
      /// A value of SelectOption.
      /// </summary>
      [JsonPropertyName("selectOption")]
      public Common SelectOption
      {
        get => selectOption ??= new();
        set => selectOption = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailHiddenCrtfd.
      /// </summary>
      [JsonPropertyName("detailHiddenCrtfd")]
      public Common DetailHiddenCrtfd
      {
        get => detailHiddenCrtfd ??= new();
        set => detailHiddenCrtfd = value;
      }

      /// <summary>
      /// A value of DetailHiddenOblgr.
      /// </summary>
      [JsonPropertyName("detailHiddenOblgr")]
      public CsePerson DetailHiddenOblgr
      {
        get => detailHiddenOblgr ??= new();
        set => detailHiddenOblgr = value;
      }

      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public Obligation DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of SsnChangeFlag.
      /// </summary>
      [JsonPropertyName("ssnChangeFlag")]
      public Common SsnChangeFlag
      {
        get => ssnChangeFlag ??= new();
        set => ssnChangeFlag = value;
      }

      /// <summary>
      /// A value of FdsoDecertFlag.
      /// </summary>
      [JsonPropertyName("fdsoDecertFlag")]
      public Common FdsoDecertFlag
      {
        get => fdsoDecertFlag ??= new();
        set => fdsoDecertFlag = value;
      }

      /// <summary>
      /// A value of ManAutoFlag.
      /// </summary>
      [JsonPropertyName("manAutoFlag")]
      public Common ManAutoFlag
      {
        get => manAutoFlag ??= new();
        set => manAutoFlag = value;
      }

      /// <summary>
      /// A value of FlowExmp.
      /// </summary>
      [JsonPropertyName("flowExmp")]
      public Common FlowExmp
      {
        get => flowExmp ??= new();
        set => flowExmp = value;
      }

      /// <summary>
      /// A value of Pass.
      /// </summary>
      [JsonPropertyName("pass")]
      public AdministrativeAction Pass
      {
        get => pass ??= new();
        set => pass = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common selectOption;
      private ObligationType detailObligationType;
      private Common detailHiddenCrtfd;
      private CsePerson detailHiddenOblgr;
      private Obligation detailHidden;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
      private Common ssnChangeFlag;
      private Common fdsoDecertFlag;
      private Common manAutoFlag;
      private Common flowExmp;
      private AdministrativeAction pass;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
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
    /// A value of ZdelImportRequiredServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelImportRequiredServiceProvider")]
    public ServiceProvider ZdelImportRequiredServiceProvider
    {
      get => zdelImportRequiredServiceProvider ??= new();
      set => zdelImportRequiredServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelImportRequiredOffice.
    /// </summary>
    [JsonPropertyName("zdelImportRequiredOffice")]
    public Office ZdelImportRequiredOffice
    {
      get => zdelImportRequiredOffice ??= new();
      set => zdelImportRequiredOffice = value;
    }

    /// <summary>
    /// A value of ListOptSystManualActs.
    /// </summary>
    [JsonPropertyName("listOptSystManualActs")]
    public Standard ListOptSystManualActs
    {
      get => listOptSystManualActs ??= new();
      set => listOptSystManualActs = value;
    }

    /// <summary>
    /// A value of PromptAdminActType.
    /// </summary>
    [JsonPropertyName("promptAdminActType")]
    public Standard PromptAdminActType
    {
      get => promptAdminActType ??= new();
      set => promptAdminActType = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
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
    /// Gets a value of AdminAction.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionGroup> AdminAction => adminAction ??= new(
      AdminActionGroup.Capacity);

    /// <summary>
    /// Gets a value of AdminAction for json serialization.
    /// </summary>
    [JsonPropertyName("adminAction")]
    [Computed]
    public IList<AdminActionGroup> AdminAction_Json
    {
      get => adminAction;
      set => AdminAction.Assign(value);
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of One.
    /// </summary>
    [JsonPropertyName("one")]
    public Case1 One
    {
      get => one ??= new();
      set => one = value;
    }

    /// <summary>
    /// A value of Two.
    /// </summary>
    [JsonPropertyName("two")]
    public Case1 Two
    {
      get => two ??= new();
      set => two = value;
    }

    /// <summary>
    /// A value of Three.
    /// </summary>
    [JsonPropertyName("three")]
    public Case1 Three
    {
      get => three ??= new();
      set => three = value;
    }

    /// <summary>
    /// A value of Four.
    /// </summary>
    [JsonPropertyName("four")]
    public Case1 Four
    {
      get => four ??= new();
      set => four = value;
    }

    /// <summary>
    /// A value of Five.
    /// </summary>
    [JsonPropertyName("five")]
    public Case1 Five
    {
      get => five ??= new();
      set => five = value;
    }

    /// <summary>
    /// A value of Six.
    /// </summary>
    [JsonPropertyName("six")]
    public Case1 Six
    {
      get => six ??= new();
      set => six = value;
    }

    /// <summary>
    /// A value of Seven.
    /// </summary>
    [JsonPropertyName("seven")]
    public Case1 Seven
    {
      get => seven ??= new();
      set => seven = value;
    }

    /// <summary>
    /// A value of Eight.
    /// </summary>
    [JsonPropertyName("eight")]
    public Case1 Eight
    {
      get => eight ??= new();
      set => eight = value;
    }

    /// <summary>
    /// A value of Nine.
    /// </summary>
    [JsonPropertyName("nine")]
    public Case1 Nine
    {
      get => nine ??= new();
      set => nine = value;
    }

    /// <summary>
    /// A value of Ten.
    /// </summary>
    [JsonPropertyName("ten")]
    public Case1 Ten
    {
      get => ten ??= new();
      set => ten = value;
    }

    /// <summary>
    /// A value of Eleven.
    /// </summary>
    [JsonPropertyName("eleven")]
    public Case1 Eleven
    {
      get => eleven ??= new();
      set => eleven = value;
    }

    /// <summary>
    /// A value of Twelve.
    /// </summary>
    [JsonPropertyName("twelve")]
    public Case1 Twelve
    {
      get => twelve ??= new();
      set => twelve = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    private Common showAll;
    private Standard standard;
    private ServiceProvider zdelImportRequiredServiceProvider;
    private Office zdelImportRequiredOffice;
    private Standard listOptSystManualActs;
    private Standard promptAdminActType;
    private DateWorkArea startDate;
    private AdministrativeAction required;
    private Case1 case1;
    private Standard listCsePersons;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<AdminActionGroup> adminAction;
    private SsnWorkArea ssnWorkArea;
    private Case1 one;
    private Case1 two;
    private Case1 three;
    private Case1 four;
    private Case1 five;
    private Case1 six;
    private Case1 seven;
    private Case1 eight;
    private Case1 nine;
    private Case1 ten;
    private Case1 eleven;
    private Case1 twelve;
    private CsePersonsWorkSet hiddenPrev;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AdminActionGroup group.</summary>
    [Serializable]
    public class AdminActionGroup
    {
      /// <summary>
      /// A value of SelectOption.
      /// </summary>
      [JsonPropertyName("selectOption")]
      public Common SelectOption
      {
        get => selectOption ??= new();
        set => selectOption = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailHiddenCrtfd.
      /// </summary>
      [JsonPropertyName("detailHiddenCrtfd")]
      public Common DetailHiddenCrtfd
      {
        get => detailHiddenCrtfd ??= new();
        set => detailHiddenCrtfd = value;
      }

      /// <summary>
      /// A value of DetailHiddenOblgr.
      /// </summary>
      [JsonPropertyName("detailHiddenOblgr")]
      public CsePerson DetailHiddenOblgr
      {
        get => detailHiddenOblgr ??= new();
        set => detailHiddenOblgr = value;
      }

      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public Obligation DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailAdministrativeAction")]
      public AdministrativeAction DetailAdministrativeAction
      {
        get => detailAdministrativeAction ??= new();
        set => detailAdministrativeAction = value;
      }

      /// <summary>
      /// A value of DetailAdministrativeActCertification.
      /// </summary>
      [JsonPropertyName("detailAdministrativeActCertification")]
      public AdministrativeActCertification DetailAdministrativeActCertification
      {
        get => detailAdministrativeActCertification ??= new();
        set => detailAdministrativeActCertification = value;
      }

      /// <summary>
      /// A value of DetailFederalDebtSetoff.
      /// </summary>
      [JsonPropertyName("detailFederalDebtSetoff")]
      public AdministrativeActCertification DetailFederalDebtSetoff
      {
        get => detailFederalDebtSetoff ??= new();
        set => detailFederalDebtSetoff = value;
      }

      /// <summary>
      /// A value of DetailObligationAdministrativeAction.
      /// </summary>
      [JsonPropertyName("detailObligationAdministrativeAction")]
      public ObligationAdministrativeAction DetailObligationAdministrativeAction
      {
        get => detailObligationAdministrativeAction ??= new();
        set => detailObligationAdministrativeAction = value;
      }

      /// <summary>
      /// A value of SsnChangeFlag.
      /// </summary>
      [JsonPropertyName("ssnChangeFlag")]
      public Common SsnChangeFlag
      {
        get => ssnChangeFlag ??= new();
        set => ssnChangeFlag = value;
      }

      /// <summary>
      /// A value of FdsoDecertFlag.
      /// </summary>
      [JsonPropertyName("fdsoDecertFlag")]
      public Common FdsoDecertFlag
      {
        get => fdsoDecertFlag ??= new();
        set => fdsoDecertFlag = value;
      }

      /// <summary>
      /// A value of ManAutoFlag.
      /// </summary>
      [JsonPropertyName("manAutoFlag")]
      public Common ManAutoFlag
      {
        get => manAutoFlag ??= new();
        set => manAutoFlag = value;
      }

      /// <summary>
      /// A value of FlowExmp.
      /// </summary>
      [JsonPropertyName("flowExmp")]
      public Common FlowExmp
      {
        get => flowExmp ??= new();
        set => flowExmp = value;
      }

      /// <summary>
      /// A value of Pass.
      /// </summary>
      [JsonPropertyName("pass")]
      public AdministrativeAction Pass
      {
        get => pass ??= new();
        set => pass = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common selectOption;
      private ObligationType detailObligationType;
      private Common detailHiddenCrtfd;
      private CsePerson detailHiddenOblgr;
      private Obligation detailHidden;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
      private Common ssnChangeFlag;
      private Common fdsoDecertFlag;
      private Common manAutoFlag;
      private Common flowExmp;
      private AdministrativeAction pass;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of SelectedObligationType.
    /// </summary>
    [JsonPropertyName("selectedObligationType")]
    public ObligationType SelectedObligationType
    {
      get => selectedObligationType ??= new();
      set => selectedObligationType = value;
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
    /// A value of AllObligations.
    /// </summary>
    [JsonPropertyName("allObligations")]
    public Common AllObligations
    {
      get => allObligations ??= new();
      set => allObligations = value;
    }

    /// <summary>
    /// A value of ListOptSystManualActs.
    /// </summary>
    [JsonPropertyName("listOptSystManualActs")]
    public Standard ListOptSystManualActs
    {
      get => listOptSystManualActs ??= new();
      set => listOptSystManualActs = value;
    }

    /// <summary>
    /// A value of PromptAdminActType.
    /// </summary>
    [JsonPropertyName("promptAdminActType")]
    public Standard PromptAdminActType
    {
      get => promptAdminActType ??= new();
      set => promptAdminActType = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public AdministrativeAction Required
    {
      get => required ??= new();
      set => required = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedAdministrativeAction.
    /// </summary>
    [JsonPropertyName("selectedAdministrativeAction")]
    public AdministrativeAction SelectedAdministrativeAction
    {
      get => selectedAdministrativeAction ??= new();
      set => selectedAdministrativeAction = value;
    }

    /// <summary>
    /// A value of SelectedObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("selectedObligationAdministrativeAction")]
    public ObligationAdministrativeAction SelectedObligationAdministrativeAction
    {
      get => selectedObligationAdministrativeAction ??= new();
      set => selectedObligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of SelectedAdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("selectedAdministrativeActCertification")]
    public AdministrativeActCertification SelectedAdministrativeActCertification
    {
      get => selectedAdministrativeActCertification ??= new();
      set => selectedAdministrativeActCertification = value;
    }

    /// <summary>
    /// A value of SelectedObligation.
    /// </summary>
    [JsonPropertyName("selectedObligation")]
    public Obligation SelectedObligation
    {
      get => selectedObligation ??= new();
      set => selectedObligation = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// Gets a value of AdminAction.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionGroup> AdminAction => adminAction ??= new(
      AdminActionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AdminAction for json serialization.
    /// </summary>
    [JsonPropertyName("adminAction")]
    [Computed]
    public IList<AdminActionGroup> AdminAction_Json
    {
      get => adminAction;
      set => AdminAction.Assign(value);
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of One.
    /// </summary>
    [JsonPropertyName("one")]
    public Case1 One
    {
      get => one ??= new();
      set => one = value;
    }

    /// <summary>
    /// A value of Two.
    /// </summary>
    [JsonPropertyName("two")]
    public Case1 Two
    {
      get => two ??= new();
      set => two = value;
    }

    /// <summary>
    /// A value of Three.
    /// </summary>
    [JsonPropertyName("three")]
    public Case1 Three
    {
      get => three ??= new();
      set => three = value;
    }

    /// <summary>
    /// A value of Four.
    /// </summary>
    [JsonPropertyName("four")]
    public Case1 Four
    {
      get => four ??= new();
      set => four = value;
    }

    /// <summary>
    /// A value of Five.
    /// </summary>
    [JsonPropertyName("five")]
    public Case1 Five
    {
      get => five ??= new();
      set => five = value;
    }

    /// <summary>
    /// A value of Six.
    /// </summary>
    [JsonPropertyName("six")]
    public Case1 Six
    {
      get => six ??= new();
      set => six = value;
    }

    /// <summary>
    /// A value of Seven.
    /// </summary>
    [JsonPropertyName("seven")]
    public Case1 Seven
    {
      get => seven ??= new();
      set => seven = value;
    }

    /// <summary>
    /// A value of Eight.
    /// </summary>
    [JsonPropertyName("eight")]
    public Case1 Eight
    {
      get => eight ??= new();
      set => eight = value;
    }

    /// <summary>
    /// A value of Nine.
    /// </summary>
    [JsonPropertyName("nine")]
    public Case1 Nine
    {
      get => nine ??= new();
      set => nine = value;
    }

    /// <summary>
    /// A value of Ten.
    /// </summary>
    [JsonPropertyName("ten")]
    public Case1 Ten
    {
      get => ten ??= new();
      set => ten = value;
    }

    /// <summary>
    /// A value of Eleven.
    /// </summary>
    [JsonPropertyName("eleven")]
    public Case1 Eleven
    {
      get => eleven ??= new();
      set => eleven = value;
    }

    /// <summary>
    /// A value of Twelve.
    /// </summary>
    [JsonPropertyName("twelve")]
    public Case1 Twelve
    {
      get => twelve ??= new();
      set => twelve = value;
    }

    /// <summary>
    /// A value of FlowKdmv.
    /// </summary>
    [JsonPropertyName("flowKdmv")]
    public CsePersonsWorkSet FlowKdmv
    {
      get => flowKdmv ??= new();
      set => flowKdmv = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    private Common showAll;
    private ObligationType selectedObligationType;
    private Standard standard;
    private Common allObligations;
    private Standard listOptSystManualActs;
    private Standard promptAdminActType;
    private DateWorkArea startDate;
    private AdministrativeAction required;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction selectedAdministrativeAction;
    private ObligationAdministrativeAction selectedObligationAdministrativeAction;
      
    private AdministrativeActCertification selectedAdministrativeActCertification;
      
    private Obligation selectedObligation;
    private CsePerson selectedCsePerson;
    private Array<AdminActionGroup> adminAction;
    private SsnWorkArea ssnWorkArea;
    private Case1 one;
    private Case1 two;
    private Case1 three;
    private Case1 four;
    private Case1 five;
    private Case1 six;
    private Case1 seven;
    private Case1 eight;
    private Case1 nine;
    private Case1 ten;
    private Case1 eleven;
    private Case1 twelve;
    private CsePersonsWorkSet flowKdmv;
    private CsePersonsWorkSet hiddenPrev;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A MatchCsePersonsGroup group.</summary>
    [Serializable]
    public class MatchCsePersonsGroup
    {
      /// <summary>
      /// A value of DetailMatchCsePer.
      /// </summary>
      [JsonPropertyName("detailMatchCsePer")]
      public CsePersonsWorkSet DetailMatchCsePer
      {
        get => detailMatchCsePer ??= new();
        set => detailMatchCsePer = value;
      }

      /// <summary>
      /// A value of DetailMatchAlt.
      /// </summary>
      [JsonPropertyName("detailMatchAlt")]
      public Common DetailMatchAlt
      {
        get => detailMatchAlt ??= new();
        set => detailMatchAlt = value;
      }

      /// <summary>
      /// A value of MatchAe.
      /// </summary>
      [JsonPropertyName("matchAe")]
      public Common MatchAe
      {
        get => matchAe ??= new();
        set => matchAe = value;
      }

      /// <summary>
      /// A value of MatchCse.
      /// </summary>
      [JsonPropertyName("matchCse")]
      public Common MatchCse
      {
        get => matchCse ??= new();
        set => matchCse = value;
      }

      /// <summary>
      /// A value of MatchKanpay.
      /// </summary>
      [JsonPropertyName("matchKanpay")]
      public Common MatchKanpay
      {
        get => matchKanpay ??= new();
        set => matchKanpay = value;
      }

      /// <summary>
      /// A value of MatchKscares.
      /// </summary>
      [JsonPropertyName("matchKscares")]
      public Common MatchKscares
      {
        get => matchKscares ??= new();
        set => matchKscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detailMatchCsePer;
      private Common detailMatchAlt;
      private Common matchAe;
      private Common matchCse;
      private Common matchKanpay;
      private Common matchKscares;
    }

    /// <summary>
    /// A value of BlankStartDate.
    /// </summary>
    [JsonPropertyName("blankStartDate")]
    public DateWorkArea BlankStartDate
    {
      get => blankStartDate ??= new();
      set => blankStartDate = value;
    }

    /// <summary>
    /// A value of CountExmp.
    /// </summary>
    [JsonPropertyName("countExmp")]
    public Common CountExmp
    {
      get => countExmp ??= new();
      set => countExmp = value;
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
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public CsePersonsWorkSet Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public CsePersonsWorkSet InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
    }

    /// <summary>
    /// Gets a value of MatchCsePersons.
    /// </summary>
    [JsonIgnore]
    public Array<MatchCsePersonsGroup> MatchCsePersons =>
      matchCsePersons ??= new(MatchCsePersonsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MatchCsePersons for json serialization.
    /// </summary>
    [JsonPropertyName("matchCsePersons")]
    [Computed]
    public IList<MatchCsePersonsGroup> MatchCsePersons_Json
    {
      get => matchCsePersons;
      set => MatchCsePersons.Assign(value);
    }

    /// <summary>
    /// A value of SearchOption.
    /// </summary>
    [JsonPropertyName("searchOption")]
    public Common SearchOption
    {
      get => searchOption ??= new();
      set => searchOption = value;
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
    /// A value of XxxLocalTotalNoOfEntries.
    /// </summary>
    [JsonPropertyName("xxxLocalTotalNoOfEntries")]
    public Common XxxLocalTotalNoOfEntries
    {
      get => xxxLocalTotalNoOfEntries ??= new();
      set => xxxLocalTotalNoOfEntries = value;
    }

    /// <summary>
    /// A value of ErrorSettingExitState.
    /// </summary>
    [JsonPropertyName("errorSettingExitState")]
    public Common ErrorSettingExitState
    {
      get => errorSettingExitState ??= new();
      set => errorSettingExitState = value;
    }

    /// <summary>
    /// A value of CertifiedAction.
    /// </summary>
    [JsonPropertyName("certifiedAction")]
    public Common CertifiedAction
    {
      get => certifiedAction ??= new();
      set => certifiedAction = value;
    }

    /// <summary>
    /// A value of FlowToDetail.
    /// </summary>
    [JsonPropertyName("flowToDetail")]
    public Common FlowToDetail
    {
      get => flowToDetail ??= new();
      set => flowToDetail = value;
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
    /// A value of GroupEntryNo.
    /// </summary>
    [JsonPropertyName("groupEntryNo")]
    public Common GroupEntryNo
    {
      get => groupEntryNo ??= new();
      set => groupEntryNo = value;
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

    private DateWorkArea blankStartDate;
    private Common countExmp;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet saved;
    private CsePersonsWorkSet initialisedToSpaces;
    private Array<MatchCsePersonsGroup> matchCsePersons;
    private Common searchOption;
    private CsePerson csePerson;
    private Common xxxLocalTotalNoOfEntries;
    private Common errorSettingExitState;
    private Common certifiedAction;
    private Common flowToDetail;
    private Common selected;
    private Common groupEntryNo;
    private NextTranInfo nextTranInfo;
  }
#endregion
}

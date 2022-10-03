// Program: LE_OBLG_ADMIN_ACTIONS_BY_OBLIGN, ID: 372599647, model: 746.
// Short name: SWEOBLGP
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
/// A program: LE_OBLG_ADMIN_ACTIONS_BY_OBLIGN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step action block lists the administrative action taken by court 
/// case.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeOblgAdminActionsByOblign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_OBLG_ADMIN_ACTIONS_BY_OBLIGN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeOblgAdminActionsByOblign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeOblgAdminActionsByOblign.
  /// </summary>
  public LeOblgAdminActionsByOblign(IContext context, Import import,
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
    //   Date		Developer	Request #	Description
    // 09-13-95        Govind			Initial development
    // 10/01/98     P. Sharp     Removed views code for CSE_person prompt. 
    // Removed dialog flow for COAG.  Added exit states after display action
    // block
    // 10/20/99	R.Jean	PR74995 - Move the taken date to the curr amt date (
    // displayed on screen) if the type is not FDSO, SDSO, CRED.
    // 10/10/02        K.Doshi          Fix screen help Id.
    // ******** END MAINTENANCE LOG **************************
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
      // **** begin group F ****
      UseScCabSignoff();

      return;

      // **** end   group F ****
    }

    export.Case1.Number = import.Case1.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    export.Obligation.Assign(import.Obligation);
    export.ObligationType.Assign(import.ObligationType);
    export.ListObligations.PromptField = import.ListObligations.PromptField;
    export.Required.Type1 = import.Required.Type1;
    export.StartDate.Date = import.StartDate.Date;
    export.PromptAdminActType.PromptField =
      import.PromptAdminActType.PromptField;
    export.ListOptAutoManualActs.OneChar = import.ListOptAutoManualActs.OneChar;
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    switch(AsChar(export.ListOptAutoManualActs.OneChar))
    {
      case 'M':
        break;
      case 'A':
        break;
      case ' ':
        break;
      default:
        var field = GetField(export.ListOptAutoManualActs, "oneChar");

        field.Error = true;

        ExitState = "LE0000_INVALID_OPT_MANUAL_AUTO";

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
          export.AdminAction.Update.DetailAdministrativeActCertification.Assign(
            import.AdminAction.Item.DetailAdministrativeActCertification);
          export.AdminAction.Update.DetailAdministrativeAction.Type1 =
            import.AdminAction.Item.DetailAdministrativeAction.Type1;
          export.AdminAction.Update.DetailObligationAdministrativeAction.
            TakenDate =
              import.AdminAction.Item.DetailObligationAdministrativeAction.
              TakenDate;
          MoveAdministrativeActCertification(import.AdminAction.Item.
            DetailFederalDebtSetoff,
            export.AdminAction.Update.DetailFederalDebtSetoff);
        }
      }

      if (!import.SelOpt.IsEmpty)
      {
        export.SelOpt.Index = -1;

        for(import.SelOpt.Index = 0; import.SelOpt.Index < import.SelOpt.Count; ++
          import.SelOpt.Index)
        {
          ++export.SelOpt.Index;
          export.SelOpt.CheckSize();

          export.SelOpt.Update.DetailSelOpt.SelectChar =
            import.SelOpt.Item.DetailSelOpt.SelectChar;
        }
      }
    }

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

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      }

      export.Obligation.SystemGeneratedIdentifier =
        local.NextTranInfo.ObligationId.GetValueOrDefault();

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      local.NextTranInfo.ObligationId =
        export.Obligation.SystemGeneratedIdentifier;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "RETOPAY") || Equal(global.Command, "RETADAA"))
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
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETOPAY") && !
      Equal(global.Command, "RETADAA"))
    {
      export.ListObligations.PromptField = "";
      export.PromptAdminActType.PromptField = "";
    }

    if (Equal(global.Command, "RETOPAY"))
    {
      global.Command = "DISPLAY";
      export.ListObligations.PromptField = "";
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

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        break;
      case "SIGNOFF":
        break;
      case "RETADAA":
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

        break;
      case "LIST":
        if (!IsEmpty(export.PromptAdminActType.PromptField) && AsChar
          (export.PromptAdminActType.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        if (!IsEmpty(export.ListObligations.PromptField) && AsChar
          (export.ListObligations.PromptField) != 'S')
        {
          var field = GetField(export.ListObligations, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        if (AsChar(export.PromptAdminActType.PromptField) == 'S' && AsChar
          (export.ListObligations.PromptField) == 'S')
        {
          var field1 = GetField(export.ListObligations, "promptField");

          field1.Error = true;

          var field2 = GetField(export.PromptAdminActType, "promptField");

          field2.Error = true;

          ExitState = "ZD_ACO_NE0_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.PromptAdminActType.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";

          return;
        }

        if (AsChar(export.ListObligations.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_LST_OPAY_OBLG_BY_AP";

          return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "DISPLAY":
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          if (IsEmpty(export.CsePersonsWorkSet.Ssn))
          {
            MoveCsePersonsWorkSet3(local.InitialisedToSpaces,
              export.CsePersonsWorkSet);

            var field1 = GetField(export.CsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

            field2.Error = true;

            var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

            field3.Error = true;

            var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

            field4.Error = true;

            ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

            return;
          }
          else
          {
            local.SearchOption.Flag = "1";
            UseCabMatchCsePerson();

            local.MatchCsePersons.Index = 0;
            local.MatchCsePersons.CheckSize();

            MoveCsePersonsWorkSet1(local.MatchCsePersons.Item.DetailMatchCsePer,
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

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ---------------------------------------------
            // CSE person not found. So move the import value back
            // ---------------------------------------------
            export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
            MoveCsePersonsWorkSet3(local.InitialisedToSpaces,
              export.CsePersonsWorkSet);

            if (!IsEmpty(export.CsePersonsWorkSet.Number))
            {
              export.CsePersonsWorkSet.Ssn = "";
            }

            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
        }

        local.CsePerson.Number = export.CsePersonsWorkSet.Number;

        if (export.Obligation.SystemGeneratedIdentifier == 0 || export
          .ObligationType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ECO_LNK_LST_OPAY_OBLG_BY_AP";

          return;
        }

        UseLeListAdminActionsByOblign();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.AdminAction.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
          }
          else
          {
            // ****************************************************************
            // 10/20/99	R.Jean	PR74995 - Move the taken date to the curr amt 
            // date (displayed on screen) if the type is not FDSO, SDSO, CRED.
            // ****************************************************************
            export.AdminAction.Index = 0;

            for(var limit = local.TotalNoOfEntries.Count; export
              .AdminAction.Index < limit; ++export.AdminAction.Index)
            {
              if (!export.AdminAction.CheckSize())
              {
                break;
              }

              export.SelOpt.Index = export.AdminAction.Index;
              export.SelOpt.CheckSize();

              export.SelOpt.Update.DetailSelOpt.SelectChar = "";

              if (!Equal(export.AdminAction.Item.
                DetailAdministrativeActCertification.Type1, "FDSO") && !
                Equal(export.AdminAction.Item.
                  DetailAdministrativeActCertification.Type1, "SDSO") && !
                Equal(export.AdminAction.Item.
                  DetailAdministrativeActCertification.Type1, "CRED"))
              {
                export.AdminAction.Update.DetailAdministrativeActCertification.
                  CurrentAmountDate =
                    export.AdminAction.Item.
                    DetailAdministrativeActCertification.TakenDate;
              }
            }

            export.AdminAction.CheckIndex();
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          return;
        }
        else if (IsExitState("LE0000_INVALID_ADMIN_ACT_TYPE"))
        {
          var field = GetField(export.Required, "type1");

          field.Error = true;
        }
        else if (IsExitState("LE0000_LIST_NOT_MATCH_TYPE"))
        {
          var field1 = GetField(export.Required, "type1");

          field1.Error = true;

          var field2 = GetField(export.ListOptAutoManualActs, "oneChar");

          field2.Error = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;
        }
        else if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;
        }
        else if (IsExitState("FN0000_OBLIGATION_NF"))
        {
          var field = GetField(export.Obligation, "systemGeneratedIdentifier");

          field.Error = true;
        }
        else
        {
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }

        break;
      case "RETURN":
        local.Selected.Count = 0;

        for(export.SelOpt.Index = 0; export.SelOpt.Index < export.SelOpt.Count; ++
          export.SelOpt.Index)
        {
          if (!export.SelOpt.CheckSize())
          {
            break;
          }

          export.AdminAction.Index = export.SelOpt.Index;
          export.AdminAction.CheckSize();

          if (!IsEmpty(export.SelOpt.Item.DetailSelOpt.SelectChar) && AsChar
            (export.SelOpt.Item.DetailSelOpt.SelectChar) != 'S')
          {
            var field = GetField(export.SelOpt.Item.DetailSelOpt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
          }

          if (AsChar(export.SelOpt.Item.DetailSelOpt.SelectChar) == 'S')
          {
            ++local.Selected.Count;

            if (local.Selected.Count > 1)
            {
              var field =
                GetField(export.SelOpt.Item.DetailSelOpt, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

              return;
            }

            export.SelectedCsePerson.Number =
              export.AdminAction.Item.DetailHiddenOblgr.Number;
            export.SelectedObligation.SystemGeneratedIdentifier =
              export.Obligation.SystemGeneratedIdentifier;
            export.SelectedAdministrativeAction.Type1 =
              export.AdminAction.Item.DetailAdministrativeAction.Type1;

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

        export.SelOpt.CheckIndex();

        if (AsChar(local.FlowToDetail.Flag) == 'Y')
        {
          if (local.Selected.Count != 1)
          {
            ExitState = "LE0000_ADMIN_ACT_MUST_BE_SEL";

            return;
          }

          if (AsChar(local.CertifiedAction.Flag) == 'Y')
          {
            // ---------------------------------------------
            // Depending on the type, flow to different detail screens.
            // ---------------------------------------------
            UseLeSetExitstateForListObliga();
          }
          else
          {
            export.DlgflwListAllObligatns.Flag = "N";
            ExitState = "ECO_LNK_TO_IADA_IDENTIFY_ADM_ACT";
          }

          return;
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

  private static void MoveAdminActions(LeListAdminActionsByOblign.Export.
    AdminActionsGroup source, Export.AdminActionGroup target)
  {
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
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
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
    target.Code = source.Code;
    target.Name = source.Name;
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
    MoveCsePersonsWorkSet2(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

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

  private void UseLeListAdminActionsByOblign()
  {
    var useImport = new LeListAdminActionsByOblign.Import();
    var useExport = new LeListAdminActionsByOblign.Export();

    useImport.ListOptAutoManualActs.OneChar =
      export.ListOptAutoManualActs.OneChar;
    useImport.Required.Type1 = import.Required.Type1;
    useImport.StartDate.Date = import.StartDate.Date;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(LeListAdminActionsByOblign.Execute, useImport, useExport);

    MoveObligationType(useExport.ObligationType, export.ObligationType);
    export.Obligation.Assign(useExport.Obligation);
    local.TotalNoOfEntries.Count = useExport.TotalNoOfEntries.Count;
    useExport.AdminActions.CopyTo(export.AdminAction, MoveAdminActions);
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
    /// <summary>A SelOptGroup group.</summary>
    [Serializable]
    public class SelOptGroup
    {
      /// <summary>
      /// A value of DetailSelOpt.
      /// </summary>
      [JsonPropertyName("detailSelOpt")]
      public Common DetailSelOpt
      {
        get => detailSelOpt ??= new();
        set => detailSelOpt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelOpt;
    }

    /// <summary>A AdminActionGroup group.</summary>
    [Serializable]
    public class AdminActionGroup
    {
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailHiddenCrtfd;
      private CsePerson detailHiddenOblgr;
      private Obligation detailHidden;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ListOptAutoManualActs.
    /// </summary>
    [JsonPropertyName("listOptAutoManualActs")]
    public Standard ListOptAutoManualActs
    {
      get => listOptAutoManualActs ??= new();
      set => listOptAutoManualActs = value;
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
    /// A value of ListObligations.
    /// </summary>
    [JsonPropertyName("listObligations")]
    public Standard ListObligations
    {
      get => listObligations ??= new();
      set => listObligations = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// Gets a value of SelOpt.
    /// </summary>
    [JsonIgnore]
    public Array<SelOptGroup> SelOpt => selOpt ??= new(SelOptGroup.Capacity);

    /// <summary>
    /// Gets a value of SelOpt for json serialization.
    /// </summary>
    [JsonPropertyName("selOpt")]
    [Computed]
    public IList<SelOptGroup> SelOpt_Json
    {
      get => selOpt;
      set => SelOpt.Assign(value);
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
    /// A value of XxxImportHidden.
    /// </summary>
    [JsonPropertyName("xxxImportHidden")]
    public Security2 XxxImportHidden
    {
      get => xxxImportHidden ??= new();
      set => xxxImportHidden = value;
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

    private Standard standard;
    private ObligationType obligationType;
    private Standard listOptAutoManualActs;
    private Standard promptAdminActType;
    private DateWorkArea startDate;
    private AdministrativeAction required;
    private Standard listObligations;
    private Obligation obligation;
    private Case1 case1;
    private Standard listCsePersons;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<SelOptGroup> selOpt;
    private Array<AdminActionGroup> adminAction;
    private Security2 xxxImportHidden;
    private SsnWorkArea ssnWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SelOptGroup group.</summary>
    [Serializable]
    public class SelOptGroup
    {
      /// <summary>
      /// A value of DetailSelOpt.
      /// </summary>
      [JsonPropertyName("detailSelOpt")]
      public Common DetailSelOpt
      {
        get => detailSelOpt ??= new();
        set => detailSelOpt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailSelOpt;
    }

    /// <summary>A AdminActionGroup group.</summary>
    [Serializable]
    public class AdminActionGroup
    {
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailHiddenCrtfd;
      private CsePerson detailHiddenOblgr;
      private Obligation detailHidden;
      private AdministrativeAction detailAdministrativeAction;
      private AdministrativeActCertification detailAdministrativeActCertification;
        
      private AdministrativeActCertification detailFederalDebtSetoff;
      private ObligationAdministrativeAction detailObligationAdministrativeAction;
        
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of DlgflwListAllObligatns.
    /// </summary>
    [JsonPropertyName("dlgflwListAllObligatns")]
    public Common DlgflwListAllObligatns
    {
      get => dlgflwListAllObligatns ??= new();
      set => dlgflwListAllObligatns = value;
    }

    /// <summary>
    /// A value of ListOptAutoManualActs.
    /// </summary>
    [JsonPropertyName("listOptAutoManualActs")]
    public Standard ListOptAutoManualActs
    {
      get => listOptAutoManualActs ??= new();
      set => listOptAutoManualActs = value;
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
    /// A value of ListObligations.
    /// </summary>
    [JsonPropertyName("listObligations")]
    public Standard ListObligations
    {
      get => listObligations ??= new();
      set => listObligations = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// Gets a value of SelOpt.
    /// </summary>
    [JsonIgnore]
    public Array<SelOptGroup> SelOpt => selOpt ??= new(SelOptGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SelOpt for json serialization.
    /// </summary>
    [JsonPropertyName("selOpt")]
    [Computed]
    public IList<SelOptGroup> SelOpt_Json
    {
      get => selOpt;
      set => SelOpt.Assign(value);
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
    /// A value of XxxExportHidden.
    /// </summary>
    [JsonPropertyName("xxxExportHidden")]
    public Security2 XxxExportHidden
    {
      get => xxxExportHidden ??= new();
      set => xxxExportHidden = value;
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

    private Standard standard;
    private ObligationType obligationType;
    private Common dlgflwListAllObligatns;
    private Standard listOptAutoManualActs;
    private Standard promptAdminActType;
    private DateWorkArea startDate;
    private AdministrativeAction required;
    private Standard listObligations;
    private Obligation obligation;
    private Case1 case1;
    private Standard listCsePersons;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction selectedAdministrativeAction;
    private ObligationAdministrativeAction selectedObligationAdministrativeAction;
      
    private AdministrativeActCertification selectedAdministrativeActCertification;
      
    private Obligation selectedObligation;
    private CsePerson selectedCsePerson;
    private Array<SelOptGroup> selOpt;
    private Array<AdminActionGroup> adminAction;
    private Security2 xxxExportHidden;
    private SsnWorkArea ssnWorkArea;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of TotalNoOfEntries.
    /// </summary>
    [JsonPropertyName("totalNoOfEntries")]
    public Common TotalNoOfEntries
    {
      get => totalNoOfEntries ??= new();
      set => totalNoOfEntries = value;
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
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public CsePersonsWorkSet InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
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

    private TextWorkArea textWorkArea;
    private CsePerson csePerson;
    private Common totalNoOfEntries;
    private Common errorSettingExitState;
    private Common certifiedAction;
    private Common flowToDetail;
    private Common selected;
    private Common groupEntryNo;
    private Array<MatchCsePersonsGroup> matchCsePersons;
    private Common searchOption;
    private CsePersonsWorkSet initialisedToSpaces;
    private NextTranInfo nextTranInfo;
  }
#endregion
}

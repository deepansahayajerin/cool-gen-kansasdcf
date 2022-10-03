// Program: LE_EXMP_ADMIN_ACTION_EXEMPTION, ID: 372589520, model: 746.
// Short name: SWEEXMPP
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
/// A program: LE_EXMP_ADMIN_ACTION_EXEMPTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeExmpAdminActionExemption: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EXMP_ADMIN_ACTION_EXEMPTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeExmpAdminActionExemption(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeExmpAdminActionExemption.
  /// </summary>
  public LeExmpAdminActionExemption(IContext context, Import import,
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
    // 09-27-95        Govind				Initial development
    // 09/09/97        Alan Samuels			Problem Report 27369
    // 10/20/98     P. Sharp     Fixed problem report 38573.  Made changes per 
    // Phase II assessment.
    // *******************************************************************
    // 9/9/16       JHarden     CQ 50347   Bring back State & County from OPAY
    // 05/16/18     JHarden     CQ 62179  Change to how State/County Display on 
    // EXMP
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

    local.HighlightError.Flag = "Y";
    local.Rollback.Flag = "N";

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    export.LegalAction.Assign(import.LegalAction);
    export.ListTribunals.PromptField = import.ListTribunals.PromptField;
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.Foreign.Country = export.Foreign.Country;
    MoveCsePersonsWorkSet3(import.CsePersonsWorkSet, export.CsePersonsWorkSet);

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    MoveAdministrativeAction(import.AdministrativeAction,
      export.AdministrativeAction);
    export.PromptAdminActionType.SelectChar =
      import.PromptAdminActionType.SelectChar;
    export.AllObligations.Flag = import.AllObligations.Flag;
    MoveObligation(import.Obligation, export.Obligation);
    export.ObligationAdmActionExemption.Assign(
      import.ObligationAdmActionExemption);
    export.PromptObligationNbr.SelectChar =
      import.PromptObligationNbr.SelectChar;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.CreatedBy.Assign(import.CreatedBy);
    export.UpdatedBy.Assign(import.UpdatedBy);
    export.ObligationType.Assign(import.ObligationType);
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    export.HiddenPrevLegalAction.Assign(import.HiddenPrevLegalAction);
    MoveCsePersonsWorkSet1(import.HiddenPrevCsePersonsWorkSet,
      export.HiddenPrevCsePersonsWorkSet);
    MoveObligation(import.HiddenPrevObligation, export.HiddenPrevObligation);
    export.HiddenPrevObligationAdmActionExemption.Assign(
      import.HiddenPrevObligationAdmActionExemption);
    MoveAdministrativeAction(import.HiddenPrevAdministrativeAction,
      export.HiddenPrevAdministrativeAction);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    MoveFips(import.HiddenFips, export.HiddenFips);

    if (IsEmpty(export.AllObligations.Flag))
    {
      export.AllObligations.Flag = "N";
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

      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";
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
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      local.NextTranInfo.ObligationId =
        export.Obligation.SystemGeneratedIdentifier;
      UseScCabNextTranPut();

      return;
    }

    // *** Check security only if CRUD action is being performed.
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
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
    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL") && !
      Equal(global.Command, "RETOPAY") && !Equal(global.Command, "RETLTRB") && !
      Equal(global.Command, "RETADAA"))
    {
      export.PromptAdminActionType.SelectChar = "";
      export.PromptObligationNbr.SelectChar = "";
      export.ListTribunals.PromptField = "";
    }

    if (Equal(global.Command, "RETOPAY"))
    {
      global.Command = "DISPLAY";
      export.PromptObligationNbr.SelectChar = "";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        if (IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          ExitState = "LE0000_CSE_PERSON_NO_OR_SSN_REQD";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.ObligationAdmActionExemption.Assign(
            local.InitialisedObligationAdmActionExemption);
          MoveAdministrativeAction(local.InitialisedAdministrativeAction,
            export.AdministrativeAction);
          MoveObligation(local.InitialisedObligation, export.Obligation);

          return;
        }
        else
        {
          local.SearchPerson.Flag = "1";
          UseCabMatchCsePerson();

          local.Local1.Index = 0;
          local.Local1.CheckSize();

          MoveCsePersonsWorkSet2(local.Local1.Item.Deatil,
            export.CsePersonsWorkSet);
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
        MoveCsePersonsWorkSet3(import.CsePersonsWorkSet,
          export.CsePersonsWorkSet);
        export.CsePersonsWorkSet.FormattedName = "";
        ExitState = "CSE_PERSON_NF";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        if (Equal(global.Command, "DISPLAY"))
        {
          export.ObligationAdmActionExemption.Assign(
            local.InitialisedObligationAdmActionExemption);
          MoveAdministrativeAction(local.InitialisedAdministrativeAction,
            export.AdministrativeAction);
          MoveObligation(local.InitialisedObligation, export.Obligation);
          export.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;
          export.Obligation.SystemGeneratedIdentifier =
            import.Obligation.SystemGeneratedIdentifier;
          export.ObligationAdmActionExemption.EffectiveDate =
            import.ObligationAdmActionExemption.EffectiveDate;
          export.HiddenPrevObligationAdmActionExemption.Assign(
            export.ObligationAdmActionExemption);
          MoveAdministrativeAction(export.AdministrativeAction,
            export.HiddenPrevAdministrativeAction);
          MoveObligation(export.Obligation, export.HiddenPrevObligation);
        }

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      if (export.LegalAction.Identifier != 0)
      {
        // CQ 62179
        if (ReadLegalAction2())
        {
          export.LegalAction.Assign(entities.ExistingLegalAction);
        }
        else
        {
          ExitState = "FN0000_LEGAL_ACTION_NOT_VALID";

          var field = GetField(export.LegalAction, "identifier");

          field.Error = true;

          return;
        }

        if (ReadTribunal())
        {
          export.Tribunal.Assign(entities.ExistingTribunal);
        }
        else
        {
          ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

          var field = GetField(export.LegalAction, "identifier");

          field.Error = true;

          return;
        }

        if (ReadFips3())
        {
          export.Tribunal.Assign(entities.ExistingTribunal);
          export.Fips.Assign(entities.ExistingFips);
        }
        else if (ReadFipsTribAddress())
        {
          export.Tribunal.Assign(entities.ExistingTribunal);
        }
        else
        {
          ExitState = "FN0000_NO_FIPS_ADDRESS_FOUND";

          var field = GetField(export.FipsTribAddress, "identifier");

          field.Error = true;

          return;
        }
      }
      else if (!IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          if (export.Tribunal.Identifier == 0)
          {
            ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

            var field = GetField(export.ListTribunals, "promptField");

            field.Error = true;

            return;
          }
          else if (ReadLegalAction1())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
          }
          else
          {
            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            return;
          }
        }
        else
        {
          if (IsEmpty(export.Fips.CountyAbbreviation))
          {
            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;

            ExitState = "LE0000_TRIB_COUNTY_REQUIRED";

            return;
          }

          if (ReadLegalActionTribunalFips())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
            export.Tribunal.Assign(entities.ExistingTribunal);
            export.Fips.Assign(entities.ExistingFips);
          }
          else
          {
            if (!ReadFips1())
            {
              if (ReadFips2())
              {
                var field = GetField(export.Fips, "countyAbbreviation");

                field.Error = true;

                ExitState = "INVALID_COUNTY_CODE";

                return;
              }
              else
              {
                var field = GetField(export.Fips, "stateAbbreviation");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_STATE_CODE";

                return;
              }
            }

            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.Fips, "stateAbbreviation");

            field2.Error = true;

            var field3 = GetField(export.Fips, "countyAbbreviation");

            field3.Error = true;

            return;
          }
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        break;
      case "RETURN":
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
        }
        else
        {
          export.FlowKdmv.Number = export.HiddenPrevCsePersonsWorkSet.Number;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "RETLTRB":
        export.ListTribunals.PromptField = "";

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Protected = false;
          field.Focused = true;
        }
        else if (IsEmpty(entities.ExistingFips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "DISPLAY":
        if (AsChar(export.AllObligations.Flag) == 'Y')
        {
          local.DisplayAnyObligation.Flag = "Y";
        }
        else
        {
          local.DisplayAnyObligation.Flag = "N";
        }

        local.CsePerson.Number = export.CsePersonsWorkSet.Number;

        if (!IsEmpty(import.HiddenPrevAdministrativeAction.Type1) || import
          .HiddenPrevObligation.SystemGeneratedIdentifier > 0 || !
          IsEmpty(import.HiddenPrevCsePersonsWorkSet.Number) || !
          IsEmpty(import.HiddenPrevCsePersonsWorkSet.Ssn) || !
          IsEmpty(import.HiddenPrevLegalAction.CourtCaseNumber) || !
          IsEmpty(import.HiddenFips.StateAbbreviation) || !
          IsEmpty(import.HiddenFips.CountyAbbreviation))
        {
          if (!Equal(export.AdministrativeAction.Type1,
            import.HiddenPrevAdministrativeAction.Type1) || export
            .Obligation.SystemGeneratedIdentifier != import
            .HiddenPrevObligation.SystemGeneratedIdentifier || !
            Equal(export.CsePersonsWorkSet.Number,
            import.HiddenPrevCsePersonsWorkSet.Number) || !
            Equal(export.CsePersonsWorkSet.Ssn,
            import.HiddenPrevCsePersonsWorkSet.Ssn) || !
            Equal(export.LegalAction.CourtCaseNumber,
            import.HiddenPrevLegalAction.CourtCaseNumber) || !
            Equal(export.Fips.StateAbbreviation,
            import.HiddenFips.StateAbbreviation) || !
            Equal(export.Fips.CountyAbbreviation,
            import.HiddenFips.CountyAbbreviation))
          {
            export.ObligationAdmActionExemption.Assign(
              local.InitialisedObligationAdmActionExemption);
          }
        }

        UseLeExmpDisplayOblnAdminExmpn();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          export.HiddenDisplayPerformed.Flag = "Y";
        }
        else
        {
          export.HiddenDisplayPerformed.Flag = "N";

          if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            export.CsePersonsWorkSet.FormattedName = "";
          }
          else if (IsExitState("LE0000_LEGAL_ACTION_ID_REQD"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;
          }
          else if (IsExitState("LE0000_OBLIGATION_REQD"))
          {
            var field =
              GetField(export.Obligation, "systemGeneratedIdentifier");

            field.Error = true;

            MoveObligation(local.InitialisedObligation, export.Obligation);
          }
          else if (IsExitState("LE0000_CSE_PERSON_NOT_AN_OBLIGOR"))
          {
          }
          else if (IsExitState("LE0000_NO_OBLIGATION_FOUND"))
          {
          }
          else if (IsExitState("LE0000_ADMIN_ACT_EXMP_NF"))
          {
          }
          else if (IsExitState("LE0000_INVALID_ADMIN_ACT_TYPE"))
          {
            var field = GetField(export.AdministrativeAction, "type1");

            field.Error = true;
          }
          else if (IsExitState("FN0000_OBLIGATION_NF"))
          {
            var field =
              GetField(export.Obligation, "systemGeneratedIdentifier");

            field.Error = true;
          }
          else
          {
          }

          export.ObligationAdmActionExemption.EndDate =
            local.InitialisedObligationAdmActionExemption.EndDate;
          export.ObligationAdmActionExemption.Description =
            Spaces(ObligationAdmActionExemption.Description_MaxLength);
          export.ObligationAdmActionExemption.FirstName = "";
          export.ObligationAdmActionExemption.LastName = "";
          export.ObligationAdmActionExemption.MiddleInitial = "";
          export.ObligationAdmActionExemption.Reason = "";
          export.ObligationAdmActionExemption.Suffix = "";
        }

        break;
      case "LIST":
        local.PromptCount.Count = 0;

        if (!IsEmpty(export.ListTribunals.PromptField) && AsChar
          (export.ListTribunals.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListTribunals, "promptField");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.PromptAdminActionType.SelectChar) && AsChar
          (export.PromptAdminActionType.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptAdminActionType, "selectChar");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.PromptObligationNbr.SelectChar) && AsChar
          (export.PromptObligationNbr.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptObligationNbr, "selectChar");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListTribunals.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
          ++local.PromptCount.Count;
        }

        if (AsChar(export.PromptAdminActionType.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";
          ++local.PromptCount.Count;
        }

        if (AsChar(export.PromptObligationNbr.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_LST_OPAY_OBLG_BY_AP";
          ++local.PromptCount.Count;
        }

        if (local.PromptCount.Count == 1)
        {
        }
        else if (local.PromptCount.Count > 1)
        {
          if (AsChar(export.ListTribunals.PromptField) == 'S')
          {
            var field = GetField(export.ListTribunals, "promptField");

            field.Error = true;
          }

          if (AsChar(export.PromptAdminActionType.SelectChar) == 'S')
          {
            var field = GetField(export.PromptAdminActionType, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.PromptObligationNbr.SelectChar) == 'S')
          {
            var field = GetField(export.PromptObligationNbr, "selectChar");

            field.Error = true;
          }

          ExitState = "ZD_ACO_NE0_INVALID_MULT_PROMPT_S";
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case "RETCDVL":
        // --------  The prompt for Legal Action Classification has been 
        // removed.
        break;
      case "RETADAA":
        export.PromptAdminActionType.SelectChar = "";

        if (IsEmpty(export.AdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.AllObligations, "flag");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "RETOPAY":
        export.PromptObligationNbr.SelectChar = "";

        if (export.Obligation.SystemGeneratedIdentifier == 0)
        {
          var field = GetField(export.Obligation, "systemGeneratedIdentifier");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field =
            GetField(export.ObligationAdmActionExemption, "effectiveDate");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "ADD":
        local.UserAction.Command = global.Command;
        export.HiddenDisplayPerformed.Flag = "N";
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeExmpValidateOblnAdminExmp();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        UseLeExmpCreateOblnAdminExmpn();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.Rollback.Flag) == 'Y')
          {
            UseEabRollbackCics();
          }

          break;
        }

        export.UpdatedBy.Assign(local.InitialisedServiceProvider);

        if (export.LegalAction.Identifier > 0)
        {
          UseLeGetPetitionerRespondent();
        }

        if (Equal(export.AdministrativeAction.Description, 1, 4, "FDSO"))
        {
          ExitState = "LE0000_ANY_OTHR_FDSO_OBLIG_EXMP";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) || !
          Equal(export.CsePersonsWorkSet.Number,
          export.HiddenPrevCsePersonsWorkSet.Number) || !
          Equal(export.AdministrativeAction.Type1,
          export.HiddenPrevAdministrativeAction.Type1) || !
          Equal(export.ObligationAdmActionExemption.EffectiveDate,
          export.HiddenPrevObligationAdmActionExemption.EffectiveDate) || !
          Equal(export.Fips.StateAbbreviation,
          export.HiddenFips.StateAbbreviation) || !
          Equal(export.Fips.CountyAbbreviation,
          export.HiddenFips.CountyAbbreviation))
        {
          if (!Equal(export.ObligationAdmActionExemption.EffectiveDate,
            export.HiddenPrevObligationAdmActionExemption.EffectiveDate))
          {
            var field =
              GetField(export.ObligationAdmActionExemption, "effectiveDate");

            field.Error = true;
          }

          if (!Equal(export.AdministrativeAction.Type1,
            export.HiddenPrevAdministrativeAction.Type1))
          {
            var field = GetField(export.AdministrativeAction, "type1");

            field.Error = true;
          }

          if (!Equal(export.CsePersonsWorkSet.Number,
            export.HiddenPrevCsePersonsWorkSet.Number))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;
          }

          if (!Equal(export.Fips.CountyAbbreviation,
            export.HiddenFips.CountyAbbreviation))
          {
            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;
          }

          if (!Equal(export.Fips.StateAbbreviation,
            export.HiddenFips.StateAbbreviation))
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;
          }

          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenPrevLegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;
          }

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        if (AsChar(export.AllObligations.Flag) != 'Y')
        {
          if (export.Obligation.SystemGeneratedIdentifier != export
            .HiddenPrevObligation.SystemGeneratedIdentifier)
          {
            var field =
              GetField(export.Obligation, "systemGeneratedIdentifier");

            field.Error = true;

            export.HiddenDisplayPerformed.Flag = "N";
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            break;
          }
        }

        local.UserAction.Command = global.Command;
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeExmpValidateOblnAdminExmp();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        UseLeExmpUpdateOblnAdminExmpn();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.Rollback.Flag) == 'Y')
          {
            UseEabRollbackCics();
          }

          break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) || !
          Equal(export.CsePersonsWorkSet.Number,
          export.HiddenPrevCsePersonsWorkSet.Number) || !
          Equal(export.AdministrativeAction.Type1,
          export.HiddenPrevAdministrativeAction.Type1) || !
          Equal(export.ObligationAdmActionExemption.EffectiveDate,
          export.HiddenPrevObligationAdmActionExemption.EffectiveDate) || !
          Equal(export.Fips.StateAbbreviation,
          export.HiddenFips.StateAbbreviation) || !
          Equal(export.Fips.CountyAbbreviation,
          export.HiddenFips.CountyAbbreviation))
        {
          if (!Equal(export.ObligationAdmActionExemption.EffectiveDate,
            export.HiddenPrevObligationAdmActionExemption.EffectiveDate))
          {
            var field =
              GetField(export.ObligationAdmActionExemption, "effectiveDate");

            field.Error = true;
          }

          if (!Equal(export.AdministrativeAction.Type1,
            export.HiddenPrevAdministrativeAction.Type1))
          {
            var field = GetField(export.AdministrativeAction, "type1");

            field.Error = true;
          }

          if (!Equal(export.CsePersonsWorkSet.Number,
            export.HiddenPrevCsePersonsWorkSet.Number))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;
          }

          if (!Equal(export.Fips.CountyAbbreviation,
            export.HiddenFips.CountyAbbreviation))
          {
            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;
          }

          if (!Equal(export.Fips.StateAbbreviation,
            export.HiddenFips.StateAbbreviation))
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;
          }

          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenPrevLegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;
          }

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        if (AsChar(export.AllObligations.Flag) != 'Y')
        {
          if (export.Obligation.SystemGeneratedIdentifier != export
            .HiddenPrevObligation.SystemGeneratedIdentifier)
          {
            var field =
              GetField(export.Obligation, "systemGeneratedIdentifier");

            field.Error = true;

            export.HiddenDisplayPerformed.Flag = "N";
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            break;
          }
        }

        local.UserAction.Command = global.Command;
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;
        UseLeExmpDeleteOblnAdminExmpn();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.Rollback.Flag) == 'Y')
          {
            UseEabRollbackCics();
          }

          break;
        }

        export.ObligationAdmActionExemption.Assign(
          local.InitialisedObligationAdmActionExemption);
        export.CreatedBy.Assign(local.InitialisedServiceProvider);
        export.UpdatedBy.Assign(local.InitialisedServiceProvider);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "OBLO":
        export.ListOptSystManualActs.OneChar = "A";
        ExitState = "ECO_LNK_OBLO_ADM_ACTS_BY_OBLIGOR";

        break;
      case "EXGR":
        ExitState = "ECO_LNK_2_EXGR_EXEMPTION_GRANTED";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // ---------------------------------------------
    // Move all exports to export hidden previous
    // ---------------------------------------------
    export.HiddenPrevLegalAction.Assign(export.LegalAction);
    MoveFips(export.Fips, export.HiddenFips);
    MoveCsePersonsWorkSet1(export.CsePersonsWorkSet,
      export.HiddenPrevCsePersonsWorkSet);
    MoveAdministrativeAction(export.AdministrativeAction,
      export.HiddenPrevAdministrativeAction);
    MoveObligation(export.Obligation, export.HiddenPrevObligation);
    export.HiddenPrevObligationAdmActionExemption.Assign(
      export.ObligationAdmActionExemption);
    export.HiddenPrevUserAction.Command = global.Command;

    if (AsChar(local.HighlightError.Flag) == 'Y')
    {
      local.ErrorCodes.Index = local.LastErrorEntryNo.Count - 1;
      local.ErrorCodes.CheckSize();

      while(local.ErrorCodes.Index >= 0)
      {
        switch(local.ErrorCodes.Item.DetailErrorCode.Count)
        {
          case 1:
            ExitState = "LE0000_INVALID_OPT_DISP_ALL_OBLG";

            var field1 = GetField(export.AllObligations, "flag");

            field1.Error = true;

            break;
          case 2:
            ExitState = "LE0000_LEG_ACT_NOT_SELECTED";

            var field2 = GetField(export.LegalAction, "courtCaseNumber");

            field2.Error = true;

            break;
          case 3:
            ExitState = "LEGAL_ACTION_NF";

            var field3 = GetField(export.LegalAction, "courtCaseNumber");

            field3.Error = true;

            break;
          case 4:
            ExitState = "LE0000_OBLG_NOT_SPECIFIED";

            var field4 =
              GetField(export.Obligation, "systemGeneratedIdentifier");

            field4.Error = true;

            break;
          case 5:
            ExitState = "CSE_PERSON_NF";

            var field5 = GetField(export.CsePersonsWorkSet, "number");

            field5.Error = true;

            break;
          case 6:
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

            var field6 = GetField(export.CsePersonsWorkSet, "number");

            field6.Error = true;

            break;
          case 7:
            ExitState = "FN0000_OBLIGATION_NF";

            var field7 =
              GetField(export.Obligation, "systemGeneratedIdentifier");

            field7.Error = true;

            break;
          case 8:
            ExitState = "LE0000_INVALID_ADMIN_ACTION_TYPE";

            var field8 = GetField(export.AdministrativeAction, "type1");

            field8.Error = true;

            break;
          case 9:
            ExitState = "LE0000_INVALID_ADM_EXMP_EFF_DT";

            var field9 =
              GetField(export.ObligationAdmActionExemption, "effectiveDate");

            field9.Error = true;

            break;
          case 10:
            ExitState = "LE0000_INVALID_ADM_EXMP_END_DT";

            var field10 =
              GetField(export.ObligationAdmActionExemption, "endDate");

            field10.Error = true;

            break;
          case 11:
            ExitState = "LE0000_ADM_EXMP_LAST_NAME_REQD";

            var field11 =
              GetField(export.ObligationAdmActionExemption, "lastName");

            field11.Error = true;

            break;
          case 12:
            ExitState = "LE0000_FIRST_NAME_ADM_EXMP_REQD";

            var field12 =
              GetField(export.ObligationAdmActionExemption, "firstName");

            field12.Error = true;

            break;
          case 13:
            ExitState = "LE0000_ADM_EXMP_REASON_REQD";

            var field13 =
              GetField(export.ObligationAdmActionExemption, "reason");

            field13.Error = true;

            break;
          case 14:
            ExitState = "LE0000_CT_CASE_REQD_IF_ALL_OBL_Y";

            var field14 = GetField(export.AllObligations, "flag");

            field14.Error = true;

            break;
          case 15:
            ExitState = "LE0000_PERS_NOT_OBLIGR_CT_CASE";

            var field15 = GetField(export.LegalAction, "courtCaseNumber");

            field15.Error = true;

            var field16 = GetField(export.CsePersonsWorkSet, "number");

            field16.Error = true;

            break;
          case 16:
            ExitState = "LE0000_MAN_ADMIN_ACT_NOT_ALLOW";

            var field17 = GetField(export.AdministrativeAction, "type1");

            field17.Error = true;

            break;
          case 17:
            ExitState = "LE0000_ALL_OBLG_YES_FOR_FDSO";

            var field18 = GetField(export.AllObligations, "flag");

            field18.Error = true;

            break;
          case 18:
            ExitState = "LE0000_OVERLAPED_EXMPS_NOT_ALLWD";

            var field19 =
              GetField(export.ObligationAdmActionExemption, "effectiveDate");

            field19.Error = true;

            var field20 =
              GetField(export.ObligationAdmActionExemption, "endDate");

            field20.Error = true;

            break;
          case 19:
            ExitState = "LE0000_ACTIVE_ALL_FDSO_EXISTS";

            var field21 = GetField(export.AdministrativeAction, "type1");

            field21.Error = true;

            break;
          case 20:
            ExitState = "LE0000_ACTIVE_FDSO_EXMP_EXISTS";

            var field22 = GetField(export.AdministrativeAction, "type1");

            field22.Error = true;

            break;
          case 21:
            ExitState = "LE0000_EXMP_END_MUST_BE_CURR_DT";

            var field23 =
              GetField(export.ObligationAdmActionExemption, "endDate");

            field23.Error = true;

            break;
          case 22:
            ExitState = "LE0000_CANT_CHG_ENDED_EXEMPTIONS";

            var field24 =
              GetField(export.ObligationAdmActionExemption, "endDate");

            field24.Error = true;

            break;
          default:
            ExitState = "ACO_NE0000_UNKNOWN_ERROR_CODE";

            break;
        }

        --local.ErrorCodes.Index;
        local.ErrorCodes.CheckSize();
      }
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
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
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveErrorCodes(LeExmpValidateOblnAdminExmp.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
  }

  private static void MoveExport1ToLocal1(CabMatchCsePerson.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.Deatil.Assign(source.Detail);
    target.DetailAlt.Flag = source.Ae.Flag;
    target.Ae.Flag = source.Cse.Flag;
    target.Cse.Flag = source.Kanpay.Flag;
    target.Kanpay.Flag = source.Kscares.Flag;
    target.Kscares.Flag = source.Alt.Flag;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
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

    MoveCommon(local.SearchPerson, useImport.Search);
    MoveCsePersonsWorkSet4(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeExmpCreateOblnAdminExmpn()
  {
    var useImport = new LeExmpCreateOblnAdminExmpn.Import();
    var useExport = new LeExmpCreateOblnAdminExmpn.Export();

    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Foreign.Identifier = export.Tribunal.Identifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.ObligationAdmActionExemption.Assign(
      export.ObligationAdmActionExemption);
    MoveLegalAction(export.LegalAction, useImport.LegalAction);
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.CreateForAllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(LeExmpCreateOblnAdminExmpn.Execute, useImport, useExport);

    local.Rollback.Flag = useExport.RollbackFlag.Flag;
    export.Foreign.Country = useExport.Foreign.Country;
    MoveObligation(useExport.Obligation, export.Obligation);
    export.ObligationType.Assign(useExport.ObligationType);
    export.LegalAction.CourtCaseNumber = useExport.LegalAction.CourtCaseNumber;
    export.Tribunal.Assign(useExport.Tribunal);
    export.Fips.Assign(useExport.Fips);
    export.CreatedBy.Assign(useExport.CreatedBy);
  }

  private void UseLeExmpDeleteOblnAdminExmpn()
  {
    var useImport = new LeExmpDeleteOblnAdminExmpn.Import();
    var useExport = new LeExmpDeleteOblnAdminExmpn.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Foreign.Identifier = export.Tribunal.Identifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.ObligationAdmActionExemption.EffectiveDate =
      export.ObligationAdmActionExemption.EffectiveDate;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.DeleteForAllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(LeExmpDeleteOblnAdminExmpn.Execute, useImport, useExport);

    local.Rollback.Flag = useExport.Rollback.Flag;
  }

  private void UseLeExmpDisplayOblnAdminExmpn()
  {
    var useImport = new LeExmpDisplayOblnAdminExmpn.Import();
    var useExport = new LeExmpDisplayOblnAdminExmpn.Export();

    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.ObligationAdmActionExemption.EffectiveDate =
      export.ObligationAdmActionExemption.EffectiveDate;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.DisplayAnyObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(LeExmpDisplayOblnAdminExmpn.Execute, useImport, useExport);

    export.Foreign.Country = useExport.Foreign.Country;
    export.ObligationType.Assign(useExport.ObligationType);
    export.LegalAction.Assign(useExport.LegalAction);
    export.Tribunal.Assign(useExport.Tribunal);
    export.Fips.Assign(useExport.Fips);
    export.UpdatedBy.Assign(useExport.UpdatedBy);
    export.CreatedBy.Assign(useExport.CreatedBy);
    export.ObligationAdmActionExemption.Assign(
      useExport.ObligationAdmActionExemption);
    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    MoveObligation(useExport.Obligation, export.Obligation);
    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeExmpUpdateOblnAdminExmpn()
  {
    var useImport = new LeExmpUpdateOblnAdminExmpn.Import();
    var useExport = new LeExmpUpdateOblnAdminExmpn.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Foreign.Identifier = export.Tribunal.Identifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.ObligationAdmActionExemption.Assign(
      export.ObligationAdmActionExemption);
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.UpdateForAllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(LeExmpUpdateOblnAdminExmpn.Execute, useImport, useExport);

    local.Rollback.Flag = useExport.Rollback.Flag;
    export.UpdatedBy.Assign(useExport.UpdatedBy);

  }

  private void UseLeExmpValidateOblnAdminExmp()
  {
    var useImport = new LeExmpValidateOblnAdminExmp.Import();
    var useExport = new LeExmpValidateOblnAdminExmp.Export();

    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.ObligationAdmActionExemption.Assign(
      export.ObligationAdmActionExemption);
    useImport.UserAction.Command = local.UserAction.Command;
    MoveAdministrativeAction(export.AdministrativeAction,
      useImport.AdministrativeAction);
    useImport.AllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(LeExmpValidateOblnAdminExmp.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
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

  private bool ReadFips1()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 1);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 2);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.TrbId = db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionTribunalFips()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingFips.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionTribunalFips",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 3);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 4);
        entities.ExistingTribunal.Name = db.GetString(reader, 5);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 6);
        entities.ExistingFips.Location = db.GetInt32(reader, 6);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 7);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 8);
        entities.ExistingFips.County = db.GetInt32(reader, 8);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 9);
        entities.ExistingFips.State = db.GetInt32(reader, 9);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 10);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 11);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 12);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingFips.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.Populated = true;
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
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of ListTribunals.
    /// </summary>
    [JsonPropertyName("listTribunals")]
    public Standard ListTribunals
    {
      get => listTribunals ??= new();
      set => listTribunals = value;
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
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of UpdatedBy.
    /// </summary>
    [JsonPropertyName("updatedBy")]
    public ServiceProvider UpdatedBy
    {
      get => updatedBy ??= new();
      set => updatedBy = value;
    }

    /// <summary>
    /// A value of CreatedBy.
    /// </summary>
    [JsonPropertyName("createdBy")]
    public ServiceProvider CreatedBy
    {
      get => createdBy ??= new();
      set => createdBy = value;
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
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
    }

    /// <summary>
    /// A value of HiddenPrevObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("hiddenPrevObligationAdmActionExemption")]
    public ObligationAdmActionExemption HiddenPrevObligationAdmActionExemption
    {
      get => hiddenPrevObligationAdmActionExemption ??= new();
      set => hiddenPrevObligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of PromptObligationNbr.
    /// </summary>
    [JsonPropertyName("promptObligationNbr")]
    public Common PromptObligationNbr
    {
      get => promptObligationNbr ??= new();
      set => promptObligationNbr = value;
    }

    /// <summary>
    /// A value of PromptLegActClassifn.
    /// </summary>
    [JsonPropertyName("promptLegActClassifn")]
    public Common PromptLegActClassifn
    {
      get => promptLegActClassifn ??= new();
      set => promptLegActClassifn = value;
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
    /// A value of AllObligations.
    /// </summary>
    [JsonPropertyName("allObligations")]
    public Common AllObligations
    {
      get => allObligations ??= new();
      set => allObligations = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of PromptAdminActionType.
    /// </summary>
    [JsonPropertyName("promptAdminActionType")]
    public Common PromptAdminActionType
    {
      get => promptAdminActionType ??= new();
      set => promptAdminActionType = value;
    }

    /// <summary>
    /// A value of HiddenPrevObligation.
    /// </summary>
    [JsonPropertyName("hiddenPrevObligation")]
    public Obligation HiddenPrevObligation
    {
      get => hiddenPrevObligation ??= new();
      set => hiddenPrevObligation = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevAdministrativeAction")]
    public AdministrativeAction HiddenPrevAdministrativeAction
    {
      get => hiddenPrevAdministrativeAction ??= new();
      set => hiddenPrevAdministrativeAction = value;
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
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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

    private FipsTribAddress fipsTribAddress;
    private FipsTribAddress foreign;
    private Standard standard;
    private Standard listTribunals;
    private Tribunal tribunal;
    private Fips hiddenFips;
    private ServiceProvider updatedBy;
    private ServiceProvider createdBy;
    private ObligationType obligationType;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private ObligationAdmActionExemption hiddenPrevObligationAdmActionExemption;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private CodeValue selected;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private Common promptObligationNbr;
    private Common promptLegActClassifn;
    private Obligation obligation;
    private Common allObligations;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private AdministrativeAction administrativeAction;
    private Common promptAdminActionType;
    private Obligation hiddenPrevObligation;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private LegalAction hiddenPrevLegalAction;
    private AdministrativeAction hiddenPrevAdministrativeAction;
    private Fips fips;
    private Security2 hiddenSecurity;
    private SsnWorkArea ssnWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of ListTribunals.
    /// </summary>
    [JsonPropertyName("listTribunals")]
    public Standard ListTribunals
    {
      get => listTribunals ??= new();
      set => listTribunals = value;
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

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
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
    /// A value of UpdatedBy.
    /// </summary>
    [JsonPropertyName("updatedBy")]
    public ServiceProvider UpdatedBy
    {
      get => updatedBy ??= new();
      set => updatedBy = value;
    }

    /// <summary>
    /// A value of CreatedBy.
    /// </summary>
    [JsonPropertyName("createdBy")]
    public ServiceProvider CreatedBy
    {
      get => createdBy ??= new();
      set => createdBy = value;
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
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
    }

    /// <summary>
    /// A value of HiddenPrevObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("hiddenPrevObligationAdmActionExemption")]
    public ObligationAdmActionExemption HiddenPrevObligationAdmActionExemption
    {
      get => hiddenPrevObligationAdmActionExemption ??= new();
      set => hiddenPrevObligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of PromptObligationNbr.
    /// </summary>
    [JsonPropertyName("promptObligationNbr")]
    public Common PromptObligationNbr
    {
      get => promptObligationNbr ??= new();
      set => promptObligationNbr = value;
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
    /// A value of AllObligations.
    /// </summary>
    [JsonPropertyName("allObligations")]
    public Common AllObligations
    {
      get => allObligations ??= new();
      set => allObligations = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of PromptAdminActionType.
    /// </summary>
    [JsonPropertyName("promptAdminActionType")]
    public Common PromptAdminActionType
    {
      get => promptAdminActionType ??= new();
      set => promptAdminActionType = value;
    }

    /// <summary>
    /// A value of HiddenPrevObligation.
    /// </summary>
    [JsonPropertyName("hiddenPrevObligation")]
    public Obligation HiddenPrevObligation
    {
      get => hiddenPrevObligation ??= new();
      set => hiddenPrevObligation = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevAdministrativeAction")]
    public AdministrativeAction HiddenPrevAdministrativeAction
    {
      get => hiddenPrevAdministrativeAction ??= new();
      set => hiddenPrevAdministrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
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
    /// A value of FlowKdmv.
    /// </summary>
    [JsonPropertyName("flowKdmv")]
    public CsePersonsWorkSet FlowKdmv
    {
      get => flowKdmv ??= new();
      set => flowKdmv = value;
    }

    private FipsTribAddress fipsTribAddress;
    private FipsTribAddress foreign;
    private Standard standard;
    private Standard listTribunals;
    private Tribunal tribunal;
    private Fips fips;
    private Fips hiddenFips;
    private Standard listOptSystManualActs;
    private ServiceProvider updatedBy;
    private ServiceProvider createdBy;
    private ObligationType obligationType;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private ObligationAdmActionExemption hiddenPrevObligationAdmActionExemption;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private Code required;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private Common promptObligationNbr;
    private Obligation obligation;
    private Common allObligations;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private AdministrativeAction administrativeAction;
    private Common promptAdminActionType;
    private Obligation hiddenPrevObligation;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private LegalAction hiddenPrevLegalAction;
    private AdministrativeAction hiddenPrevAdministrativeAction;
    private Security2 hiddenSecurity;
    private SsnWorkArea ssnWorkArea;
    private CsePersonsWorkSet flowKdmv;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Deatil.
      /// </summary>
      [JsonPropertyName("deatil")]
      public CsePersonsWorkSet Deatil
      {
        get => deatil ??= new();
        set => deatil = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet deatil;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public TextWorkArea Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of Rollback.
    /// </summary>
    [JsonPropertyName("rollback")]
    public Common Rollback
    {
      get => rollback ??= new();
      set => rollback = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
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
    /// A value of InitialisedServiceProvider.
    /// </summary>
    [JsonPropertyName("initialisedServiceProvider")]
    public ServiceProvider InitialisedServiceProvider
    {
      get => initialisedServiceProvider ??= new();
      set => initialisedServiceProvider = value;
    }

    /// <summary>
    /// A value of InitialisedObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("initialisedObligationAdmActionExemption")]
    public ObligationAdmActionExemption InitialisedObligationAdmActionExemption
    {
      get => initialisedObligationAdmActionExemption ??= new();
      set => initialisedObligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// A value of InitialisedObligation.
    /// </summary>
    [JsonPropertyName("initialisedObligation")]
    public Obligation InitialisedObligation
    {
      get => initialisedObligation ??= new();
      set => initialisedObligation = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of SearchPerson.
    /// </summary>
    [JsonPropertyName("searchPerson")]
    public Common SearchPerson
    {
      get => searchPerson ??= new();
      set => searchPerson = value;
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
    /// A value of DisplayAnyObligation.
    /// </summary>
    [JsonPropertyName("displayAnyObligation")]
    public Common DisplayAnyObligation
    {
      get => displayAnyObligation ??= new();
      set => displayAnyObligation = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of InitialisedAdministrativeAction.
    /// </summary>
    [JsonPropertyName("initialisedAdministrativeAction")]
    public AdministrativeAction InitialisedAdministrativeAction
    {
      get => initialisedAdministrativeAction ??= new();
      set => initialisedAdministrativeAction = value;
    }

    /// <summary>
    /// A value of NoOfLegalActionsFound.
    /// </summary>
    [JsonPropertyName("noOfLegalActionsFound")]
    public Common NoOfLegalActionsFound
    {
      get => noOfLegalActionsFound ??= new();
      set => noOfLegalActionsFound = value;
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
    /// A value of ReturnedFromLacn.
    /// </summary>
    [JsonPropertyName("returnedFromLacn")]
    public Common ReturnedFromLacn
    {
      get => returnedFromLacn ??= new();
      set => returnedFromLacn = value;
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

    private TextWorkArea country;
    private Common rollback;
    private Common promptCount;
    private TextWorkArea textWorkArea;
    private ServiceProvider initialisedServiceProvider;
    private ObligationAdmActionExemption initialisedObligationAdmActionExemption;
      
    private Common highlightError;
    private Array<ErrorCodesGroup> errorCodes;
    private Common lastErrorEntryNo;
    private Obligation initialisedObligation;
    private Array<LocalGroup> local1;
    private Common searchPerson;
    private CsePerson csePerson;
    private Common displayAnyObligation;
    private Common userAction;
    private AdministrativeAction initialisedAdministrativeAction;
    private Common noOfLegalActionsFound;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private DateWorkArea initialisedToZeros;
    private Common returnedFromLacn;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    private FipsTribAddress existingFipsTribAddress;
    private Tribunal existingTribunal;
    private Fips existingFips;
    private LegalAction existingLegalAction;
  }
#endregion
}

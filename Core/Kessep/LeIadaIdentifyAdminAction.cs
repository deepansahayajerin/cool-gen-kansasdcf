// Program: LE_IADA_IDENTIFY_ADMIN_ACTION, ID: 372594530, model: 746.
// Short name: SWEIADAP
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
/// A program: LE_IADA_IDENTIFY_ADMIN_ACTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeIadaIdentifyAdminAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_IADA_IDENTIFY_ADMIN_ACTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeIadaIdentifyAdminAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeIadaIdentifyAdminAction.
  /// </summary>
  public LeIadaIdentifyAdminAction(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Every initial development and change to that development
    // needs to be documented
    // --------------------------------------------------------------
    // ----------------------------------------------------------------
    // Date            Developer   	Description
    // 09-27-95        Govind      	Initial development
    // 05-06-96        Siraj Konkader	Print functions.
    //                 		added xfer to IWGL for
    // 				printing Lien docs
    // 11-14-97	govind		Flow to ASIN removed
    // 12-16-97 	R Grey	H00033998 Next Tran fix
    // 01-15-98	R Grey  H00033996 Edit for print Bond Letter only
    // 10/16/98	P. Sharp	Removed validation action
    // 				block before read.
    // 				Cleaned up exit
    // 				states.
    // 				Added validation to
    // 				only allow manual
    // 				admin actions.
    // 12/15/1998	M. Ramirez	Revised print process
    // 05/26/1999	PMcElderry	CSEnet functionality;
    // 				Added new display
    // 				exit state.
    // 07/02/99	PMcElderry	Logic to bring over OBL from
    // 				OPAY
    // 07/20/99	PMcElderry
    // For LIEN and BLTR admin types, date must be the current
    // date
    // 10/02/01        T.Bobb         PR00128723 Changed read on FIPS to a read 
    // with cursor to eliminate 811 abends.
    // ----------------------------------------------------------------
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

    UseSpDocSetLiterals();
    local.HighlightError.Flag = "Y";

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    export.Print.Name = import.Print.Name;
    export.PromptTribunal.PromptField = import.PromptTribunal.PromptField;
    export.Foreign.Country = import.Foreign.Country;
    export.LegalAction.Assign(import.LegalAction);
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
    export.ObligationAdministrativeAction.Assign(
      import.ObligationAdministrativeAction);
    export.PromptObligationNbr.SelectChar =
      import.PromptObligationNbr.SelectChar;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.ObligationType.Assign(import.ObligationType);
    export.AdminActionTakenBy.Assign(import.AdminActionTakenBy);
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.Foreign.Country = import.Foreign.Country;
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
    export.HiddenPrevObligationAdministrativeAction.Assign(
      import.HiddenPrevObligationAdministrativeAction);
    MoveAdministrativeAction(import.HiddenPrevAdministrativeAction,
      export.HiddenPrevAdministrativeAction);
    MoveFips(import.HiddenFips, export.HiddenFips);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;

    if (IsEmpty(export.AllObligations.Flag))
    {
      export.AllObligations.Flag = "N";
    }

    local.Rollback.Flag = "N";

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

      // ------------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // -------------------------------------------------------------
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();

      // ************************************************************
      // RCG  12/16/97 On Next Tran Read for Tribunal and FIPS
      // info.
      // Set Command = Display instead of Escape out.
      // ************************************************************
      if (export.LegalAction.Identifier > 0)
      {
        if (ReadLegalActionTribunalFips2())
        {
          export.LegalAction.Assign(entities.ExistingLegalAction);
          export.Tribunal.Assign(entities.ExistingTribunal);
          export.Fips.Assign(entities.ExistingFips);
        }
        else
        {
          var field1 = GetField(export.Fips, "stateAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "countyAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.LegalAction, "courtCaseNumber");

          field3.Error = true;

          ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

          return;
        }
      }

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
      global.Command = "DISPLAY";
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
      UseScCabNextTranPut1();

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "PRINT"))
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
      Equal(global.Command, "RETOPAY") && !Equal(global.Command, "RETADAA") && !
      Equal(global.Command, "RETLTRB"))
    {
      export.PromptAdminActionType.SelectChar = "";
      export.PromptObligationNbr.SelectChar = "";
      export.PromptTribunal.PromptField = "";
    }

    if (Equal(global.Command, "RETOPAY"))
    {
      local.Previous.Command = global.Command;
      global.Command = "DISPLAY";
      export.PromptObligationNbr.SelectChar = "";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "PRINT"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        if (IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          export.ObligationAdministrativeAction.Assign(
            local.InitialisedObligationAdministrativeAction);
          MoveAdministrativeAction(local.InitialisedAdministrativeAction,
            export.AdministrativeAction);
          MoveObligation(local.InitialisedObligation, export.Obligation);
          export.AdminActionTakenBy.Assign(local.InitialisedServiceProvider);
          export.ObligationType.Assign(local.InitialisedObligationType);
          export.PetitionerRespondentDetails.Assign(
            local.InitialisedPetitionerRespondentDetails);
          export.CsePersonsWorkSet.FormattedName = "";

          var field1 = GetField(export.CsePersonsWorkSet, "number");

          field1.Error = true;

          var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field2.Error = true;

          var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field3.Error = true;

          var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field4.Error = true;

          ExitState = "LE0000_CSE_PERSON_NO_OR_SSN_REQD";

          return;
        }
        else
        {
          local.SearchPerson.Flag = "1";
          UseCabMatchCsePerson();

          local.Local1.Index = 0;
          local.Local1.CheckSize();

          MoveCsePersonsWorkSet2(local.Local1.Item.Detail,
            export.CsePersonsWorkSet);

          if (IsEmpty(export.CsePersonsWorkSet.Number))
          {
            MoveCsePersonsWorkSet3(import.CsePersonsWorkSet,
              export.CsePersonsWorkSet);

            var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

            field1.Error = true;

            var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

            field2.Error = true;

            var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

            field3.Error = true;

            var field4 = GetField(export.CsePersonsWorkSet, "number");

            field4.Error = true;

            export.CsePersonsWorkSet.FormattedName = "";

            if (AsChar(export.HiddenDisplayPerformed.Flag) == 'Y')
            {
              export.ObligationAdministrativeAction.Assign(
                local.InitialisedObligationAdministrativeAction);
              MoveAdministrativeAction(local.InitialisedAdministrativeAction,
                export.AdministrativeAction);
              MoveObligation(local.InitialisedObligation, export.Obligation);
              export.AdminActionTakenBy.
                Assign(local.InitialisedServiceProvider);
              export.PetitionerRespondentDetails.Assign(
                local.InitialisedPetitionerRespondentDetails);
              export.ObligationType.Assign(local.InitialisedObligationType);
              export.Fips.Assign(local.InitialisedFips);
              export.Foreign.Country = local.InitialisedFipsTribAddress.Country;
              export.Tribunal.Assign(local.InitialisedTribunal);
              export.LegalAction.Assign(local.Initialized);
            }

            ExitState = "LE0000_CSE_PERSON_NF_FOR_SSN";

            return;
          }

          UseSiFormatCsePersonName();
        }
      }
      else
      {
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCsePersonsWorkSet3(import.CsePersonsWorkSet,
            export.CsePersonsWorkSet);
          export.CsePersonsWorkSet.FormattedName = "";
          ExitState = "CSE_PERSON_NF";

          if (AsChar(export.HiddenDisplayPerformed.Flag) == 'Y')
          {
            export.ObligationAdministrativeAction.Assign(
              local.InitialisedObligationAdministrativeAction);
            MoveAdministrativeAction(local.InitialisedAdministrativeAction,
              export.AdministrativeAction);
            MoveObligation(local.InitialisedObligation, export.Obligation);
            export.AdminActionTakenBy.Assign(local.InitialisedServiceProvider);
            export.PetitionerRespondentDetails.Assign(
              local.InitialisedPetitionerRespondentDetails);
            export.ObligationType.Assign(local.InitialisedObligationType);
            export.Fips.Assign(local.InitialisedFips);
            export.Foreign.Country = local.InitialisedFipsTribAddress.Country;
            export.Tribunal.Assign(local.InitialisedTribunal);
            export.LegalAction.Assign(local.Initialized);
          }

          return;
        }
      }

      if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
      {
        export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "PRINT"))
    {
      if (!IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          if (export.Tribunal.Identifier == 0)
          {
            ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

            var field = GetField(export.PromptTribunal, "promptField");

            field.Error = true;

            return;
          }
          else if (ReadLegalActionTribunal())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
            export.Tribunal.Assign(entities.ExistingTribunal);
          }
          else
          {
            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.Foreign, "country");

            field2.Error = true;

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

          if (ReadLegalActionTribunalFips1())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
            export.Tribunal.Assign(entities.ExistingTribunal);
            export.Fips.Assign(entities.ExistingFips);
          }
          else
          {
            // *****************************************************************
            // 10/02/01 T.Bobb PR00128723 Changed read from a select only to 
            // read
            //  with select only and cursor to eliminate 811 abends.
            // *****************************************************************
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

            var field1 = GetField(export.Fips, "stateAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.Fips, "countyAbbreviation");

            field2.Error = true;

            var field3 = GetField(export.LegalAction, "courtCaseNumber");

            field3.Error = true;

            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

            return;
          }
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "IWGL":
        ExitState = "ECO_XFR_TO_IWO_GARNISHMENT_LIEN";

        return;
      case "PRINT":
        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // Revised print process -
        //   Added flow to DOCM
        //   Added flow to DKEY (using NEXTTRAN)
        //   Added command RETDOCM
        //   Added command PRINTRET
        // ------------------------------------------------------------
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE") && !
          Equal(export.HiddenPrevUserAction.Command, "ADD"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
          export.HiddenDisplayPerformed.Flag = "N";

          return;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
          export.HiddenDisplayPerformed.Flag = "N";

          return;
        }

        export.DocmProtectFilter.Flag = "Y";
        export.Print.Type1 = "IADA";
        ExitState = "ECO_LNK_TO_DOCM";

        return;
      case "RETDOCM":
        if (IsEmpty(import.Print.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          return;
        }

        export.Standard.NextTransaction = "DKEY";
        local.NextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + import.Print.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        local.NextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;
        local.NextTranInfo.ObligationId =
          export.Obligation.SystemGeneratedIdentifier;
        local.NextTranInfo.MiscNum2 =
          export.ObligationType.SystemGeneratedIdentifier;
        local.NextTranInfo.CsePersonNumberObligor =
          export.CsePersonsWorkSet.Number;
        local.NextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdAdminAction) + TrimEnd
          (export.AdministrativeAction.Type1) + ";";
        local.WorkArea.Text50 =
          NumberToString(DateToInt(
            export.ObligationAdministrativeAction.TakenDate), 8, 8);
        local.NextTranInfo.MiscText1 = TrimEnd(local.NextTranInfo.MiscText1) + TrimEnd
          (local.SpDocLiteral.IdObligationAdminAction) + local.WorkArea.Text50;
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
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
        export.LegalAction.Identifier =
          local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
        export.Obligation.SystemGeneratedIdentifier =
          local.NextTranInfo.ObligationId.GetValueOrDefault();
        export.ObligationType.SystemGeneratedIdentifier =
          (int)local.NextTranInfo.MiscNum2.GetValueOrDefault();
        export.CsePersonsWorkSet.Number =
          local.NextTranInfo.CsePersonNumberObligor ?? Spaces(10);
        local.Position.Count =
          Find(String(
            local.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdAdminAction));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.AdministrativeAction.Type1 =
          Substring(local.NextTranInfo.MiscText1, local.Position.Count + 7, 4);
        local.Position.Count =
          Find(String(
            local.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdObligationAdminAction));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.ObligationAdministrativeAction.TakenDate =
          IntToDate((int)StringToNumber(Substring(
            local.NextTranInfo.MiscText1, 50, local.Position.Count + 9, 8)));
        global.Command = "DISPLAY";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "DISPLAY":
        break;
      case "LIST":
        local.PromptCount.Count = 0;

        if (!IsEmpty(export.PromptTribunal.PromptField) && AsChar
          (export.PromptTribunal.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptTribunal, "promptField");

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

        if (AsChar(export.PromptTribunal.PromptField) == 'S')
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
          break;
        }
        else if (local.PromptCount.Count > 1)
        {
          if (AsChar(export.PromptTribunal.PromptField) == 'S')
          {
            var field = GetField(export.PromptTribunal, "promptField");

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

          break;
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETCDVL":
        // -------------------------------------------------------------
        // the prompt for legal action classification has been removed
        // from this procedure
        // -------------------------------------------------------------
        break;
      case "RETLTRB":
        export.PromptTribunal.PromptField = "";

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
            GetField(export.ObligationAdministrativeAction, "takenDate");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "ADD":
        if ((Equal(export.AdministrativeAction.Type1, "LIEN") || Equal
          (export.AdministrativeAction.Type1, "BLTR")) && !
          Equal(export.ObligationAdministrativeAction.TakenDate, Now().Date))
        {
          var field =
            GetField(export.ObligationAdministrativeAction, "takenDate");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NE0000_DT_MUST_B_CURRENT_DT";

          break;
        }
        else
        {
          local.UserAction.Command = global.Command;
          export.HiddenDisplayPerformed.Flag = "N";
          local.CsePerson.Number = export.CsePersonsWorkSet.Number;
          UseLeIadaValidateOblnAdminAct();

          if (local.LastErrorEntryNo.Count > 0)
          {
            local.HighlightError.Flag = "Y";

            break;
          }

          UseLeIadaCreateOblnAdminAction();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(local.Rollback.Flag) == 'Y')
            {
              UseEabRollbackCics();
            }

            break;
          }

          if (export.LegalAction.Identifier != 0)
          {
            UseLeGetPetitionerRespondent();
          }
        }

        // --------------------
        // CSEnet functionality
        // --------------------
        if (Equal(export.AdministrativeAction.Type1, "LIEN") || Equal
          (export.AdministrativeAction.Type1, "OREL"))
        {
          if (IsEmpty(export.LegalAction.CourtCaseNumber))
          {
            switch(TrimEnd(export.AdministrativeAction.Type1))
            {
              case "OREL":
                local.Specific.ActionReasonCode = "OPERS";

                break;
              case "LIEN":
                local.Specific.ActionReasonCode = "LPERS";

                break;
              default:
                // ------------------
                // only possibilities
                // ------------------
                break;
            }
          }
          else if (ReadLegalActionPerson())
          {
            if (ReadCase())
            {
              local.Case1.Number = entities.Case1.Number;

              switch(TrimEnd(export.AdministrativeAction.Type1))
              {
                case "OREL":
                  local.Specific.ActionReasonCode = "OCASE";

                  break;
                case "LIEN":
                  local.Specific.ActionReasonCode = "LCASE";

                  break;
                default:
                  // ------------------
                  // only possibilities
                  // ------------------
                  break;
              }
            }
            else
            {
              ExitState = "CASE_NF_RB";

              return;
            }
          }
          else
          {
            ExitState = "CO0000_LEGAL_ACTION_PERSON_NF_RB";

            return;
          }

          if (!IsEmpty(local.Specific.ActionReasonCode))
          {
            local.ScreenIdentification.Command = "IADA";
            UseSiCreateAutoCsenetTrans();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
            else
            {
              // -----------------------------------------
              // EAB for rollback already called within AB
              // -----------------------------------------
              return;
            }
          }
          else
          {
            // -----------------------------
            // no CSEnet processing required
            // -----------------------------
          }
        }
        else
        {
          // -----------------------------
          // no CSEnet processing required
          // -----------------------------
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        if ((Equal(export.AdministrativeAction.Type1, "LIEN") || Equal
          (export.AdministrativeAction.Type1, "BLTR")) && !
          Equal(export.ObligationAdministrativeAction.TakenDate, Now().Date))
        {
          var field =
            GetField(export.ObligationAdministrativeAction, "takenDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DT_MUST_B_CURRENT_DT";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

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
          Equal(export.ObligationAdministrativeAction.TakenDate,
          export.HiddenPrevObligationAdministrativeAction.TakenDate) || !
          Equal(export.Fips.StateAbbreviation,
          import.HiddenFips.StateAbbreviation) || !
          Equal(export.Fips.CountyAbbreviation,
          import.HiddenFips.CountyAbbreviation))
        {
          if (!Equal(export.ObligationAdministrativeAction.TakenDate,
            export.HiddenPrevObligationAdministrativeAction.TakenDate))
          {
            var field =
              GetField(export.ObligationAdministrativeAction, "takenDate");

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
        UseLeIadaValidateOblnAdminAct();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        UseLeIadaUpdateOblnAdminAction();

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
          Equal(export.ObligationAdministrativeAction.TakenDate,
          export.HiddenPrevObligationAdministrativeAction.TakenDate) || !
          Equal(export.Fips.StateAbbreviation,
          import.HiddenFips.StateAbbreviation) || !
          Equal(export.Fips.CountyAbbreviation,
          import.HiddenFips.CountyAbbreviation))
        {
          if (!Equal(export.ObligationAdministrativeAction.TakenDate,
            export.HiddenPrevObligationAdministrativeAction.TakenDate))
          {
            var field =
              GetField(export.ObligationAdministrativeAction, "takenDate");

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
        UseLeIadaDeleteOblnAdminAction();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.Rollback.Flag) == 'Y')
          {
            UseEabRollbackCics();
          }

          break;
        }

        export.ObligationAdministrativeAction.Assign(
          local.InitialisedObligationAdministrativeAction);
        export.AdminActionTakenBy.Assign(local.InitialisedServiceProvider);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "OBLO":
        export.ListOptSystManual.OneChar = "M";
        ExitState = "ECO_LNK_OBLO_ADM_ACTS_BY_OBLIGOR";

        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // mjr
    // ------------------------------------------------
    // 12/15/1998
    // Pulled command Display out of regular case of command construct,
    // so it would be performed after a PrintRet.
    // -------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
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
          // ----------------------------------------------------------
          // Removed SET statement that set obligation administrative
          // action action_date to zeros from here. When linked from
          // AACC to IADA it was clearing it and hence was not
          // displaying the selected record.
          // ----------------------------------------------------------
          export.ObligationAdministrativeAction.ResponseDate =
            local.InitialisedToZeros.Date;
          export.ObligationAdministrativeAction.Response =
            Spaces(ObligationAdministrativeAction.Response_MaxLength);
        }
      }

      local.MoreAdminAct.Flag = "N";
      UseLeIadaDisplayOblnAdminAct();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(local.MoreAdminAct.Flag) == 'Y')
        {
          ExitState = "LE0000_MORE_ADMIN_ACTION_FOR_OBL";
        }
        else
        {
          // mjr
          // -----------------------------------------------
          // 12/15/1998
          // Added check for an exitstate returned from Print
          // ------------------------------------------------------------
          local.Position.Count =
            Find(String(
              local.NextTranInfo.MiscText2, NextTranInfo.MiscText2_MaxLength),
            TrimEnd(local.SpDocLiteral.IdDocument));

          if (local.Position.Count <= 0)
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else
          {
            // mjr---> Determines the appropriate exitstate for the Print 
            // process
            local.WorkArea.Text50 = local.NextTranInfo.MiscText2 ?? Spaces(50);
            UseSpPrintDecodeReturnCode();
            local.NextTranInfo.MiscText2 = local.WorkArea.Text50;
          }
        }

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
          var field = GetField(export.Obligation, "systemGeneratedIdentifier");

          field.Error = true;

          MoveObligation(local.InitialisedObligation, export.Obligation);
        }
        else if (IsExitState("LE0000_CSE_PERSON_NOT_AN_OBLIGOR"))
        {
          var field1 = GetField(export.Obligation, "systemGeneratedIdentifier");

          field1.Error = true;

          var field2 = GetField(export.CsePersonsWorkSet, "number");

          field2.Error = true;
        }
        else if (IsExitState("LE0000_NO_OBLIGATION_FOUND"))
        {
          var field = GetField(export.Obligation, "systemGeneratedIdentifier");

          field.Error = true;
        }
        else if (IsExitState("LE0000_ADMIN_ACTION_NOT_AVAIL"))
        {
          // This is handled in the display action block
        }
        else if (IsExitState("LE0000_INVALID_ADMIN_ACT_TYPE"))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;
        }
        else if (IsExitState("FN0000_OBLIGATION_NF"))
        {
          var field = GetField(export.Obligation, "systemGeneratedIdentifier");

          field.Error = true;
        }
        else if (IsExitState("LE0000_ONLY_MANUAL_ADM_ACT_ALLWD"))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;
        }
        else
        {
        }

        if (IsExitState("LE0000_ADMIN_ACTION_NOT_AVAIL") && Equal
          (local.Previous.Command, "RETOPAY"))
        {
        }
        else
        {
          export.ObligationAdministrativeAction.Assign(
            local.InitialisedObligationAdministrativeAction);
          MoveObligation(local.InitialisedObligation, export.Obligation);
          export.AdminActionTakenBy.Assign(local.InitialisedServiceProvider);
          export.ObligationType.Assign(local.InitialisedObligationType);
        }
      }
    }

    // ---------------------------------------------
    // Move all exports to export hidden previous
    // ---------------------------------------------
    export.HiddenPrevLegalAction.Assign(export.LegalAction);
    MoveCsePersonsWorkSet1(export.CsePersonsWorkSet,
      export.HiddenPrevCsePersonsWorkSet);
    MoveAdministrativeAction(export.AdministrativeAction,
      export.HiddenPrevAdministrativeAction);
    MoveObligation(export.Obligation, export.HiddenPrevObligation);
    export.HiddenPrevObligationAdministrativeAction.Assign(
      export.ObligationAdministrativeAction);
    MoveFips(export.Fips, export.HiddenFips);
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
            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

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
            ExitState = "LE0000_INVALID_ADM_ACT_TAKN_DATE";

            var field9 =
              GetField(export.ObligationAdministrativeAction, "takenDate");

            field9.Error = true;

            break;
          case 10:
            ExitState = "LE0000_INVALID_ADM_ACT_RESP_DATE";

            var field10 =
              GetField(export.ObligationAdministrativeAction, "responseDate");

            field10.Error = true;

            break;
          case 11:
            ExitState = "LE0000_ADMIN_ACT_RESPONSE_REQD";

            var field11 =
              GetField(export.ObligationAdministrativeAction, "response");

            field11.Error = true;

            break;
          case 12:
            ExitState = "LE0000_CT_CASE_REQD_IF_ALL_OBL_Y";

            var field12 = GetField(export.AllObligations, "flag");

            field12.Error = true;

            break;
          case 13:
            ExitState = "LE0000_ONLY_MANUAL_ADM_ACT_ALLWD";

            var field13 = GetField(export.AdministrativeAction, "type1");

            field13.Error = true;

            break;
          case 14:
            ExitState = "LE0000_ACTION_TAKEN_DATE_REQD";

            var field14 =
              GetField(export.ObligationAdministrativeAction, "takenDate");

            field14.Error = true;

            break;
          case 15:
            ExitState = "LE0000_PERS_NOT_OBLIGR_CT_CASE";

            var field15 = GetField(export.CsePersonsWorkSet, "number");

            field15.Error = true;

            var field16 = GetField(export.LegalAction, "courtCaseNumber");

            field16.Error = true;

            break;
          default:
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

  private static void MoveErrorCodes(LeIadaValidateOblnAdminAct.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
  }

  private static void MoveExport1ToLocal1(CabMatchCsePerson.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.Detail.Assign(source.Detail);
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

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdAdminAction = source.IdAdminAction;
    target.IdDocument = source.IdDocument;
    target.IdObligationAdminAction = source.IdObligationAdminAction;
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

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeIadaCreateOblnAdminAction()
  {
    var useImport = new LeIadaCreateOblnAdminAction.Import();
    var useExport = new LeIadaCreateOblnAdminAction.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    MoveLegalAction3(export.LegalAction, useImport.LegalAction);
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.CreateForAllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ObligationAdministrativeAction.Assign(
      export.ObligationAdministrativeAction);

    Call(LeIadaCreateOblnAdminAction.Execute, useImport, useExport);

    local.Rollback.Flag = useExport.Rollback.Flag;
    export.ObligationType.Assign(useExport.ObligationType);
    export.AdminActionTakenBy.Assign(useExport.ActionTakenBy);
    export.ObligationAdministrativeAction.CreatedBy =
      useExport.ObligationAdministrativeAction.CreatedBy;
  }

  private void UseLeIadaDeleteOblnAdminAction()
  {
    var useImport = new LeIadaDeleteOblnAdminAction.Import();
    var useExport = new LeIadaDeleteOblnAdminAction.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.Foreign.Identifier = export.Tribunal.Identifier;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.DeleteForAllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ObligationAdministrativeAction.TakenDate =
      export.ObligationAdministrativeAction.TakenDate;
    MoveLegalAction3(export.LegalAction, useImport.LegalAction);

    Call(LeIadaDeleteOblnAdminAction.Execute, useImport, useExport);

    local.Rollback.Flag = useExport.Rollback.Flag;
  }

  private void UseLeIadaDisplayOblnAdminAct()
  {
    var useImport = new LeIadaDisplayOblnAdminAct.Import();
    var useExport = new LeIadaDisplayOblnAdminAct.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Foreign.Identifier = export.Tribunal.Identifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.DisplayAnyObligation.Flag = local.DisplayAnyObligation.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ObligationAdministrativeAction.Assign(
      export.ObligationAdministrativeAction);
    MoveLegalAction3(export.LegalAction, useImport.LegalAction);

    Call(LeIadaDisplayOblnAdminAct.Execute, useImport, useExport);

    local.MoreAdminAct.Flag = useExport.MoreAdminAct.Flag;
    export.Foreign.Country = useExport.Foreign.Country;
    export.Tribunal.Assign(useExport.Tribunal);
    export.Fips.Assign(useExport.Fips);
    export.ObligationType.Assign(useExport.ObligationType);
    export.AdminActionTakenBy.Assign(useExport.ActionTakenBy);
    export.LegalAction.Assign(useExport.LegalAction);
    export.ObligationAdministrativeAction.Assign(
      useExport.ObligationAdministrativeAction);
    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AdministrativeAction);
    MoveObligation(useExport.Obligation, export.Obligation);
    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeIadaUpdateOblnAdminAction()
  {
    var useImport = new LeIadaUpdateOblnAdminAction.Import();
    var useExport = new LeIadaUpdateOblnAdminAction.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    MoveFips(export.Fips, useImport.Fips);
    useImport.Foreign.Identifier = export.Tribunal.Identifier;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.UpdateForAllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ObligationAdministrativeAction.Assign(
      export.ObligationAdministrativeAction);
    MoveLegalAction3(export.LegalAction, useImport.LegalAction);

    Call(LeIadaUpdateOblnAdminAction.Execute, useImport, useExport);

    local.Rollback.Flag = useExport.Rollback.Flag;
  }

  private void UseLeIadaValidateOblnAdminAct()
  {
    var useImport = new LeIadaValidateOblnAdminAct.Import();
    var useExport = new LeIadaValidateOblnAdminAct.Export();

    useImport.ObligationType.Assign(export.ObligationType);
    MoveFips(export.Fips, useImport.Fips);
    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.AdministrativeAction.Type1 = export.AdministrativeAction.Type1;
    useImport.AllObligation.Flag = export.AllObligations.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ObligationAdministrativeAction.Assign(
      export.ObligationAdministrativeAction);
    MoveLegalAction3(export.LegalAction, useImport.LegalAction);

    Call(LeIadaValidateOblnAdminAct.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(local.NextTranInfo);
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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    MoveLegalAction2(import.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.Case1.Number = local.Case1.Number;
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.Specific.ActionReasonCode = local.Specific.ActionReasonCode;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveLegalAction1(export.LegalAction, useImport.LegalAction);

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
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

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionPerson.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.LegalActionPerson.CspNumber ?? "");
        db.SetInt32(command, "lgaId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
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

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetNullableString(
          command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionTribunal()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
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
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 7);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 8);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 9);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionTribunalFips1()
  {
    entities.ExistingFips.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionTribunalFips1",
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
        entities.ExistingFips.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionTribunalFips2()
  {
    entities.ExistingFips.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionTribunalFips2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
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
        entities.ExistingFips.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;
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
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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
    /// A value of AdminActionTakenBy.
    /// </summary>
    [JsonPropertyName("adminActionTakenBy")]
    public ServiceProvider AdminActionTakenBy
    {
      get => adminActionTakenBy ??= new();
      set => adminActionTakenBy = value;
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
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
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
    /// A value of HiddenPrevObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevObligationAdministrativeAction")]
    public ObligationAdministrativeAction HiddenPrevObligationAdministrativeAction
      
    {
      get => hiddenPrevObligationAdministrativeAction ??= new();
      set => hiddenPrevObligationAdministrativeAction = value;
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

    private FipsTribAddress foreign;
    private Document print;
    private Standard standard;
    private Standard promptTribunal;
    private Fips fips;
    private Tribunal tribunal;
    private Fips hiddenFips;
    private ServiceProvider adminActionTakenBy;
    private ObligationType obligationType;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private CodeValue selected;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private Common promptObligationNbr;
    private Obligation obligation;
    private Common allObligations;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private AdministrativeAction administrativeAction;
    private Common promptAdminActionType;
    private Obligation hiddenPrevObligation;
    private ObligationAdministrativeAction hiddenPrevObligationAdministrativeAction;
      
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private LegalAction hiddenPrevLegalAction;
    private AdministrativeAction hiddenPrevAdministrativeAction;
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
    /// A value of DocmProtectFilter.
    /// </summary>
    [JsonPropertyName("docmProtectFilter")]
    public Common DocmProtectFilter
    {
      get => docmProtectFilter ??= new();
      set => docmProtectFilter = value;
    }

    /// <summary>
    /// A value of DlgflwAsinObject.
    /// </summary>
    [JsonPropertyName("dlgflwAsinObject")]
    public SpTextWorkArea DlgflwAsinObject
    {
      get => dlgflwAsinObject ??= new();
      set => dlgflwAsinObject = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCsePerson.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCsePerson")]
    public CsePerson DlgflwSelectedCsePerson
    {
      get => dlgflwSelectedCsePerson ??= new();
      set => dlgflwSelectedCsePerson = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCsePersonAccount")]
    public CsePersonAccount DlgflwSelectedCsePersonAccount
    {
      get => dlgflwSelectedCsePersonAccount ??= new();
      set => dlgflwSelectedCsePersonAccount = value;
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
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
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
    /// A value of ListOptSystManual.
    /// </summary>
    [JsonPropertyName("listOptSystManual")]
    public Standard ListOptSystManual
    {
      get => listOptSystManual ??= new();
      set => listOptSystManual = value;
    }

    /// <summary>
    /// A value of AdminActionTakenBy.
    /// </summary>
    [JsonPropertyName("adminActionTakenBy")]
    public ServiceProvider AdminActionTakenBy
    {
      get => adminActionTakenBy ??= new();
      set => adminActionTakenBy = value;
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
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
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
    /// A value of HiddenPrevObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevObligationAdministrativeAction")]
    public ObligationAdministrativeAction HiddenPrevObligationAdministrativeAction
      
    {
      get => hiddenPrevObligationAdministrativeAction ??= new();
      set => hiddenPrevObligationAdministrativeAction = value;
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

    private Common docmProtectFilter;
    private SpTextWorkArea dlgflwAsinObject;
    private CsePerson dlgflwSelectedCsePerson;
    private CsePersonAccount dlgflwSelectedCsePersonAccount;
    private FipsTribAddress foreign;
    private Document print;
    private Standard standard;
    private Standard promptTribunal;
    private Fips fips;
    private Tribunal tribunal;
    private Fips hiddenFips;
    private Standard listOptSystManual;
    private ServiceProvider adminActionTakenBy;
    private ObligationType obligationType;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Code required;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private Common promptObligationNbr;
    private Obligation obligation;
    private Common allObligations;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private AdministrativeAction administrativeAction;
    private Common promptAdminActionType;
    private Obligation hiddenPrevObligation;
    private ObligationAdministrativeAction hiddenPrevObligationAdministrativeAction;
      
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private LegalAction hiddenPrevLegalAction;
    private AdministrativeAction hiddenPrevAdministrativeAction;
    private Security2 hiddenSecurity;
    private SsnWorkArea ssnWorkArea;
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
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
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

      private CsePersonsWorkSet detail;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// A value of Specific.
    /// </summary>
    [JsonPropertyName("specific")]
    public InterstateRequestHistory Specific
    {
      get => specific ??= new();
      set => specific = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public LegalAction Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of InitialisedFipsTribAddress.
    /// </summary>
    [JsonPropertyName("initialisedFipsTribAddress")]
    public FipsTribAddress InitialisedFipsTribAddress
    {
      get => initialisedFipsTribAddress ??= new();
      set => initialisedFipsTribAddress = value;
    }

    /// <summary>
    /// A value of InitialisedFips.
    /// </summary>
    [JsonPropertyName("initialisedFips")]
    public Fips InitialisedFips
    {
      get => initialisedFips ??= new();
      set => initialisedFips = value;
    }

    /// <summary>
    /// A value of InitialisedTribunal.
    /// </summary>
    [JsonPropertyName("initialisedTribunal")]
    public Tribunal InitialisedTribunal
    {
      get => initialisedTribunal ??= new();
      set => initialisedTribunal = value;
    }

    /// <summary>
    /// A value of InitialisedObligationType.
    /// </summary>
    [JsonPropertyName("initialisedObligationType")]
    public ObligationType InitialisedObligationType
    {
      get => initialisedObligationType ??= new();
      set => initialisedObligationType = value;
    }

    /// <summary>
    /// A value of InitialisedPetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("initialisedPetitionerRespondentDetails")]
    public PetitionerRespondentDetails InitialisedPetitionerRespondentDetails
    {
      get => initialisedPetitionerRespondentDetails ??= new();
      set => initialisedPetitionerRespondentDetails = value;
    }

    /// <summary>
    /// A value of MoreAdminAct.
    /// </summary>
    [JsonPropertyName("moreAdminAct")]
    public Common MoreAdminAct
    {
      get => moreAdminAct ??= new();
      set => moreAdminAct = value;
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
    /// A value of InitialisedObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("initialisedObligationAdministrativeAction")]
    public ObligationAdministrativeAction InitialisedObligationAdministrativeAction
      
    {
      get => initialisedObligationAdministrativeAction ??= new();
      set => initialisedObligationAdministrativeAction = value;
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

    private Common previous;
    private Case1 case1;
    private Common screenIdentification;
    private InterstateRequestHistory specific;
    private SpDocLiteral spDocLiteral;
    private Common position;
    private WorkArea workArea;
    private LegalAction initialized;
    private FipsTribAddress initialisedFipsTribAddress;
    private Fips initialisedFips;
    private Tribunal initialisedTribunal;
    private ObligationType initialisedObligationType;
    private PetitionerRespondentDetails initialisedPetitionerRespondentDetails;
    private Common moreAdminAct;
    private Common rollback;
    private Common promptCount;
    private TextWorkArea textWorkArea;
    private ServiceProvider initialisedServiceProvider;
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
    private ObligationAdministrativeAction initialisedObligationAdministrativeAction;
      
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
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

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionCaseRole legalActionCaseRole;
    private Case1 case1;
    private LegalActionPerson legalActionPerson;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private LegalAction existingLegalAction;
  }
#endregion
}

// Program: LE_IWGL_IWO_GARNISHMENT_LIEN, ID: 372028498, model: 746.
// Short name: SWEIWGLP
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
/// A program: LE_IWGL_IWO_GARNISHMENT_LIEN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step maintains IWO, Garnishment and Liens (
/// LEGAL_ACTION_INCOME_SOURCE and LEGAL ACTION_PERSON_RESOURCE)
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeIwglIwoGarnishmentLien: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_IWGL_IWO_GARNISHMENT_LIEN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeIwglIwoGarnishmentLien(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeIwglIwoGarnishmentLien.
  /// </summary>
  public LeIwglIwoGarnishmentLien(IContext context, Import import, Export export)
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
    // *********************************************************************************************************
    // Developer	Date	    Ch Req#	Description
    // -------------   ---------   ----------  
    // ----------------------------------------------------------------
    // Govind		11/20/96    IDCR244	Effective date is made optional.
    // Govind		06/28/97    IDCR342	Identifier changed or Legal Action Income 
    // Source and
    // 					Legal Action Person Resource. Effective Date is no
    // 					longer a part of the identifier in both.
    // Siraj Konkader  8/13/97			Changed view match to Print action block. Prob 
    // Rpt 25101,
    // 					design change, want county name instead of tribunal name.
    // M Ramirez	12/30/1998		Removed print process.  Changed security to check 
    // for
    // 					CRUD actions only. Removed Cases of "Enter" and
    // 					"Invalid" from main case of command.  These will be
    // 					handled in Case of "Otherwise"
    // J Magat		01/07/2000  PR 83723	Correct NF condition when displaying "G" 
    // class legal action.
    // C. Scroggins    04/14/2000  WR 00162	Added check for family violence 
    // indicator on CSE Person.
    // GVandy		09/08/2000  PR 100875	Not passing the correct identifier when 
    // deleting Garnishments.
    // GVandy		10/15/2001  PR 129640	Correct Prev and Next logic.
    // GVandy		11/27/2002  PR 164426	Modify exit state message and highlighting 
    // when multiple
    // 					obligors on the court case.
    // G. Pan		09/12/2007  PR197954	Added legal_action classification = "O" to 
    // two
    // 					Read Each statements in the edit section.
    // GVandy		06/09/2015  CQ22212	Do not allow updates and deletes to IWO, 
    // IWOMODO,
    // 					ORDIWO2, IWOTERM, ORDIWOLS, or ORDIWOPT legal actions.
    // GVandy		11/17/2015  CQ50342	Allow deletes to IWO, IWOMODO, ORDIWO2, 
    // ORDIWOLS,
    // 					and ORDIWOPT legal actions if there are no ties to
    // 					LAIS.  Also, allow updates to the end date for
    // 					these legal actions.
    // *********************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      var field1 = GetField(export.CsePersonsWorkSet, "number");

      field1.Color = "green";
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

      field2.Color = "green";
      field2.Highlighting = Highlighting.Underscore;
      field2.Protected = false;

      var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

      field3.Color = "green";
      field3.Highlighting = Highlighting.Underscore;
      field3.Protected = false;

      var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

      field4.Color = "green";
      field4.Highlighting = Highlighting.Underscore;
      field4.Protected = false;

      var field5 = GetField(export.LegalActionIncomeSource, "withholdingType");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.PromptIwoType, "selectChar");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.LegalActionIncomeSource, "wageOrNonWage");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.PromptIncomeSource, "selectChar");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.PromptLienResourceDesc, "selectChar");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.LegalActionIncomeSource, "effectiveDate");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.LegalActionIncomeSource, "endDate");

      field11.Color = "cyan";
      field11.Protected = true;

      return;
    }

    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    MoveCsePersonResource2(import.CsePersonResource, export.CsePersonResource);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (import.SsnWorkArea.SsnNumPart1 > 0)
    {
      MoveSsnWorkArea2(import.SsnWorkArea, local.SsnWorkArea);
      local.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = local.SsnWorkArea.SsnText9;
    }
    else
    {
      export.CsePersonsWorkSet.Ssn = "";
    }

    local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
    UseEabPadLeftWithZeros();
    export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    MoveIncomeSource(import.IncomeSource, export.IncomeSource);
    MoveLegalAction1(import.LegalAction, export.LegalAction);
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.Foreign.Country = import.Foreign.Country;
    export.LegalActionIncomeSource.Assign(import.LegalActionIncomeSource);
    export.LegalActionPersonResource.Assign(import.LegalActionPersonResource);
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenFips.Assign(import.HiddenFips);
    MoveIncomeSource(import.HiddenIncomeSource, export.HiddenIncomeSource);
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    export.HiddenPrevCsePersonsWorkSet.Number =
      import.HiddenPrevCsePersonsWorkSet.Number;
    MoveCsePersonResource2(import.HiddenLien, export.HiddenLien);
    MoveCsePersonResource2(import.HiddenLien, export.HiddenLien);
    export.HiddenPrevLegalActionIncomeSource.Assign(
      import.HiddenPrevLegalActionIncomeSource);
    export.PromptTribuType.SelectChar = import.PromptTribuType.SelectChar;
    export.PromptIncomeSource.SelectChar = import.PromptIncomeSource.SelectChar;
    export.PromptIwoType.SelectChar = import.PromptIwoType.SelectChar;
    export.PromptLienResourceDesc.SelectChar =
      import.PromptLienResourceDesc.SelectChar;
    export.PromptLienType.SelectChar = import.PromptLienType.SelectChar;
    export.IwglType.Text1 = import.IwglType.Text1;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);

    if (AsChar(export.IwglType.Text1) == 'I')
    {
      var field1 = GetField(export.LegalActionIncomeSource, "wageOrNonWage");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.PromptLienResourceDesc, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.LegalActionIncomeSource, "withholdingType");

      field3.Color = "green";
      field3.Protected = false;

      var field4 = GetField(export.PromptIwoType, "selectChar");

      field4.Color = "green";
      field4.Protected = false;
    }
    else if (AsChar(export.IwglType.Text1) == 'G')
    {
      var field1 = GetField(export.LegalActionIncomeSource, "wageOrNonWage");

      field1.Color = "green";
      field1.Protected = false;

      var field2 = GetField(export.PromptLienResourceDesc, "selectChar");

      field2.Color = "green";
      field2.Protected = false;

      var field3 = GetField(export.LegalActionIncomeSource, "withholdingType");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.PromptIwoType, "selectChar");

      field4.Color = "cyan";
      field4.Protected = true;
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
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (export.LegalAction.Identifier != 0)
      {
        if (ReadLegalAction2())
        {
          MoveLegalAction3(entities.ExistingLegalAction, export.LegalAction);
          export.IwglType.Text1 = entities.ExistingLegalAction.Classification;

          if (AsChar(export.IwglType.Text1) == 'I')
          {
            var field1 =
              GetField(export.LegalActionIncomeSource, "wageOrNonWage");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.CsePersonResource, "resourceDescription");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.PromptLienResourceDesc, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field4.Color = "green";
            field4.Protected = false;

            var field5 = GetField(export.PromptIwoType, "selectChar");

            field5.Color = "green";
            field5.Protected = false;
          }
          else
          {
            var field1 =
              GetField(export.LegalActionIncomeSource, "wageOrNonWage");

            field1.Color = "green";
            field1.Protected = false;

            var field2 =
              GetField(export.CsePersonResource, "resourceDescription");

            field2.Color = "green";
            field2.Protected = false;

            var field3 = GetField(export.PromptLienResourceDesc, "selectChar");

            field3.Color = "green";
            field3.Protected = false;

            var field4 =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.PromptIwoType, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (ReadTribunal())
          {
            export.Tribunal.Assign(entities.ExistingTribunal);

            if (ReadFips())
            {
              export.Fips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress3())
            {
              export.Foreign.Country = entities.FipsTribAddress.Country;
            }
          }
        }
      }

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
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      UseScCabNextTranPut();

      return;
    }

    // mjr
    // ------------------------------------------------
    // 12/30/1998
    // Changed security to check CRUD actions only
    // Also matched Legal Action and CSE Person Workset to security cab
    // -------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    if (Equal(global.Command, "RETLACN") || Equal
      (global.Command, "RETLAPS") || Equal(global.Command, "FROMLACN"))
    {
      global.Command = "DISPLAY";
      local.UserAction.Command = global.Command;

      if (export.LegalAction.Identifier != 0)
      {
        if (ReadLegalAction2())
        {
          MoveLegalAction3(entities.ExistingLegalAction, export.LegalAction);
          MoveLegalAction4(export.LegalAction, export.HiddenLegalAction);
          export.IwglType.Text1 = entities.ExistingLegalAction.Classification;

          if (AsChar(export.IwglType.Text1) == 'I')
          {
            var field1 =
              GetField(export.LegalActionIncomeSource, "wageOrNonWage");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.PromptLienResourceDesc, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field3.Color = "green";
            field3.Protected = false;

            var field4 = GetField(export.PromptIwoType, "selectChar");

            field4.Color = "green";
            field4.Protected = false;
          }
          else if (AsChar(export.IwglType.Text1) == 'G')
          {
            var field1 =
              GetField(export.LegalActionIncomeSource, "wageOrNonWage");

            field1.Color = "green";
            field1.Protected = false;

            var field2 = GetField(export.PromptLienResourceDesc, "selectChar");

            field2.Color = "green";
            field2.Protected = false;

            var field3 =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.PromptIwoType, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          UseLeGetPetitionerRespondent();

          if (ReadTribunal())
          {
            export.Tribunal.Assign(entities.ExistingTribunal);

            if (ReadFips())
            {
              export.Fips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress3())
            {
              export.Foreign.Country = entities.FipsTribAddress.Country;
            }
          }
        }
      }
    }
    else
    {
      local.UserAction.Command = global.Command;

      if (Equal(global.Command, "PREV"))
      {
        if (AsChar(export.ScrollingAttributes.MinusFlag) == '-')
        {
          global.Command = "DISPLAY";
        }
        else
        {
          ExitState = "LE0000_NO_PREV_IWGL";

          return;
        }
      }

      if (Equal(global.Command, "NEXT"))
      {
        if (AsChar(export.ScrollingAttributes.PlusFlag) == '+')
        {
          global.Command = "DISPLAY";
        }
        else
        {
          ExitState = "LE0000_NO_MORE_IWGL_RECORD";

          return;
        }
      }
    }

    // ---------------------------------------------
    //             E D I T    L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RLTRIB") || Equal(global.Command, "RETINCL") || Equal
      (global.Command, "RETRESL") || Equal(global.Command, "SIGNOFF"))
    {
    }
    else
    {
      if (IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        if (Equal(global.Command, "DISPLAY"))
        {
          ExitState = "ECO_LNK_TO_LST_LGL_ACT_BY_CP";

          return;
        }
        else
        {
          var field12 = GetField(export.CsePersonsWorkSet, "number");

          field12.Color = "green";
          field12.Highlighting = Highlighting.Underscore;
          field12.Protected = false;
          field12.Focused = true;

          var field13 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field13.Color = "green";
          field13.Highlighting = Highlighting.Underscore;
          field13.Protected = false;

          var field14 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field14.Color = "green";
          field14.Highlighting = Highlighting.Underscore;
          field14.Protected = false;

          var field15 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field15.Color = "green";
          field15.Highlighting = Highlighting.Underscore;
          field15.Protected = false;

          var field16 =
            GetField(export.LegalActionIncomeSource, "withholdingType");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.PromptIwoType, "selectChar");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 =
            GetField(export.LegalActionIncomeSource, "wageOrNonWage");

          field18.Color = "cyan";
          field18.Protected = true;

          var field19 = GetField(export.PromptIncomeSource, "selectChar");

          field19.Color = "cyan";
          field19.Protected = true;

          var field20 = GetField(export.PromptLienResourceDesc, "selectChar");

          field20.Color = "cyan";
          field20.Protected = true;

          var field21 =
            GetField(export.LegalActionIncomeSource, "effectiveDate");

          field21.Color = "cyan";
          field21.Protected = true;

          var field22 = GetField(export.LegalActionIncomeSource, "endDate");

          field22.Color = "cyan";
          field22.Protected = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }
      }

      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        if (IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          if (export.LegalAction.Identifier != 0)
          {
            local.LastReadObligor.Number = "";

            foreach(var item in ReadLegalActionLegalActionDetailLegalActionPerson())
              
            {
              if (IsEmpty(local.LastReadObligor.Number))
              {
                local.LastReadObligor.Number =
                  entities.ExistingCsePerson.Number;
              }

              if (Equal(local.LastReadObligor.Number,
                entities.ExistingCsePerson.Number))
              {
                continue;
              }
              else
              {
                // @@@
                var field12 = GetField(export.CsePersonsWorkSet, "number");

                field12.Color = "green";
                field12.Highlighting = Highlighting.ReverseVideo;
                field12.Protected = false;
                field12.Focused = true;

                var field13 = GetField(export.SsnWorkArea, "ssnNumPart1");

                field13.Color = "green";
                field13.Highlighting = Highlighting.ReverseVideo;
                field13.Protected = false;
                field13.Focused = false;

                var field14 = GetField(export.SsnWorkArea, "ssnNumPart2");

                field14.Color = "green";
                field14.Highlighting = Highlighting.ReverseVideo;
                field14.Protected = false;
                field14.Focused = false;

                var field15 = GetField(export.SsnWorkArea, "ssnNumPart3");

                field15.Color = "green";
                field15.Highlighting = Highlighting.ReverseVideo;
                field15.Protected = false;
                field15.Focused = false;

                ExitState = "LE0000_MORE_THAN_1_OBLR_EXIST";

                return;
              }
            }

            if (IsEmpty(local.LastReadObligor.Number))
            {
              // ********************************************
              // RCG - H00031886 - 11/12/97
              // Previously this condition caused the program to ESCAPE and 
              // return the exit state to the screen.
              // Instead should attempt to identify Obligor on Class I or Class 
              // G actions with no required LOPS obligor person by identifying
              // Obligor for most recent JE or I class legal action for this
              // specified Court Case Number.
              // Removed Exit State is LE000_no_obligor_in_lact and
              // Make export cse_persons_work_set number Error statements
              // ********************************************
              goto Test1;
            }

            export.CsePersonsWorkSet.Number = local.LastReadObligor.Number;

            goto Test2;
          }

Test1:

          // --- Legal Action not identified yet. So try checking all the legal 
          // actions for the court case.
          foreach(var item in ReadLegalActionTribunalLegalActionDetailLegalActionPerson())
            
          {
            if (Lt(entities.LegalActionDetail.EndDate, Now().Date))
            {
              continue;
            }

            if (ReadFips())
            {
              if (!Equal(entities.ExistingFips.StateAbbreviation,
                export.Fips.StateAbbreviation) || !
                Equal(entities.ExistingFips.CountyAbbreviation,
                export.Fips.CountyAbbreviation))
              {
                continue;
              }
            }
            else
            {
              // --- It is a foreign tribunal
              if (entities.ExistingTribunal.Identifier != export
                .Tribunal.Identifier)
              {
                continue;
              }
            }

            if (IsEmpty(local.LastReadObligor.Number))
            {
              local.LastReadObligor.Number = entities.ExistingCsePerson.Number;
            }

            if (Equal(local.LastReadObligor.Number,
              entities.ExistingCsePerson.Number))
            {
              continue;
            }
            else
            {
              // @@@
              var field12 = GetField(export.CsePersonsWorkSet, "number");

              field12.Color = "green";
              field12.Highlighting = Highlighting.ReverseVideo;
              field12.Protected = false;
              field12.Focused = true;

              var field13 = GetField(export.SsnWorkArea, "ssnNumPart1");

              field13.Color = "green";
              field13.Highlighting = Highlighting.ReverseVideo;
              field13.Protected = false;
              field13.Focused = false;

              var field14 = GetField(export.SsnWorkArea, "ssnNumPart2");

              field14.Color = "green";
              field14.Highlighting = Highlighting.ReverseVideo;
              field14.Protected = false;
              field14.Focused = false;

              var field15 = GetField(export.SsnWorkArea, "ssnNumPart3");

              field15.Color = "green";
              field15.Highlighting = Highlighting.ReverseVideo;
              field15.Protected = false;
              field15.Focused = false;

              ExitState = "LE0000_MORE_THAN_1_OBLR_EXIST";

              return;
            }
          }

          if (IsEmpty(local.LastReadObligor.Number))
          {
            ExitState = "LE0000_NO_OBLR_IN_CT_CASE";

            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }

          export.CsePersonsWorkSet.Number = local.LastReadObligor.Number;
        }
      }

Test2:

      if (IsEmpty(export.Fips.StateAbbreviation) && export
        .Tribunal.Identifier == 0)
      {
        var field = GetField(export.Fips, "stateAbbreviation");

        field.Color = "red";
        field.Intensity = Intensity.High;
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      if (IsEmpty(export.Fips.CountyAbbreviation) && export
        .Tribunal.Identifier == 0)
      {
        var field = GetField(export.Fips, "countyAbbreviation");

        field.Color = "red";
        field.Intensity = Intensity.High;
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      local.Save.Assign(export.CsePersonsWorkSet);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        UseSiReadCsePerson();
        local.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();
      }
      else if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
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

          return;
        }

        UseSiFormatCsePersonName();
      }
      else
      {
        MoveCsePersonsWorkSet1(local.Save, export.CsePersonsWorkSet);
        ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

        return;
      }

      // ---------------------------------------------------------------------------------------
      // CLS 04/14/00 - Added check for family violence.
      // ---------------------------------------------------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseScSecurityCheckForFv();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      foreach(var item in ReadLegalActionTribunalLegalActionDetailLegalActionPerson())
        
      {
        local.LastReadObligor.Number = entities.ExistingCsePerson.Number;
        local.Test.Number = local.LastReadObligor.Number;

        if (Equal(export.CsePersonsWorkSet.Number, local.Test.Number))
        {
          goto Test3;
        }
      }

      ExitState = "LE0000_CSE_PERSON_NOT_AN_OBLIGOR";

      var field1 = GetField(export.CsePersonsWorkSet, "number");

      field1.Color = "red";
      field1.Intensity = Intensity.High;
      field1.Highlighting = Highlighting.ReverseVideo;
      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.SsnWorkArea, "ssnNumPart1");

      field2.Color = "red";
      field2.Intensity = Intensity.High;
      field2.Highlighting = Highlighting.ReverseVideo;
      field2.Protected = false;

      var field3 = GetField(export.SsnWorkArea, "ssnNumPart2");

      field3.Color = "red";
      field3.Intensity = Intensity.High;
      field3.Highlighting = Highlighting.ReverseVideo;
      field3.Protected = false;

      var field4 = GetField(export.SsnWorkArea, "ssnNumPart3");

      field4.Color = "red";
      field4.Intensity = Intensity.High;
      field4.Highlighting = Highlighting.ReverseVideo;
      field4.Protected = false;

      var field5 = GetField(export.LegalActionIncomeSource, "withholdingType");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.PromptIwoType, "selectChar");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.LegalActionIncomeSource, "wageOrNonWage");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.PromptIncomeSource, "selectChar");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.PromptLienResourceDesc, "selectChar");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.LegalActionIncomeSource, "effectiveDate");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.LegalActionIncomeSource, "endDate");

      field11.Color = "cyan";
      field11.Protected = true;

      return;
    }

Test3:

    // -- GVandy  11/17/2015  CQ50342  Allow deletes to IWO, IWOMODO, ORDIWO2, 
    // ORDIWOLS,
    //    and ORDIWOPT legal actions if there are no ties to LAIS.  Also, allow 
    // updates to
    //    the end date for these legal actions.
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (ReadLegalAction1())
      {
        if (Equal(entities.LegalAction.ActionTaken, "IWO") || Equal
          (entities.LegalAction.ActionTaken, "IWOMODO") || Equal
          (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
          (entities.LegalAction.ActionTaken, "IWOTERM") || Equal
          (entities.LegalAction.ActionTaken, "ORDIWOLS") || Equal
          (entities.LegalAction.ActionTaken, "ORDIWOPT"))
        {
          if (!ReadIncomeSource())
          {
            goto Test4;
          }

          foreach(var item in ReadIwoTransaction())
          {
            if (Equal(global.Command, "DELETE"))
            {
              // -- GVandy 11/17/2015 CQ50342  Allow deletes to IWO, IWOMODO, 
              // ORDIWO2, ORDIWOLS,
              //    and ORDIWOPT legal actions if there are no ties to LAIS.
              var field = GetField(export.LegalAction, "actionTaken");

              field.Error = true;

              ExitState = "LE0000_IWO_OR_EIWO_STOPS_DELETE";

              return;
            }

            if (Equal(global.Command, "UPDATE"))
            {
              // -- GVandy 11/17/2015 CQ50342  Allow allow updates to the end 
              // date for IWO, IWOMODO,
              //    ORDIWO2, ORDIWOLS, and ORDIWOPT legal actions.
              if (!Equal(export.LegalActionIncomeSource.EffectiveDate,
                export.HiddenPrevLegalActionIncomeSource.EffectiveDate))
              {
                var field =
                  GetField(export.LegalActionIncomeSource, "effectiveDate");

                field.Error = true;

                ExitState = "LE0000_ONLY_UPDATE_END_DATE";
              }

              if (!Equal(export.LegalActionIncomeSource.WithholdingType,
                export.HiddenPrevLegalActionIncomeSource.WithholdingType))
              {
                var field =
                  GetField(export.LegalActionIncomeSource, "withholdingType");

                field.Error = true;

                ExitState = "LE0000_ONLY_UPDATE_END_DATE";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }
          }
        }
      }
    }

Test4:

    // ---------------------------------------------
    // Based on which entity is being created, check
    // the mandatory fields for being present
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // --- Edit for mandatory effective date has been removed from here per 
      // IDCR 244.
      if (Lt(local.InitialisedToZeros.Date,
        export.LegalActionIncomeSource.EndDate))
      {
        if (Lt(export.LegalActionIncomeSource.EndDate,
          export.LegalActionIncomeSource.EffectiveDate))
        {
          var field = GetField(export.LegalActionIncomeSource, "endDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_END_LESS_THAN_EFF";
          }
        }
      }

      switch(AsChar(import.IwglType.Text1))
      {
        case 'I':
          if (IsEmpty(export.LegalActionIncomeSource.WithholdingType))
          {
            var field =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }

          local.Code.CodeName = "LEGAL ACTION IWO TYPE";
          local.CodeValue.Cdvalue =
            export.LegalActionIncomeSource.WithholdingType;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_INVALID_IWO_TYPE";
            }
          }

          if (!IsEmpty(export.LegalActionIncomeSource.WageOrNonWage))
          {
            var field =
              GetField(export.LegalActionIncomeSource, "wageOrNonWage");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_GARN_TYPE_MUST_BE_BLANK";
            }
          }

          break;
        case 'G':
          if (!IsEmpty(export.LegalActionIncomeSource.WithholdingType))
          {
            var field =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_IWO_WHLD_TYPE_MUST_BE_BLK";
            }
          }

          switch(AsChar(export.LegalActionIncomeSource.WageOrNonWage))
          {
            case 'W':
              break;
            case 'N':
              break;
            default:
              var field =
                GetField(export.LegalActionIncomeSource, "wageOrNonWage");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }

              break;
          }

          break;
        case 'L':
          // * 01/12/99 
          // *******************************************
          // D. Jean *
          // * Per Jan Brigham, the functionality of this procedure that
          // * allows a Lien to be added or updated should not be
          // * enabled, yet do not remove existing logic.  Later it may be 
          // activated.
          // ****************************************************************
          var field1 = GetField(export.IwglType, "text1");

          field1.Color = "red";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = true;

          ExitState = "LE0000_INVALID_IWO_GARN_LIEN_TYP";

          return;

          // * 01/12/99 
          // *******************************************
          // D. Jean *
          if (!IsEmpty(export.LegalActionIncomeSource.WithholdingType))
          {
            var field =
              GetField(export.LegalActionIncomeSource, "withholdingType");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_IWO_WHLD_TYPE_MUST_BE_BLK";
            }
          }

          if (!IsEmpty(export.LegalActionIncomeSource.WageOrNonWage))
          {
            var field =
              GetField(export.LegalActionIncomeSource, "wageOrNonWage");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_GARN_TYPE_MUST_BE_BLANK";
            }
          }

          export.LegalActionPersonResource.EffectiveDate =
            export.LegalActionIncomeSource.EffectiveDate;
          export.LegalActionPersonResource.EndDate =
            export.LegalActionIncomeSource.EndDate;
          local.Code.CodeName = "LEGAL ACTION LIEN TYPE";
          local.CodeValue.Cdvalue =
            export.LegalActionPersonResource.LienType ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_INVALID_LIEN_TYPE";
            }
          }

          break;
        default:
          var field2 = GetField(export.IwglType, "text1");

          field2.Color = "red";
          field2.Intensity = Intensity.High;
          field2.Highlighting = Highlighting.ReverseVideo;
          field2.Protected = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_INVALID_IWO_GARN_LIEN_TYP";
          }

          break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      if (export.LegalAction.Identifier == 0)
      {
        local.NoOfLegalActions.Count = 0;

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          if (export.Tribunal.Identifier == 0)
          {
            ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

            return;
          }
          else
          {
            foreach(var item in ReadLegalActionTribunal())
            {
              ++local.NoOfLegalActions.Count;

              if (local.NoOfLegalActions.Count > 1)
              {
                export.HiddenPrevUserAction.Command = global.Command;
                export.LegalAction.Classification = "";
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              MoveLegalAction3(entities.ExistingLegalAction, export.LegalAction);
                
              export.Tribunal.Assign(entities.ExistingTribunal);
              export.IwglType.Text1 =
                entities.ExistingLegalAction.Classification;

              if (AsChar(export.IwglType.Text1) == 'I')
              {
                var field1 =
                  GetField(export.LegalActionIncomeSource, "wageOrNonWage");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.PromptLienResourceDesc, "selectChar");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.LegalActionIncomeSource, "withholdingType");

                field3.Color = "green";
                field3.Protected = false;

                var field4 = GetField(export.PromptIwoType, "selectChar");

                field4.Color = "green";
                field4.Protected = false;
              }
              else if (AsChar(export.IwglType.Text1) == 'G')
              {
                var field1 =
                  GetField(export.LegalActionIncomeSource, "wageOrNonWage");

                field1.Color = "green";
                field1.Protected = false;

                var field2 =
                  GetField(export.PromptLienResourceDesc, "selectChar");

                field2.Color = "green";
                field2.Protected = false;

                var field3 =
                  GetField(export.LegalActionIncomeSource, "withholdingType");

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 = GetField(export.PromptIwoType, "selectChar");

                field4.Color = "cyan";
                field4.Protected = true;
              }

              if (ReadFipsTribAddress2())
              {
                export.Foreign.Country = entities.FipsTribAddress.Country;
              }
            }
          }
        }
        else
        {
          foreach(var item in ReadLegalActionTribunalFips())
          {
            ++local.NoOfLegalActions.Count;

            if (local.NoOfLegalActions.Count > 1)
            {
              export.HiddenPrevUserAction.Command = global.Command;
              export.LegalAction.Classification = "";
              ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

              return;
            }

            MoveLegalAction3(entities.ExistingLegalAction, export.LegalAction);
            export.Fips.Assign(entities.ExistingFips);
            export.Tribunal.Assign(entities.ExistingTribunal);
            export.IwglType.Text1 = entities.ExistingLegalAction.Classification;

            if (AsChar(export.IwglType.Text1) == 'I')
            {
              var field1 =
                GetField(export.LegalActionIncomeSource, "wageOrNonWage");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.PromptLienResourceDesc, "selectChar");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.LegalActionIncomeSource, "withholdingType");

              field3.Color = "green";
              field3.Protected = false;

              var field4 = GetField(export.PromptIwoType, "selectChar");

              field4.Color = "green";
              field4.Protected = false;
            }
            else if (AsChar(export.IwglType.Text1) == 'G')
            {
              var field1 =
                GetField(export.LegalActionIncomeSource, "wageOrNonWage");

              field1.Color = "green";
              field1.Protected = false;

              var field2 =
                GetField(export.PromptLienResourceDesc, "selectChar");

              field2.Color = "green";
              field2.Protected = false;

              var field3 =
                GetField(export.LegalActionIncomeSource, "withholdingType");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.PromptIwoType, "selectChar");

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }
        }

        if (local.NoOfLegalActions.Count == 0)
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }

        // ---------------------------------------------
        // Get Petitioner/Respondent Information
        // ---------------------------------------------
        UseLeGetPetitionerRespondent();
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ************************************************************
    // Combined the edits for CASE update and CASE print
    // Siraj 5/14/96
    // ************************************************************
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (local.UserAction.Command, "NEXT") || Equal
      (local.UserAction.Command, "PREV"))
    {
      // ---------------------------------------------
      // Make sure that Court Case Number hasn't been
      // changed before an update.
      // ---------------------------------------------
      if (!Equal(import.Fips.StateAbbreviation,
        import.HiddenFips.StateAbbreviation))
      {
        var field = GetField(export.Fips, "stateAbbreviation");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

        return;
      }

      if (!Equal(import.Fips.CountyAbbreviation,
        import.HiddenFips.CountyAbbreviation))
      {
        var field = GetField(export.Fips, "countyAbbreviation");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

        return;
      }

      if (!Equal(import.LegalAction.CourtCaseNumber,
        import.HiddenLegalAction.CourtCaseNumber))
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

        return;
      }

      if (!Equal(export.CsePersonsWorkSet.Number,
        export.HiddenPrevCsePersonsWorkSet.Number))
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

        return;
      }

      // ---------------------------------------------
      // Verify that a display has been performed
      // before the update can take place.
      // ---------------------------------------------
      if (import.LegalAction.Identifier != import.HiddenLegalAction.Identifier)
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

        return;
      }

      if (!Equal(export.LegalActionIncomeSource.EffectiveDate,
        import.HiddenPrevLegalActionIncomeSource.EffectiveDate))
      {
        if (Equal(global.Command, "UPDATE"))
        {
          // --- Allow changing the effecive date
        }
        else if (Equal(global.Command, "DELETE"))
        {
          ExitState = "LE0000_CANT_CHANGE_IWGL_EFF_DATE";

          return;
        }
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "DISPLAY":
        break;
      case "RLTRIB":
        if (!IsEmpty(export.PromptTribuType.SelectChar))
        {
          export.PromptTribuType.SelectChar = "";

          if (import.DlgflwSelectedFips.State > 0)
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Protected = false;
            field.Focused = true;

            export.Fips.Assign(import.DlgflwSelectedFips);
            export.HiddenFips.Assign(import.DlgflwSelectedFips);
            export.Tribunal.Assign(import.DlgflwSelectedTribunal);

            return;
          }
          else if (import.DlgflwSelectedTribunal.Identifier > 0)
          {
            export.Fips.StateAbbreviation = "";
            export.Fips.CountyAbbreviation = "";
            export.Fips.CountyDescription = "";
            export.Tribunal.Assign(import.DlgflwSelectedTribunal);

            if (ReadFipsTribAddress1())
            {
              export.Foreign.Country = entities.FipsTribAddress.Country;
            }

            return;
          }
        }

        break;
      case "LIST":
        // ---------------------------------------------
        // Check and process list prompts
        // ---------------------------------------------
        if (!IsEmpty(export.PromptTribuType.SelectChar) && AsChar
          (export.PromptTribuType.SelectChar) != 'S')
        {
          var field = GetField(export.PromptTribuType, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }
        else if (AsChar(export.PromptTribuType.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (!IsEmpty(export.PromptIwoType.SelectChar) && AsChar
          (export.PromptIwoType.SelectChar) != 'S')
        {
          var field = GetField(export.PromptIwoType, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }
        else if (AsChar(export.PromptIwoType.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (!IsEmpty(export.PromptLienType.SelectChar) && AsChar
          (export.PromptLienType.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }
        else if (AsChar(export.PromptLienType.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (!IsEmpty(export.PromptIncomeSource.SelectChar) && AsChar
          (export.PromptIncomeSource.SelectChar) != 'S')
        {
          var field = GetField(export.PromptIncomeSource, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }
        else if (AsChar(export.PromptIncomeSource.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (!IsEmpty(export.PromptLienResourceDesc.SelectChar) && AsChar
          (export.PromptLienResourceDesc.SelectChar) != 'S')
        {
          var field = GetField(export.PromptLienResourceDesc, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }
        else if (AsChar(export.PromptLienResourceDesc.SelectChar) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (local.Prompt.Count == 0)
        {
          var field3 = GetField(export.PromptTribuType, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.PromptIwoType, "selectChar");

          field4.Error = true;

          var field5 = GetField(export.PromptLienType, "selectChar");

          field5.Error = true;

          var field6 = GetField(export.PromptIncomeSource, "selectChar");

          field6.Error = true;

          var field7 = GetField(export.PromptLienResourceDesc, "selectChar");

          field7.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }
        else if (local.Prompt.Count > 1)
        {
          if (!IsEmpty(export.PromptTribuType.SelectChar))
          {
            var field = GetField(export.PromptTribuType, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptIwoType.SelectChar))
          {
            var field = GetField(export.PromptIwoType, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptLienType.SelectChar))
          {
          }

          if (!IsEmpty(export.PromptIncomeSource.SelectChar))
          {
            var field = GetField(export.PromptIncomeSource, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptLienResourceDesc.SelectChar))
          {
            var field = GetField(export.PromptLienResourceDesc, "selectChar");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        switch(AsChar(export.IwglType.Text1))
        {
          case 'I':
            if (!IsEmpty(export.PromptLienType.SelectChar))
            {
              ExitState = "LE0000_PROMPT_INVALID_FOR_IWO";

              return;
            }

            if (!IsEmpty(export.PromptLienResourceDesc.SelectChar))
            {
              var field = GetField(export.PromptLienResourceDesc, "selectChar");

              field.Error = true;

              ExitState = "LE0000_PROMPT_INVALID_FOR_IWO";

              return;
            }

            break;
          case 'G':
            switch(AsChar(export.LegalActionIncomeSource.WageOrNonWage))
            {
              case 'W':
                if (!IsEmpty(export.PromptLienType.SelectChar))
                {
                  ExitState = "LE0000_INVALID_WAGE_GARNSH";

                  return;
                }

                if (!IsEmpty(export.PromptLienResourceDesc.SelectChar))
                {
                  var field =
                    GetField(export.PromptLienResourceDesc, "selectChar");

                  field.Error = true;

                  ExitState = "LE0000_INVALID_WAGE_GARNSH";

                  return;
                }

                break;
              case 'N':
                if (!IsEmpty(export.PromptLienType.SelectChar))
                {
                  ExitState = "LE0000_INVALID_NON_WAGE_GARNISH";

                  return;
                }

                if (!IsEmpty(export.PromptIncomeSource.SelectChar))
                {
                  var field = GetField(export.PromptIncomeSource, "selectChar");

                  field.Error = true;

                  ExitState = "LE0000_INVALID_NON_WAGE_GARNISH";

                  return;
                }

                break;
              default:
                break;
            }

            break;
          case 'L':
            if (!IsEmpty(export.PromptIwoType.SelectChar))
            {
              var field = GetField(export.PromptIwoType, "selectChar");

              field.Error = true;

              ExitState = "LE0000_PROMPT_INVALID_FOR_LIEN";

              return;
            }

            if (!IsEmpty(export.PromptIncomeSource.SelectChar))
            {
              var field = GetField(export.PromptIncomeSource, "selectChar");

              field.Error = true;

              ExitState = "LE0000_PROMPT_INVALID_FOR_LIEN";

              return;
            }

            break;
          default:
            break;
        }

        if (AsChar(export.PromptTribuType.SelectChar) == 'S')
        {
          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            export.Fips.StateAbbreviation = "KS";
          }

          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS1";

          return;
        }

        if (AsChar(export.PromptIwoType.SelectChar) == 'S')
        {
          export.Required.CodeName = "LEGAL ACTION IWO TYPE";
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptLienType.SelectChar) == 'S')
        {
          export.Required.CodeName = "LEGAL ACTION LIEN TYPE";
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptIncomeSource.SelectChar) == 'S')
        {
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";

          return;
        }

        if (AsChar(export.PromptLienResourceDesc.SelectChar) == 'S')
        {
          export.DlgflwLinkResl.Number = export.CsePersonsWorkSet.Number;
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_RESL_RESOURCE_LIST";

          return;
        }

        break;
      case "RETLACN":
        break;
      case "RETLAPS":
        break;
      case "RETCDVL":
        if (AsChar(import.PromptIwoType.SelectChar) == 'S')
        {
          export.PromptIwoType.SelectChar = "";
          export.LegalActionIncomeSource.WithholdingType =
            import.CodeValue.Cdvalue;

          var field = GetField(export.LegalActionIncomeSource, "orderType");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        if (AsChar(export.PromptLienType.SelectChar) == 'S')
        {
          export.PromptLienType.SelectChar = "";
          export.LegalActionPersonResource.LienType = import.CodeValue.Cdvalue;

          var field = GetField(export.LegalActionIncomeSource, "effectiveDate");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        break;
      case "RETRESL":
        export.PromptLienResourceDesc.SelectChar = "";

        if (ReadCsePersonResource())
        {
          MoveCsePersonResource1(entities.ExistingCsePersonResource,
            export.CsePersonResource);

          if (IsEmpty(entities.ExistingCsePersonResource.ResourceDescription))
          {
            local.Code.CodeName = "RESOURCE TYPE";
            local.CodeValue.Cdvalue =
              entities.ExistingCsePersonResource.Type1 ?? Spaces(10);
            UseCabGetCodeValueDescription();
            export.CsePersonResource.ResourceDescription =
              local.CodeValue.Description;
          }
        }

        MoveIncomeSource(local.InitialisedIncomeSource, export.IncomeSource);
        MoveIncomeSource(local.InitialisedIncomeSource,
          export.HiddenIncomeSource);

        var field1 = GetField(export.LegalAction, "courtCaseNumber");

        field1.Protected = false;
        field1.Focused = true;

        return;

        if (ReadCsePersonResource())
        {
          MoveCsePersonResource1(entities.ExistingCsePersonResource,
            export.CsePersonResource);
        }

        break;
      case "RETINCL":
        export.PromptIncomeSource.SelectChar = "";

        if (ReadIncomeSource())
        {
          MoveIncomeSource(entities.ExistingIncomeSource, export.IncomeSource);
        }

        export.CsePersonResource.Assign(local.InitialisedCsePersonResource);
        MoveCsePersonResource2(local.InitialisedCsePersonResource,
          export.HiddenLien);

        var field2 = GetField(export.LegalAction, "courtCaseNumber");

        field2.Protected = false;
        field2.Focused = true;

        return;

        if (ReadIncomeSource())
        {
          MoveIncomeSource(entities.ExistingIncomeSource, export.IncomeSource);
        }

        break;
      case "ADD":
        // --- At this point the currency on legal action has been established.
        if (!ReadLegalActionPerson())
        {
          if (AsChar(export.LegalAction.Classification) == 'J')
          {
            ExitState = "LE0000_NOT_AN_OBLIGOR_IN_LACT";

            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }

        switch(AsChar(export.IwglType.Text1))
        {
          case 'I':
            // ---------------------------------------------
            // Add a new IWO.
            // ---------------------------------------------
            UseEstabLegalActionIncSource();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              goto Test5;
            }

            break;
          case 'G':
            // ---------------------------------------------
            // Add a new Garnishment.
            // ---------------------------------------------
            if (AsChar(export.LegalActionIncomeSource.WageOrNonWage) == 'W')
            {
              UseEstabLegalActionIncSource();
            }
            else
            {
              export.CsePersonResource.CseActionTakenCode = "G";
              export.CsePersonResource.LienHolderStateOfKansasInd = "Y";
              export.CsePersonResource.LienIndicator = "N";
              export.LegalActionPersonResource.EffectiveDate =
                export.LegalActionIncomeSource.EffectiveDate;
              export.LegalActionPersonResource.EndDate =
                export.LegalActionIncomeSource.EndDate;
              UseEstabLegalActionPersResrc();
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              goto Test5;
            }

            break;
          case 'L':
            // ---------------------------------------------
            // Add a new Lien.
            // ---------------------------------------------
            export.CsePersonResource.CseActionTakenCode = "L";
            export.CsePersonResource.LienHolderStateOfKansasInd = "Y";
            export.CsePersonResource.LienIndicator = "Y";
            export.LegalActionPersonResource.EffectiveDate =
              export.LegalActionIncomeSource.EffectiveDate;
            export.LegalActionPersonResource.EndDate =
              export.LegalActionIncomeSource.EndDate;
            UseEstabLegalActionPersResrc();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              goto Test5;
            }

            break;
          default:
            break;
        }

        if (IsExitState("LEGAL_ACTION_NF"))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          if (!IsEmpty(export.CsePersonsWorkSet.Number))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;
          }

          if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
          {
            var field = GetField(export.CsePersonsWorkSet, "ssn");

            field.Error = true;
          }
        }
        else
        {
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        MoveLegalAction4(export.LegalAction, export.HiddenLegalAction);
        export.HiddenPrevCsePersonsWorkSet.Number =
          export.CsePersonsWorkSet.Number;
        MoveIncomeSource(export.IncomeSource, export.HiddenIncomeSource);
        MoveCsePersonResource2(export.CsePersonResource, export.HiddenLien);
        export.HiddenPrevLegalActionIncomeSource.Assign(
          export.LegalActionIncomeSource);
        export.HiddenPrevLegalActionPersonResource.Assign(
          export.LegalActionPersonResource);
        export.HiddenFips.Assign(export.Fips);

        break;
      case "UPDATE":
        // ************************************************************
        // Combined the edits for CASE update and CASE print
        // Siraj 5/14/96
        // ************************************************************
        // ---------------------------------------------
        // Update the current IWO/Garnishment/Lien.
        // ---------------------------------------------
        UseLeUpdateIwoGarnishmentLien();

        if (IsExitState("LEGAL_ACTION_NF"))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          if (!IsEmpty(export.CsePersonsWorkSet.Number))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;
          }

          if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
          {
            var field = GetField(export.CsePersonsWorkSet, "ssn");

            field.Error = true;
          }
        }
        else if (IsExitState("LE0000_LEGAL_ACT_INCOM_SOURCE_NU"))
        {
        }
        else if (IsExitState("INCOME_SOURCE_NF"))
        {
        }
        else if (IsExitState("LE0000_LEGAL_ACT_PERS_RESOURC_NU"))
        {
          var field = GetField(export.LegalActionPersonResource, "lienType");

          field.Error = true;
        }
        else if (IsExitState("LE0000_LIEN_RESOURCE_NF"))
        {
          var field = GetField(export.LegalActionPersonResource, "lienType");

          field.Error = true;
        }
        else
        {
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        MoveLegalAction4(export.LegalAction, export.HiddenLegalAction);
        export.HiddenPrevCsePersonsWorkSet.Number =
          export.CsePersonsWorkSet.Number;
        MoveIncomeSource(export.IncomeSource, export.HiddenIncomeSource);
        export.HiddenPrevLegalActionIncomeSource.Assign(
          export.LegalActionIncomeSource);
        MoveCsePersonResource2(export.CsePersonResource, export.HiddenLien);
        export.HiddenPrevLegalActionPersonResource.Assign(
          export.LegalActionPersonResource);
        export.HiddenFips.Assign(export.Fips);

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "DELETE":
        // ---------------------------------------------
        // Delete the current IWO/Garnishment/Lien.
        // ---------------------------------------------
        // GVandy		09/08/2000	PR#100875: Not passing the correct identifier when
        // deleting Garnishments.
        if (AsChar(export.IwglType.Text1) == 'G' && AsChar
          (export.LegalActionIncomeSource.WageOrNonWage) != 'W')
        {
          export.LegalActionIncomeSource.Identifier =
            export.LegalActionPersonResource.Identifier;
        }

        UseLeDeleteIwoGarnishmentLien();

        if (IsExitState("LEGAL_ACTION_NF"))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          if (!IsEmpty(export.CsePersonsWorkSet.Number))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;
          }

          if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
          {
            var field = GetField(export.CsePersonsWorkSet, "ssn");

            field.Error = true;
          }
        }
        else if (IsExitState("INCOME_SOURCE_NF"))
        {
        }
        else if (IsExitState("LE0000_LIEN_RESOURCE_NF"))
        {
          var field = GetField(export.LegalActionPersonResource, "lienType");

          field.Error = true;
        }
        else
        {
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        export.CsePersonResource.Assign(local.InitialisedCsePersonResource);
        MoveIncomeSource(local.InitialisedIncomeSource, export.IncomeSource);
        export.LegalActionIncomeSource.Assign(
          local.InitialisedLegalActionIncomeSource);
        export.LegalActionPersonResource.Assign(
          local.InitialisedLegalActionPersonResource);
        MoveCsePersonResource2(local.InitialisedCsePersonResource,
          export.HiddenLien);
        MoveIncomeSource(local.InitialisedIncomeSource,
          export.HiddenIncomeSource);
        export.HiddenPrevLegalActionIncomeSource.Assign(
          local.InitialisedLegalActionIncomeSource);
        export.HiddenPrevLegalActionPersonResource.Assign(
          local.InitialisedLegalActionPersonResource);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "SIGNOFF":
        // ---------------------------------------------
        // Sign the user off the KESSEP system.
        // ---------------------------------------------
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

Test5:

    if (Equal(global.Command, "DISPLAY"))
    {
      switch(AsChar(export.IwglType.Text1))
      {
        case ' ':
          ExitState = "LE0000_IWGL_TYPE_REQD";

          var field1 = GetField(export.IwglType, "text1");

          field1.Color = "red";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = true;

          return;
        case 'G':
          switch(AsChar(export.LegalActionIncomeSource.WageOrNonWage))
          {
            case ' ':
              // ****************************************************************
              // * When a G class legal action selection from LACN is made
              // * the wage/non-wage indicator is spaces when entry into
              // * IWGL is made.  First attempt to read the for Garn/ W, if
              // * not found setup to read for Garn/ N.
              // ****************************************************************
              export.LegalActionIncomeSource.WageOrNonWage = "W";
              UseLeDisplayIwoGarnishmentLien2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.LegalActionIncomeSource.WageOrNonWage = "N";

                // *** PR#83723 - Reset Exitstate prior to again
                // calling SWE000766 using LAIS wage_or_non_wage = N.
                ExitState = "ACO_NN0000_ALL_OK";
              }

              break;
            case 'W':
              break;
            case 'N':
              break;
            default:
              ExitState = "LE0000_IWGL_INV_SEARCH_W_OR_NW";

              var field =
                GetField(export.LegalActionIncomeSource, "wageOrNonWage");

              field.Error = true;

              return;
          }

          break;
        case 'I':
          if (!IsEmpty(export.LegalActionIncomeSource.WageOrNonWage))
          {
            ExitState = "LE0000_IWGL_SEARCH_W_NW_BE_SPACE";

            var field =
              GetField(export.LegalActionIncomeSource, "wageOrNonWage");

            field.Error = true;

            return;
          }

          break;
        case 'L':
          ExitState = "LE0000_INV_IWGL_TYPE";

          var field2 = GetField(export.IwglType, "text1");

          field2.Color = "red";
          field2.Intensity = Intensity.High;
          field2.Highlighting = Highlighting.ReverseVideo;
          field2.Protected = true;

          return;
        default:
          ExitState = "LE0000_INV_IWGL_TYPE";

          var field3 = GetField(export.IwglType, "text1");

          field3.Color = "red";
          field3.Intensity = Intensity.High;
          field3.Highlighting = Highlighting.ReverseVideo;
          field3.Protected = true;

          return;
      }

      UseLeDisplayIwoGarnishmentLien1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        MoveLegalAction4(export.LegalAction, export.HiddenLegalAction);
        export.IwglType.Text1 = export.LegalAction.Classification;

        if (AsChar(export.IwglType.Text1) == 'I')
        {
          var field1 =
            GetField(export.LegalActionIncomeSource, "wageOrNonWage");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.PromptLienResourceDesc, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.LegalActionIncomeSource, "withholdingType");

          field3.Color = "green";
          field3.Protected = false;

          var field4 = GetField(export.PromptIwoType, "selectChar");

          field4.Color = "green";
          field4.Protected = false;
        }
        else if (AsChar(export.IwglType.Text1) == 'G')
        {
          var field1 =
            GetField(export.LegalActionIncomeSource, "wageOrNonWage");

          field1.Color = "green";
          field1.Protected = false;

          var field2 = GetField(export.PromptLienResourceDesc, "selectChar");

          field2.Color = "green";
          field2.Protected = false;

          var field3 =
            GetField(export.LegalActionIncomeSource, "withholdingType");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.PromptIwoType, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        export.HiddenPrevCsePersonsWorkSet.Number =
          export.CsePersonsWorkSet.Number;
        MoveIncomeSource(export.IncomeSource, export.HiddenIncomeSource);
        MoveCsePersonResource2(export.CsePersonResource, export.HiddenLien);
        export.HiddenPrevLegalActionIncomeSource.Assign(
          export.LegalActionIncomeSource);
        export.HiddenPrevLegalActionPersonResource.Assign(
          export.LegalActionPersonResource);
        export.HiddenFips.Assign(export.Fips);
      }
    }
    else
    {
    }

    if (IsExitState("LE0000_LIEN_RESO_NOT_SELECTED"))
    {
      var field1 = GetField(export.PromptLienResourceDesc, "selectChar");

      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.CsePersonResource, "resourceDescription");

      field2.Color = "red";
      field2.Intensity = Intensity.High;
      field2.Highlighting = Highlighting.ReverseVideo;
      field2.Protected = true;

      return;
    }
    else if (IsExitState("LE0000_INCOME_SOURCE_NOT_SPECIFD"))
    {
      var field1 = GetField(export.PromptIncomeSource, "selectChar");

      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.IncomeSource, "name");

      field2.Color = "red";
      field2.Intensity = Intensity.High;
      field2.Highlighting = Highlighting.ReverseVideo;
      field2.Protected = true;

      return;
    }
    else if (IsExitState("LEGAL_ACTION_NF"))
    {
      var field = GetField(export.LegalAction, "courtCaseNumber");

      field.Error = true;

      return;
    }
    else if (IsExitState("CSE_PERSON_NF"))
    {
      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;
      }

      if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
      {
        var field = GetField(export.CsePersonsWorkSet, "ssn");

        field.Error = true;
      }

      return;
    }
    else if (IsExitState("INCOME_SOURCE_NF"))
    {
      var field = GetField(export.LegalActionIncomeSource, "withholdingType");

      field.Error = true;

      return;
    }
    else if (IsExitState("LE0000_LIEN_RESOURCE_NF"))
    {
      var field = GetField(export.LegalActionPersonResource, "lienType");

      field.Error = true;

      return;
    }
    else
    {
    }

    // ------------------------------------------------
    // If these dates were stored as max dates,
    // (12312099), convert them to zeros and don't
    // display them on the screen.
    // ------------------------------------------------
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(export.LegalActionIncomeSource.EffectiveDate, local.Max.Date))
    {
      export.LegalActionIncomeSource.EffectiveDate = null;
    }

    if (Equal(export.LegalActionIncomeSource.EndDate, local.Max.Date))
    {
      export.LegalActionIncomeSource.EndDate = null;
    }

    if (Equal(export.LegalActionPersonResource.EffectiveDate, local.Max.Date))
    {
      export.LegalActionPersonResource.EffectiveDate = null;
    }

    if (Equal(export.LegalActionPersonResource.EndDate, local.Max.Date))
    {
      export.LegalActionPersonResource.EndDate = null;
    }

    // ------------------------------------------------
    // If all processing completed successfully, move
    // all exports to previous exports.
    // ------------------------------------------------
    MoveLegalAction4(export.LegalAction, export.HiddenLegalAction);
    export.HiddenPrevCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveIncomeSource(export.IncomeSource, export.HiddenIncomeSource);
    MoveCsePersonResource2(export.CsePersonResource, export.HiddenLien);
    export.HiddenFips.Assign(export.Fips);
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Percentage = source.Percentage;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonResource1(CsePersonResource source,
    CsePersonResource target)
  {
    target.LienHolderStateOfKansasInd = source.LienHolderStateOfKansasInd;
    target.LienIndicator = source.LienIndicator;
    target.ResourceNo = source.ResourceNo;
    target.ResourceDescription = source.ResourceDescription;
    target.CseActionTakenCode = source.CseActionTakenCode;
  }

  private static void MoveCsePersonResource2(CsePersonResource source,
    CsePersonResource target)
  {
    target.ResourceNo = source.ResourceNo;
    target.ResourceDescription = source.ResourceDescription;
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

  private static void MoveExport1ToMatchCsePersons(CabMatchCsePerson.Export.
    ExportGroup source, Local.MatchCsePersonsGroup target)
  {
    target.DetailMatchCsePer.Assign(source.Detail);
    target.ExportDetailAlt.Flag = source.Ae.Flag;
    target.Ae.Flag = source.Cse.Flag;
    target.Cse.Flag = source.Kanpay.Flag;
    target.Kanpay.Flag = source.Kscares.Flag;
    target.Kscares.Flag = source.Alt.Flag;
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalActionIncomeSource(
    LegalActionIncomeSource source, LegalActionIncomeSource target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.Identifier = source.Identifier;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNum9 = source.SsnNum9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.CodeValue.Assign(useExport.CodeValue);
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea1(useExport.SsnWorkArea, local.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.SsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.CodeValue.Assign(useExport.CodeValue);
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

  private void UseEstabLegalActionIncSource()
  {
    var useImport = new EstabLegalActionIncSource.Import();
    var useExport = new EstabLegalActionIncSource.Export();

    useImport.IwglInd.Text1 = export.IwglType.Text1;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.LegalActionIncomeSource.Assign(export.LegalActionIncomeSource);
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;

    Call(EstabLegalActionIncSource.Execute, useImport, useExport);
  }

  private void UseEstabLegalActionPersResrc()
  {
    var useImport = new EstabLegalActionPersResrc.Import();
    var useExport = new EstabLegalActionPersResrc.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.LienLegalActionPersonResource.Assign(
      export.LegalActionPersonResource);
    useImport.LienCsePersonResource.Assign(export.CsePersonResource);

    Call(EstabLegalActionPersResrc.Execute, useImport, useExport);
  }

  private void UseLeDeleteIwoGarnishmentLien()
  {
    var useImport = new LeDeleteIwoGarnishmentLien.Import();
    var useExport = new LeDeleteIwoGarnishmentLien.Export();

    useImport.CsePersonResource.ResourceNo =
      export.CsePersonResource.ResourceNo;
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.LegalActionIncomeSource.Assign(export.LegalActionIncomeSource);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Type1.Text1 = export.IwglType.Text1;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(LeDeleteIwoGarnishmentLien.Execute, useImport, useExport);
  }

  private void UseLeDisplayIwoGarnishmentLien1()
  {
    var useImport = new LeDisplayIwoGarnishmentLien.Import();
    var useExport = new LeDisplayIwoGarnishmentLien.Export();

    useImport.SearchForGarnishment.WageOrNonWage =
      export.LegalActionIncomeSource.WageOrNonWage;
    useImport.SearchIwglType.Text1 = export.IwglType.Text1;
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.LegalActionIncomeSource.Assign(export.LegalActionIncomeSource);

    Call(LeDisplayIwoGarnishmentLien.Execute, useImport, useExport);

    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    MoveLegalAction2(useExport.LegalAction, export.LegalAction);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.LegalActionIncomeSource.Assign(useExport.LegalActionIncomeSource);
    MoveIncomeSource(useExport.IncomeSource, export.IncomeSource);
    export.LegalActionPersonResource.
      Assign(useExport.LegalActionPersonResource);
    MoveCsePersonResource2(useExport.CsePersonResource, export.CsePersonResource);
      
  }

  private void UseLeDisplayIwoGarnishmentLien2()
  {
    var useImport = new LeDisplayIwoGarnishmentLien.Import();
    var useExport = new LeDisplayIwoGarnishmentLien.Export();

    useImport.SearchForGarnishment.WageOrNonWage =
      export.LegalActionIncomeSource.WageOrNonWage;
    useImport.SearchIwglType.Text1 = export.IwglType.Text1;
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.LegalActionIncomeSource.Assign(export.LegalActionIncomeSource);

    Call(LeDisplayIwoGarnishmentLien.Execute, useImport, useExport);
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

  private void UseLeUpdateIwoGarnishmentLien()
  {
    var useImport = new LeUpdateIwoGarnishmentLien.Import();
    var useExport = new LeUpdateIwoGarnishmentLien.Export();

    MoveLegalActionIncomeSource(export.HiddenPrevLegalActionIncomeSource,
      useImport.Current);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Type1.Text1 = export.IwglType.Text1;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.New1.Assign(export.LegalActionIncomeSource);
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.LienLegalActionPersonResource.Assign(
      export.LegalActionPersonResource);
    useImport.LienCsePersonResource.ResourceNo =
      export.CsePersonResource.ResourceNo;

    Call(LeUpdateIwoGarnishmentLien.Execute, useImport, useExport);
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

    MoveLegalAction4(export.LegalAction, useImport.LegalAction);
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
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

  private bool ReadCsePersonResource()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
        db.SetInt32(command, "resourceNo", import.CsePersonResource.ResourceNo);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 18);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 19);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 21);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.ExistingCsePersonResource.Populated = true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
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

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress3()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", export.CsePersonsWorkSet.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Name = db.GetNullableString(reader, 1);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 2);
        entities.ExistingIncomeSource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIwoTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);
    entities.IwoTransaction.Populated = false;

    return ReadEach("ReadIwoTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(
          command, "cspINumber", entities.ExistingIncomeSource.CspINumber);
        db.SetNullableDateTime(
          command, "isrIdentifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 1);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 2);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 3);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 4);
        entities.IwoTransaction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionDetailLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return ReadEach("ReadLegalActionLegalActionDetailLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.ExistingCsePerson.Number = db.GetString(reader, 8);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 10);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.LegalActionPerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", export.LegalAction.Identifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
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
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 5);
        entities.ExistingTribunal.Name = db.GetString(reader, 6);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalFips()
  {
    entities.ExistingFips.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunalFips",
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
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 5);
        entities.ExistingTribunal.Name = db.GetString(reader, 6);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.ExistingFips.Location = db.GetInt32(reader, 7);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.ExistingFips.County = db.GetInt32(reader, 9);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.ExistingFips.State = db.GetInt32(reader, 10);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 11);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 12);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 13);
        entities.ExistingFips.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionTribunalLegalActionDetailLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingCsePerson.Populated = false;

    return ReadEach("ReadLegalActionTribunalLegalActionDetailLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 5);
        entities.ExistingTribunal.Name = db.GetString(reader, 6);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 11);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 12);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 13);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 14);
        entities.ExistingCsePerson.Number = db.GetString(reader, 14);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 15);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 16);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 17);
        entities.LegalActionPerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingCsePerson.Populated = true;

        return true;
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
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
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
    /// A value of DlgflwSelectedFips.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedFips")]
    public Fips DlgflwSelectedFips
    {
      get => dlgflwSelectedFips ??= new();
      set => dlgflwSelectedFips = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalActionPersonResource")]
    public LegalActionPersonResource HiddenPrevLegalActionPersonResource
    {
      get => hiddenPrevLegalActionPersonResource ??= new();
      set => hiddenPrevLegalActionPersonResource = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalActionIncomeSource")]
    public LegalActionIncomeSource HiddenPrevLegalActionIncomeSource
    {
      get => hiddenPrevLegalActionIncomeSource ??= new();
      set => hiddenPrevLegalActionIncomeSource = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of IwglType.
    /// </summary>
    [JsonPropertyName("iwglType")]
    public WorkArea IwglType
    {
      get => iwglType ??= new();
      set => iwglType = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of PromptIwoType.
    /// </summary>
    [JsonPropertyName("promptIwoType")]
    public Common PromptIwoType
    {
      get => promptIwoType ??= new();
      set => promptIwoType = value;
    }

    /// <summary>
    /// A value of PromptIncomeSource.
    /// </summary>
    [JsonPropertyName("promptIncomeSource")]
    public Common PromptIncomeSource
    {
      get => promptIncomeSource ??= new();
      set => promptIncomeSource = value;
    }

    /// <summary>
    /// A value of PromptTribuType.
    /// </summary>
    [JsonPropertyName("promptTribuType")]
    public Common PromptTribuType
    {
      get => promptTribuType ??= new();
      set => promptTribuType = value;
    }

    /// <summary>
    /// A value of PromptLienType.
    /// </summary>
    [JsonPropertyName("promptLienType")]
    public Common PromptLienType
    {
      get => promptLienType ??= new();
      set => promptLienType = value;
    }

    /// <summary>
    /// A value of PromptLienResourceDesc.
    /// </summary>
    [JsonPropertyName("promptLienResourceDesc")]
    public Common PromptLienResourceDesc
    {
      get => promptLienResourceDesc ??= new();
      set => promptLienResourceDesc = value;
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
    /// A value of HiddenIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenIncomeSource")]
    public IncomeSource HiddenIncomeSource
    {
      get => hiddenIncomeSource ??= new();
      set => hiddenIncomeSource = value;
    }

    /// <summary>
    /// A value of HiddenLien.
    /// </summary>
    [JsonPropertyName("hiddenLien")]
    public CsePersonResource HiddenLien
    {
      get => hiddenLien ??= new();
      set => hiddenLien = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    private Tribunal dlgflwSelectedTribunal;
    private Fips dlgflwSelectedFips;
    private Tribunal tribunal;
    private Fips hiddenFips;
    private Fips fips;
    private Document document;
    private ScrollingAttributes scrollingAttributes;
    private LegalActionPersonResource hiddenPrevLegalActionPersonResource;
    private LegalActionIncomeSource hiddenPrevLegalActionIncomeSource;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private Common hiddenPrevUserAction;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private LegalAction legalAction;
    private WorkArea iwglType;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource incomeSource;
    private LegalActionPersonResource legalActionPersonResource;
    private CsePersonResource csePersonResource;
    private Common promptIwoType;
    private Common promptIncomeSource;
    private Common promptTribuType;
    private Common promptLienType;
    private Common promptLienResourceDesc;
    private LegalAction hiddenLegalAction;
    private IncomeSource hiddenIncomeSource;
    private CsePersonResource hiddenLien;
    private CodeValue codeValue;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private FipsTribAddress foreign;
    private SsnWorkArea ssnWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of HiddenPrevLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalActionPersonResource")]
    public LegalActionPersonResource HiddenPrevLegalActionPersonResource
    {
      get => hiddenPrevLegalActionPersonResource ??= new();
      set => hiddenPrevLegalActionPersonResource = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalActionIncomeSource")]
    public LegalActionIncomeSource HiddenPrevLegalActionIncomeSource
    {
      get => hiddenPrevLegalActionIncomeSource ??= new();
      set => hiddenPrevLegalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DlgflwLinkResl.
    /// </summary>
    [JsonPropertyName("dlgflwLinkResl")]
    public CsePerson DlgflwLinkResl
    {
      get => dlgflwLinkResl ??= new();
      set => dlgflwLinkResl = value;
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
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of IwglType.
    /// </summary>
    [JsonPropertyName("iwglType")]
    public WorkArea IwglType
    {
      get => iwglType ??= new();
      set => iwglType = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of PromptIwoType.
    /// </summary>
    [JsonPropertyName("promptIwoType")]
    public Common PromptIwoType
    {
      get => promptIwoType ??= new();
      set => promptIwoType = value;
    }

    /// <summary>
    /// A value of PromptTribuType.
    /// </summary>
    [JsonPropertyName("promptTribuType")]
    public Common PromptTribuType
    {
      get => promptTribuType ??= new();
      set => promptTribuType = value;
    }

    /// <summary>
    /// A value of PromptIncomeSource.
    /// </summary>
    [JsonPropertyName("promptIncomeSource")]
    public Common PromptIncomeSource
    {
      get => promptIncomeSource ??= new();
      set => promptIncomeSource = value;
    }

    /// <summary>
    /// A value of PromptLienType.
    /// </summary>
    [JsonPropertyName("promptLienType")]
    public Common PromptLienType
    {
      get => promptLienType ??= new();
      set => promptLienType = value;
    }

    /// <summary>
    /// A value of PromptLienResourceDesc.
    /// </summary>
    [JsonPropertyName("promptLienResourceDesc")]
    public Common PromptLienResourceDesc
    {
      get => promptLienResourceDesc ??= new();
      set => promptLienResourceDesc = value;
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
    /// A value of HiddenIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenIncomeSource")]
    public IncomeSource HiddenIncomeSource
    {
      get => hiddenIncomeSource ??= new();
      set => hiddenIncomeSource = value;
    }

    /// <summary>
    /// A value of HiddenLien.
    /// </summary>
    [JsonPropertyName("hiddenLien")]
    public CsePersonResource HiddenLien
    {
      get => hiddenLien ??= new();
      set => hiddenLien = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
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
    /// A value of ListTribunal.
    /// </summary>
    [JsonPropertyName("listTribunal")]
    public Standard ListTribunal
    {
      get => listTribunal ??= new();
      set => listTribunal = value;
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

    private Tribunal tribunal;
    private Fips hiddenFips;
    private Fips fips;
    private Document document;
    private LegalActionPersonResource hiddenPrevLegalActionPersonResource;
    private LegalActionIncomeSource hiddenPrevLegalActionIncomeSource;
    private ScrollingAttributes scrollingAttributes;
    private Code required;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private CsePerson dlgflwLinkResl;
    private Common hiddenPrevUserAction;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private LegalAction legalAction;
    private WorkArea iwglType;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource incomeSource;
    private LegalActionPersonResource legalActionPersonResource;
    private CsePersonResource csePersonResource;
    private Common promptIwoType;
    private Common promptTribuType;
    private Common promptIncomeSource;
    private Common promptLienType;
    private Common promptLienResourceDesc;
    private LegalAction hiddenLegalAction;
    private IncomeSource hiddenIncomeSource;
    private CsePersonResource hiddenLien;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private FipsTribAddress foreign;
    private Standard listTribunal;
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
      /// A value of ExportDetailAlt.
      /// </summary>
      [JsonPropertyName("exportDetailAlt")]
      public Common ExportDetailAlt
      {
        get => exportDetailAlt ??= new();
        set => exportDetailAlt = value;
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

      private CsePersonsWorkSet detailMatchCsePer;
      private Common exportDetailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public CsePersonsWorkSet Test
    {
      get => test ??= new();
      set => test = value;
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
    /// A value of LastReadObligor.
    /// </summary>
    [JsonPropertyName("lastReadObligor")]
    public CsePerson LastReadObligor
    {
      get => lastReadObligor ??= new();
      set => lastReadObligor = value;
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
    /// A value of InitialisedLegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("initialisedLegalActionIncomeSource")]
    public LegalActionIncomeSource InitialisedLegalActionIncomeSource
    {
      get => initialisedLegalActionIncomeSource ??= new();
      set => initialisedLegalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of InitialisedIncomeSource.
    /// </summary>
    [JsonPropertyName("initialisedIncomeSource")]
    public IncomeSource InitialisedIncomeSource
    {
      get => initialisedIncomeSource ??= new();
      set => initialisedIncomeSource = value;
    }

    /// <summary>
    /// A value of InitialisedLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("initialisedLegalActionPersonResource")]
    public LegalActionPersonResource InitialisedLegalActionPersonResource
    {
      get => initialisedLegalActionPersonResource ??= new();
      set => initialisedLegalActionPersonResource = value;
    }

    /// <summary>
    /// A value of InitialisedCsePersonResource.
    /// </summary>
    [JsonPropertyName("initialisedCsePersonResource")]
    public CsePersonResource InitialisedCsePersonResource
    {
      get => initialisedCsePersonResource ??= new();
      set => initialisedCsePersonResource = value;
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
    /// A value of XxxPrint.
    /// </summary>
    [JsonPropertyName("xxxPrint")]
    public Document XxxPrint
    {
      get => xxxPrint ??= new();
      set => xxxPrint = value;
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
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CsePersonsWorkSet Save
    {
      get => save ??= new();
      set => save = value;
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
    /// A value of NumberOfLegalActions.
    /// </summary>
    [JsonPropertyName("numberOfLegalActions")]
    public Common NumberOfLegalActions
    {
      get => numberOfLegalActions ??= new();
      set => numberOfLegalActions = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of TotalPromptsSelected.
    /// </summary>
    [JsonPropertyName("totalPromptsSelected")]
    public Common TotalPromptsSelected
    {
      get => totalPromptsSelected ??= new();
      set => totalPromptsSelected = value;
    }

    /// <summary>
    /// A value of NoOfLegalActions.
    /// </summary>
    [JsonPropertyName("noOfLegalActions")]
    public Common NoOfLegalActions
    {
      get => noOfLegalActions ??= new();
      set => noOfLegalActions = value;
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    private CsePersonsWorkSet test;
    private Common prompt;
    private CsePerson lastReadObligor;
    private DateWorkArea current;
    private LegalActionIncomeSource initialisedLegalActionIncomeSource;
    private IncomeSource initialisedIncomeSource;
    private LegalActionPersonResource initialisedLegalActionPersonResource;
    private CsePersonResource initialisedCsePersonResource;
    private TextWorkArea textWorkArea;
    private Document xxxPrint;
    private Common userAction;
    private Array<MatchCsePersonsGroup> matchCsePersons;
    private Common searchOption;
    private CsePersonsWorkSet save;
    private DateWorkArea initialisedToZeros;
    private Common numberOfLegalActions;
    private DateWorkArea max;
    private LegalAction legalAction;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common totalPromptsSelected;
    private Common noOfLegalActions;
    private NextTranInfo nextTranInfo;
    private SsnWorkArea ssnWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
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
    /// A value of ExistingCsePersonResource.
    /// </summary>
    [JsonPropertyName("existingCsePersonResource")]
    public CsePersonResource ExistingCsePersonResource
    {
      get => existingCsePersonResource ??= new();
      set => existingCsePersonResource = value;
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

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private IwoTransaction iwoTransaction;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private FipsTribAddress fipsTribAddress;
    private IncomeSource existingIncomeSource;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private CsePersonResource existingCsePersonResource;
    private LegalAction existingLegalAction;
    private CsePerson existingCsePerson;
  }
#endregion
}

// Program: LE_LCCC_LST_CSE_CASES_BY_CT_CASE, ID: 371978058, model: 746.
// Short name: SWELCCCP
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
/// A program: LE_LCCC_LST_CSE_CASES_BY_CT_CASE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLcccLstCseCasesByCtCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LCCC_LST_CSE_CASES_BY_CT_CASE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLcccLstCseCasesByCtCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLcccLstCseCasesByCtCase.
  /// </summary>
  public LeLcccLstCseCasesByCtCase(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 07/31/95	Dave Allen			Initial Code
    // 12/19/95	Maryrose Mallari		Enhancement
    // 11/18,27/98	P McElderry	None listed
    // Various I/O performance enhancements; named unused
    // entity types as 'ZDEL'.  These could not be deleted at the
    // time.
    // Restructured DISPLAY logic to perform more efficiently and
    // perform necessary error logic.
    // 11/30/98	P McElderry	None listed
    // Inserted NEXT and PREV logic.
    // 06/08/00	JMagat		PR#96867
    // Needed Actual LEGAL ACTION Read (instead of Extended
    // Read) for the CASE Read else fatal error occurs.
    // 10/04/2000	M Ramirez	102858		Added link from DKEY to LCCC
    // 10/04/2000	M Ramirez	none		Basic restructuring for maintenance purposes.
    // Each place I made a change has a note, followed by the replaced code (
    // which is commented out), followed by the new code.
    // 10/05/2000	M Ramirez	none		Changed READs and READ EACHs to select-only 
    // and uncommitted/browse where applicable.
    // 10/16/2000	M Ramirez	105376		Changed READ EACH to look for Cases
    // related to all legal actions, not just the one identified by
    // legal_action identifier
    // 12/18/00	GVandy		106495
    // Change the message displayed when no records match the entered criteria.
    // 7/1/2019        R Mathews        CQ65312        Add AP name to case 
    // detail
    // 8/21/2019       R Mathews        CQ66294        Fixed open cases not 
    // displaying after a closed case
    // ----------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -----------------------
    // Move Imports to Exports
    // -----------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.SelectedCase.Number = import.SelectedCase.Number;
    MoveLegalAction(import.PreviousLegalAction, export.PreviousLegalAction);
    export.DataExists.Flag = import.DataExists.Flag;
    MoveFips(export.PreviousFips, export.Fips);
    export.Foreign.Country = export.PreviousFipsTribAddress.Country;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);

    if (!IsEmpty(export.SelectedCase.Number))
    {
      local.TextWorkArea.Text10 = export.SelectedCase.Number;
      UseEabPadLeftWithZeros();
      export.SelectedCase.Number = local.TextWorkArea.Text10;
    }

    export.SelectedOffice.SystemGeneratedId =
      import.SelectedOffice.SystemGeneratedId;
    export.SelectedServiceProvider.SystemGeneratedId =
      import.SelectedServiceProvider.SystemGeneratedId;
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.Foreign.Country = import.Foreign.Country;
    export.Cases.Index = -1;

    if (!Equal(global.Command, "RLTRIB"))
    {
      if (Equal(export.LegalAction.CourtCaseNumber,
        export.PreviousLegalAction.CourtCaseNumber))
      {
        for(import.Cases.Index = 0; import.Cases.Index < import.Cases.Count; ++
          import.Cases.Index)
        {
          ++export.Cases.Index;
          export.Cases.CheckSize();

          export.Cases.Update.Common.SelectChar =
            import.Cases.Item.Common.SelectChar;
          export.Cases.Update.Case1.Assign(import.Cases.Item.Case1);
          MoveOffice(import.Cases.Item.DetailOffice,
            export.Cases.Update.DetailOffice);
          export.Cases.Update.DetailServiceProvider.Assign(
            import.Cases.Item.DetailServiceProvider);
          MoveCsePersonsWorkSet(import.Cases.Item.CsePersonsWorkSet,
            export.Cases.Update.CsePersonsWorkSet);
          export.Cases.Update.TextWorkArea.Text30 =
            import.Cases.Item.TextWorkArea.Text30;

          if (!IsEmpty(import.Cases.Item.Common.SelectChar))
          {
            export.SelectedCase.Number = import.Cases.Item.Case1.Number;
            export.SelectedServiceProvider.SystemGeneratedId =
              import.Cases.Item.DetailServiceProvider.SystemGeneratedId;
            MoveOffice(import.Cases.Item.DetailOffice, export.SelectedOffice);
          }
        }
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    // MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ----------------------
      // Begin 11/18/98 changes
      // ----------------------
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // -----------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base.
      // Set command to initial command required or ESCAPE
      // ------------------------------------------------------------
      // mjr
      // ----------------------------------
      // 10/05/2000
      // Modified Next_Tran to read for the tribunal info
      // -----------------------------------------------
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }

      if (ReadLegalAction3())
      {
        export.LegalAction.CourtCaseNumber =
          entities.LegalAction.CourtCaseNumber;

        if (ReadTribunal())
        {
          export.Tribunal.Identifier = entities.Tribunal.Identifier;

          if (ReadFips2())
          {
            export.Fips.CountyAbbreviation = entities.Fips.CountyAbbreviation;
            export.Fips.StateAbbreviation = entities.Fips.StateAbbreviation;
          }
          else if (ReadFipsTribAddress1())
          {
            export.Foreign.Country = entities.ExistingForeign.Country;
          }
          else
          {
            return;
          }
        }
        else
        {
          return;
        }
      }
      else
      {
        return;
      }

      // --------------------
      // End 11/18/98 changes
      // --------------------
      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // --------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // ---------------------------------------------------------------
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;

      for(export.Cases.Index = 0; export.Cases.Index < export.Cases.Count; ++
        export.Cases.Index)
      {
        if (!export.Cases.CheckSize())
        {
          break;
        }

        if (AsChar(export.Cases.Item.Common.SelectChar) == 'S')
        {
          local.NextTranInfo.CaseNumber = export.Cases.Item.Case1.Number;

          break;
        }
      }

      export.Cases.CheckIndex();
      UseScCabNextTranPut();

      return;
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }

    if (!Equal(global.Command, "RLTRIB"))
    {
      // ------------------------------
      // Validate action level security
      // ------------------------------
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "COMP":
        for(export.Cases.Index = 0; export.Cases.Index < export.Cases.Count; ++
          export.Cases.Index)
        {
          if (!export.Cases.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Cases.Item.Common.SelectChar))
          {
            if (AsChar(export.Cases.Item.Common.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_SI_COMP_CASE_COMP";

              return;
            }
            else
            {
              var field = GetField(export.Cases.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
            }

            local.NonSpaces.Flag = "N";
          }
        }

        export.Cases.CheckIndex();

        if (IsEmpty(local.NonSpaces.Flag))
        {
          for(export.Cases.Index = 0; export.Cases.Index < export.Cases.Count; ++
            export.Cases.Index)
          {
            if (!export.Cases.CheckSize())
            {
              break;
            }

            if (IsEmpty(export.Cases.Item.Common.SelectChar))
            {
              var field = GetField(export.Cases.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            }
            else
            {
              // --------------------
              // no other possibility
              // --------------------
            }
          }

          export.Cases.CheckIndex();
        }

        break;
      case "LIST":
        switch(AsChar(export.PromptTribunal.SelectChar))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

            break;
          case ' ':
            var field1 = GetField(export.PromptTribunal, "selectChar");

            field1.Error = true;

            ExitState = "ZD_ACO_NE00_MUST_SELECT_4_PROMPT";

            break;
          default:
            var field2 = GetField(export.PromptTribunal, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        break;
      case "RLTRIB":
        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          // mjr
          // -----------------------------------------
          // 10/05/2000
          // Removed setting previous legal_action
          // ------------------------------------------------------
          export.PromptTribunal.SelectChar = "";

          if (import.DlgflwSelected.State > 0)
          {
            export.Fips.Assign(import.DlgflwSelected);
          }
          else
          {
            export.Fips.StateAbbreviation = "";
            export.Fips.CountyAbbreviation = "";
            export.Fips.CountyDescription = "";
          }
        }

        break;
      case "DISPLAY":
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          if (export.LegalAction.Identifier > 0)
          {
            if (ReadLegalAction3())
            {
              export.LegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;

              if (ReadTribunal())
              {
                export.Tribunal.Identifier = entities.Tribunal.Identifier;

                if (ReadFips2())
                {
                  export.Fips.CountyAbbreviation =
                    entities.Fips.CountyAbbreviation;
                  export.Fips.StateAbbreviation =
                    entities.Fips.StateAbbreviation;
                }
                else if (ReadFipsTribAddress1())
                {
                  export.Foreign.Country = entities.ExistingForeign.Country;
                }
              }
            }
            else
            {
              export.LegalAction.Identifier = 0;
            }
          }

          if (export.LegalAction.Identifier == 0)
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }

        if (IsEmpty(export.Fips.CountyAbbreviation) && IsEmpty
          (export.Fips.StateAbbreviation) && IsEmpty(export.Foreign.Country))
        {
          var field1 = GetField(export.Fips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "stateAbbreviation");

          field2.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
        else if (!IsEmpty(export.Foreign.Country))
        {
          if (!IsEmpty(export.Fips.CountyAbbreviation))
          {
            var field1 = GetField(export.Fips, "countyAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.Foreign, "country");

            field2.Color = "red";
            field2.Intensity = Intensity.High;
            field2.Highlighting = Highlighting.ReverseVideo;
            field2.Protected = true;

            ExitState = "ACO_NI0000_CLEAR_SCREEN_TO_DISP";
          }

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            var field1 = GetField(export.Fips, "stateAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.Foreign, "country");

            field2.Color = "red";
            field2.Intensity = Intensity.High;
            field2.Highlighting = Highlighting.ReverseVideo;
            field2.Protected = true;

            ExitState = "ACO_NI0000_CLEAR_SCREEN_TO_DISP";
          }
        }
        else
        {
          if (IsEmpty(export.Fips.CountyAbbreviation))
          {
            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.PreviousLegalAction.CourtCaseNumber) || !
          Equal(export.Fips.StateAbbreviation,
          export.PreviousFips.StateAbbreviation) || !
          Equal(export.Fips.CountyAbbreviation,
          export.PreviousFips.CountyAbbreviation) || !
          Equal(export.Foreign.Country, export.PreviousFipsTribAddress.Country))
        {
          export.LegalAction.Identifier = 0;
          export.PetitionerRespondentDetails.PetitionerName = "";
          export.PetitionerRespondentDetails.MorePetitionerInd = "";
          export.PetitionerRespondentDetails.RespondentName = "";
          export.PetitionerRespondentDetails.MoreRespondentInd = "";
        }

        if (export.LegalAction.Identifier == 0)
        {
          if (ReadLegalAction1())
          {
            export.LegalAction.Identifier = entities.LegalAction.Identifier;
          }

          if (export.LegalAction.Identifier == 0)
          {
            if (IsEmpty(export.Foreign.Country))
            {
              export.Fips.CountyDescription = "";

              if (ReadFips1())
              {
                export.Fips.CountyDescription = entities.Fips.CountyDescription;
              }

              export.Cases.Count = 0;

              var field1 = GetField(export.Fips, "countyAbbreviation");

              field1.Error = true;

              var field2 = GetField(export.Fips, "stateAbbreviation");

              field2.Error = true;

              if (IsEmpty(export.Fips.CountyDescription))
              {
                ExitState = "INVALID_FIPS_STATE_COUNTY_LOCN";
              }
              else
              {
                var field = GetField(export.LegalAction, "courtCaseNumber");

                field.Error = true;

                ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
              }

              return;
            }
            else
            {
              if (ReadLegalAction2())
              {
                export.LegalAction.Identifier = entities.LegalAction.Identifier;
              }

              if (export.LegalAction.Identifier == 0)
              {
                export.Cases.Count = 0;

                var field = GetField(export.Foreign, "country");

                field.Color = "red";
                field.Intensity = Intensity.High;
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;

                if (ReadFipsTribAddress2())
                {
                  var field1 = GetField(export.LegalAction, "courtCaseNumber");

                  field1.Error = true;

                  ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
                }
                else
                {
                  ExitState = "FN0000_NO_FIPS_ADDRESS_FOUND";
                }

                return;
              }
            }
          }
        }

        if (ReadTribunal())
        {
          export.Tribunal.Assign(entities.Tribunal);
        }
        else
        {
          export.Cases.Count = 0;
          ExitState = "LE0000_TRIBUNAL_NF";

          return;
        }

        if (ReadFips2())
        {
          export.Fips.Assign(entities.Fips);
        }
        else if (ReadFipsTribAddress1())
        {
          export.Foreign.Country = entities.ExistingForeign.Country;
        }
        else
        {
          export.Cases.Count = 0;
          ExitState = "FIPS_NF";

          return;
        }

        MoveLegalAction(export.LegalAction, export.PreviousLegalAction);
        MoveFips(export.Fips, export.PreviousFips);
        export.PreviousFipsTribAddress.Country = export.Foreign.Country;
        UseLeGetPetitionerRespondent();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Cases.Count = 0;

          return;
        }

        export.Cases.Index = -1;

        // mjr
        // ----------------------------------
        // 10/05/2000
        // changed READ EACH to use export legal_action,
        // rather than CURRENT legal_action - START
        // -----------------------------------------------
        // mjr
        // ----------------------------------
        // 10/16/2000
        // PR# 105376 - Changed READ EACH to look for Cases
        // related to all legal actions, not just the one identified by
        // legal_action identifier
        // -----------------------------------------------
        foreach(var item in ReadCase())
        {
          UseSpCabDetOspAssgndToCsecase();

          if (IsEmpty(local.Spd.LastName) && IsEmpty(local.Spd.FirstName) && IsEmpty
            (local.Spd.MiddleInitial))
          {
            local.SpdName.Text30 = "";
          }
          else
          {
            local.SpdName.Text30 = TrimEnd(local.Spd.LastName) + ", " + TrimEnd
              (local.Spd.FirstName) + " " + local.Spd.MiddleInitial;
          }

          // CQ66294 - Reset exit state after caseworker lookup to allow an open
          // case
          // to display correctly after a closed case
          ExitState = "ACO_NN0000_ALL_OK";

          if (AsChar(entities.ExistingCase.Status) == 'O')
          {
            local.ApCount.Count = 0;

            foreach(var item1 in ReadCaseRoleCsePerson())
            {
              local.ApCount.Count = (int)((long)local.ApCount.Count + 1);
              local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
              UseCabReadAdabasPerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto ReadEach;
              }

              UseSiFormatCsePersonName();

              if (export.Cases.Index + 1 >= Export.CasesGroup.Capacity)
              {
                goto ReadEach;
              }

              ++export.Cases.Index;
              export.Cases.CheckSize();

              export.Cases.Update.Common.SelectChar = "";
              export.Cases.Update.Case1.Assign(entities.ExistingCase);
              MoveOffice(local.Office, export.Cases.Update.DetailOffice);
              export.Cases.Update.TextWorkArea.Text30 = local.SpdName.Text30;
              export.Cases.Update.CsePersonsWorkSet.FormattedName =
                local.CsePersonsWorkSet.FormattedName;
            }

            // Ensure case detail is displayed if there is no active AP on open 
            // case
            if (local.ApCount.Count <= 0)
            {
              if (export.Cases.Index + 1 >= Export.CasesGroup.Capacity)
              {
                break;
              }

              ++export.Cases.Index;
              export.Cases.CheckSize();

              export.Cases.Update.Common.SelectChar = "";
              export.Cases.Update.Case1.Assign(entities.ExistingCase);
              MoveOffice(local.Office, export.Cases.Update.DetailOffice);
              export.Cases.Update.TextWorkArea.Text30 = local.SpdName.Text30;
              export.Cases.Update.CsePersonsWorkSet.FormattedName = "";
            }
          }
          else
          {
            if (export.Cases.Index + 1 >= Export.CasesGroup.Capacity)
            {
              break;
            }

            ++export.Cases.Index;
            export.Cases.CheckSize();

            export.Cases.Update.Common.SelectChar = "";
            export.Cases.Update.Case1.Assign(entities.ExistingCase);
            MoveOffice(local.Office, export.Cases.Update.DetailOffice);
            export.Cases.Update.TextWorkArea.Text30 = local.SpdName.Text30;
            export.Cases.Update.CsePersonsWorkSet.FormattedName = "";
          }
        }

ReadEach:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!export.Cases.IsEmpty && IsExitState("CASE_ASSIGNMENT_NF"))
          {
            export.DataExists.Flag = "Y";
          }
        }
        else if (export.Cases.IsEmpty)
        {
          export.DataExists.Flag = "N";
          ExitState = "ACO_NI0000_NO_RECORDS_FOUND";
        }
        else if (export.Cases.IsFull)
        {
          export.DataExists.Flag = "Y";
          ExitState = "OE0000_LIST_IS_FULL";
        }
        else
        {
          export.DataExists.Flag = "Y";
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        // -----------------------
        // End 11/27/98 changes
        // -----------------------
        break;
      case "RETURN":
        local.TotalSelected.Count = 0;

        for(export.Cases.Index = 0; export.Cases.Index < export.Cases.Count; ++
          export.Cases.Index)
        {
          if (!export.Cases.CheckSize())
          {
            break;
          }

          if (AsChar(export.Cases.Item.Common.SelectChar) == 'S')
          {
            ++local.TotalSelected.Count;

            if (local.TotalSelected.Count > 10)
            {
              break;
            }

            export.DialogFlowSelectd.Index = local.TotalSelected.Count - 1;
            export.DialogFlowSelectd.CheckSize();

            export.DialogFlowSelectd.Update.DlgflwSelectd.Assign(
              export.Cases.Item.Case1);
          }
        }

        export.Cases.CheckIndex();
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

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

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
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

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSpCabDetOspAssgndToCsecase()
  {
    var useImport = new SpCabDetOspAssgndToCsecase.Import();
    var useExport = new SpCabDetOspAssgndToCsecase.Export();

    useImport.Case1.Number = entities.ExistingCase.Number;

    Call(SpCabDetOspAssgndToCsecase.Execute, useImport, useExport);

    local.Spd.Assign(useExport.ServiceProvider);
    MoveOffice(useExport.Office, local.Office);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.ExistingCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(command, "endDate", date);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Type1 = db.GetString(reader, 5);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.ExistingForeign.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingForeign.Identifier = db.GetInt32(reader, 0);
        entities.ExistingForeign.Country = db.GetNullableString(reader, 1);
        entities.ExistingForeign.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingForeign.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.ExistingForeign.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableString(command, "country", export.Foreign.Country ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingForeign.Identifier = db.GetInt32(reader, 0);
        entities.ExistingForeign.Country = db.GetNullableString(reader, 1);
        entities.ExistingForeign.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingForeign.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(command, "country", export.Foreign.Country ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
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
    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of DetailOffice.
      /// </summary>
      [JsonPropertyName("detailOffice")]
      public Office DetailOffice
      {
        get => detailOffice ??= new();
        set => detailOffice = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
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
      /// A value of TextWorkArea.
      /// </summary>
      [JsonPropertyName("textWorkArea")]
      public TextWorkArea TextWorkArea
      {
        get => textWorkArea ??= new();
        set => textWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private Case1 case1;
      private Office detailOffice;
      private ServiceProvider detailServiceProvider;
      private CsePersonsWorkSet csePersonsWorkSet;
      private TextWorkArea textWorkArea;
    }

    /// <summary>
    /// A value of PreviousTribunal.
    /// </summary>
    [JsonPropertyName("previousTribunal")]
    public Tribunal PreviousTribunal
    {
      get => previousTribunal ??= new();
      set => previousTribunal = value;
    }

    /// <summary>
    /// A value of PreviousFipsTribAddress.
    /// </summary>
    [JsonPropertyName("previousFipsTribAddress")]
    public FipsTribAddress PreviousFipsTribAddress
    {
      get => previousFipsTribAddress ??= new();
      set => previousFipsTribAddress = value;
    }

    /// <summary>
    /// A value of PreviousFips.
    /// </summary>
    [JsonPropertyName("previousFips")]
    public Fips PreviousFips
    {
      get => previousFips ??= new();
      set => previousFips = value;
    }

    /// <summary>
    /// A value of DataExists.
    /// </summary>
    [JsonPropertyName("dataExists")]
    public Common DataExists
    {
      get => dataExists ??= new();
      set => dataExists = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
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
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
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
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public Fips DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    private Tribunal previousTribunal;
    private FipsTribAddress previousFipsTribAddress;
    private Fips previousFips;
    private Common dataExists;
    private LegalAction previousLegalAction;
    private FipsTribAddress foreign;
    private Common promptTribunal;
    private Fips fips;
    private Tribunal tribunal;
    private ServiceProvider selectedServiceProvider;
    private Office selectedOffice;
    private Case1 selectedCase;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private LegalAction legalAction;
    private Array<CasesGroup> cases;
    private Fips dlgflwSelected;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of DetailOffice.
      /// </summary>
      [JsonPropertyName("detailOffice")]
      public Office DetailOffice
      {
        get => detailOffice ??= new();
        set => detailOffice = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
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
      /// A value of TextWorkArea.
      /// </summary>
      [JsonPropertyName("textWorkArea")]
      public TextWorkArea TextWorkArea
      {
        get => textWorkArea ??= new();
        set => textWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private Case1 case1;
      private Office detailOffice;
      private ServiceProvider detailServiceProvider;
      private CsePersonsWorkSet csePersonsWorkSet;
      private TextWorkArea textWorkArea;
    }

    /// <summary>A DialogFlowSelectdGroup group.</summary>
    [Serializable]
    public class DialogFlowSelectdGroup
    {
      /// <summary>
      /// A value of DlgflwSelectd.
      /// </summary>
      [JsonPropertyName("dlgflwSelectd")]
      public Case1 DlgflwSelectd
      {
        get => dlgflwSelectd ??= new();
        set => dlgflwSelectd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 dlgflwSelectd;
    }

    /// <summary>
    /// A value of PreviousFipsTribAddress.
    /// </summary>
    [JsonPropertyName("previousFipsTribAddress")]
    public FipsTribAddress PreviousFipsTribAddress
    {
      get => previousFipsTribAddress ??= new();
      set => previousFipsTribAddress = value;
    }

    /// <summary>
    /// A value of PreviousFips.
    /// </summary>
    [JsonPropertyName("previousFips")]
    public Fips PreviousFips
    {
      get => previousFips ??= new();
      set => previousFips = value;
    }

    /// <summary>
    /// A value of DataExists.
    /// </summary>
    [JsonPropertyName("dataExists")]
    public Common DataExists
    {
      get => dataExists ??= new();
      set => dataExists = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
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
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
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
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
    }

    /// <summary>
    /// Gets a value of DialogFlowSelectd.
    /// </summary>
    [JsonIgnore]
    public Array<DialogFlowSelectdGroup> DialogFlowSelectd =>
      dialogFlowSelectd ??= new(DialogFlowSelectdGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DialogFlowSelectd for json serialization.
    /// </summary>
    [JsonPropertyName("dialogFlowSelectd")]
    [Computed]
    public IList<DialogFlowSelectdGroup> DialogFlowSelectd_Json
    {
      get => dialogFlowSelectd;
      set => DialogFlowSelectd.Assign(value);
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

    private FipsTribAddress previousFipsTribAddress;
    private Fips previousFips;
    private Common dataExists;
    private LegalAction previousLegalAction;
    private FipsTribAddress foreign;
    private Common promptTribunal;
    private Fips fips;
    private Tribunal tribunal;
    private ServiceProvider selectedServiceProvider;
    private Office selectedOffice;
    private Case1 selectedCase;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private LegalAction legalAction;
    private Array<CasesGroup> cases;
    private Array<DialogFlowSelectdGroup> dialogFlowSelectd;
    private AbendData abendData;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NonSpaces.
    /// </summary>
    [JsonPropertyName("nonSpaces")]
    public Common NonSpaces
    {
      get => nonSpaces ??= new();
      set => nonSpaces = value;
    }

    /// <summary>
    /// A value of PreviousCommon.
    /// </summary>
    [JsonPropertyName("previousCommon")]
    public Common PreviousCommon
    {
      get => previousCommon ??= new();
      set => previousCommon = value;
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
    /// A value of PreviousCase.
    /// </summary>
    [JsonPropertyName("previousCase")]
    public Case1 PreviousCase
    {
      get => previousCase ??= new();
      set => previousCase = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of Spd.
    /// </summary>
    [JsonPropertyName("spd")]
    public ServiceProvider Spd
    {
      get => spd ??= new();
      set => spd = value;
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
    /// A value of SpdName.
    /// </summary>
    [JsonPropertyName("spdName")]
    public TextWorkArea SpdName
    {
      get => spdName ??= new();
      set => spdName = value;
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
    /// A value of ApCount.
    /// </summary>
    [JsonPropertyName("apCount")]
    public Common ApCount
    {
      get => apCount ??= new();
      set => apCount = value;
    }

    private Common nonSpaces;
    private Common previousCommon;
    private TextWorkArea textWorkArea;
    private Case1 previousCase;
    private Common totalSelected;
    private NextTranInfo nextTranInfo;
    private ServiceProvider spd;
    private Office office;
    private TextWorkArea spdName;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common apCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalActionCaseRole")]
    public LegalActionCaseRole ExistingLegalActionCaseRole
    {
      get => existingLegalActionCaseRole ??= new();
      set => existingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingForeign.
    /// </summary>
    [JsonPropertyName("existingForeign")]
    public FipsTribAddress ExistingForeign
    {
      get => existingForeign ??= new();
      set => existingForeign = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
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

    private LegalActionCaseRole existingLegalActionCaseRole;
    private FipsTribAddress existingForeign;
    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private CaseRole caseRole;
    private CsePerson csePerson;
  }
#endregion
}

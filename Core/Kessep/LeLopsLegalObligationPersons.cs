// Program: LE_LOPS_LEGAL_OBLIGATION_PERSONS, ID: 372006173, model: 746.
// Short name: SWELOPSP
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
/// A program: LE_LOPS_LEGAL_OBLIGATION_PERSONS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLopsLegalObligationPersons: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LOPS_LEGAL_OBLIGATION_PERSONS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLopsLegalObligationPersons(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLopsLegalObligationPersons.
  /// </summary>
  public LeLopsLegalObligationPersons(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 08/07/95  Dave Allen			Initial Code
    // 12/22/95  T.O.Redmond			Add Petitioner/Respondent
    // 01/03/97  R. Marchman			Add new security/next tran.
    // 05/01/97  govind	Prob Rep	Fixed not to expect select char for the
    // 					record before PF4. Fixed not to clear Select
    // 					char if update not successful,  Fixed error
    // 					message text for obligor/ obligation amount
    // 					not specified.
    // 10/07/97  R Grey	H00029874	Set Effective Date screen field property to
    // 					'Protected'
    // 10/26/97  R Grey	H00030824	Fix Exist State msg and use CAB for Code
    // 					Value read
    // 10/31/97  Govind	PR 30861	Fixed the fix to protect/unprotect the fields
    // 					when a dbet has been set up.
    // 02/10/98  R Grey	H00034760/	Protection / unprotection le act person
    // 			H00034764	eff/end dates
    // 01/04/99  M Ramirez			Revised print process.
    // 01/28/99  P. Sharp  			Made changed per Phase II assessment sheet.
    // 					IDCR 444 and 443.
    // 					Problem reports
    // 04/28/99  M Ramirez			Re-revised print process.
    // 09/21/99  Bud Adams	PR# 74353	Added edit to prevent the same supported
    // 					person from being selected more than once on
    // 					a specific legal detail.
    // 01/03/00  Anand Katuri	PR# H00082508	Do not delete on LOPS once the 
    // financial
    // 					details are established.
    // 08/22/00  GVandy	PR# H00101595	Do not allow a non-case related person to 
    // be
    // 					added as an obligor for "J" class legal
    // 					actions.
    // 09/18/01  GVandy	WR 20127	For J class legal action details LOPS is now
    // 					mandatory.  Also, the user must return to 					LDET via F9 to 
    // continue.
    // 04/02/02  GVandy	PR# 138221	Read for end dated code values when
    // 					retrieving action taken description.
    // 09/30/02  GVandy	WR020120	Finance archive changes and screen protection
    // 					logic simplification.
    // 09/12/03  GVandy	PR186785	Set date_patern_estab when a legal action
    // 					containing an EP legal detail is filed.
    // 05/10/17  GVandy	CQ48108		IV-D PEP changes.
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    UseSpDocSetLiterals();

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveLegalAction2(import.LegalAction, export.LegalAction);
    export.LegalActionDetail.Assign(import.LegalActionDetail);
    export.Classification.Text11 = import.Classification.Text11;
    export.Fips.Assign(import.Fips);
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.Tribunal.Assign(import.Tribunal);
    export.ObligationType.Code = import.ObligationType.Code;
    export.ActionTaken.Description = import.ActionTaken.Description;
    export.HiddenReturnRequired.Flag = import.HiddenReturnRequired.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      export.CseCases.Index = 0;
      export.CseCases.CheckSize();

      export.CseCases.Update.Cse.Number = "";

      return;
    }

    export.CseCases.Index = -1;
    import.CseCases.Index = 0;

    for(var limit = import.CseCases.Count; import.CseCases.Index < limit; ++
      import.CseCases.Index)
    {
      if (!import.CseCases.CheckSize())
      {
        break;
      }

      if (!IsEmpty(import.CseCases.Item.Cse.Number))
      {
        ++export.CseCases.Index;
        export.CseCases.CheckSize();

        export.CseCases.Update.Cse.Number = import.CseCases.Item.Cse.Number;
        local.TextWorkArea.Text10 = export.CseCases.Item.Cse.Number;
        UseEabPadLeftWithZeros();
        export.CseCases.Update.Cse.Number = local.TextWorkArea.Text10;
      }
    }

    import.CseCases.CheckIndex();

    // -------------------------------------------------------------
    // Make sure that one blank entry exists at the end so that
    // the user can enter the next cse case number. Note that the
    // 'unused entries are protected' on the screen and this option
    // is applied by IEF to both the repeating groups.
    // -------------------------------------------------------------
    if (export.CseCases.IsEmpty)
    {
      export.CseCases.Index = 0;
      export.CseCases.CheckSize();

      export.CseCases.Update.Cse.Number = "";
    }
    else
    {
      export.CseCases.Index = export.CseCases.Count - 1;
      export.CseCases.CheckSize();

      if (IsEmpty(export.CseCases.Item.Cse.Number))
      {
        // --- A blank entry already exists.
      }
      else if (export.CseCases.Index + 1 < Export.CseCasesGroup.Capacity)
      {
        ++export.CseCases.Index;
        export.CseCases.CheckSize();

        export.CseCases.Update.Cse.Number = "";
      }
    }

    export.PromptCase.SelectChar = import.PromptCase.SelectChar;

    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "RETLACN") || Equal(global.Command, "RETLAPS"))
    {
      // ---- Dont move group imports to group exports. They will be populated 
      // afresh now.
    }
    else
    {
      // 09/30/02 GVandy WR020120 -- Finance archive changes and screen field 
      // protection logic simplification below.
      if (import.ObligationPersons.IsEmpty)
      {
        goto Test1;
      }

      if (ReadObligationTypeLegalActionLegalActionDetail())
      {
        local.ObligationType.Classification =
          entities.ObligationType.Classification;
      }
      else
      {
        local.ObligationType.Classification = "";
      }

      UseLeCabCheckOblgSetUpFLdet();

      export.ObligationPersons.Index = 0;
      export.ObligationPersons.Clear();

      for(import.ObligationPersons.Index = 0; import.ObligationPersons.Index < import
        .ObligationPersons.Count; ++import.ObligationPersons.Index)
      {
        if (export.ObligationPersons.IsFull)
        {
          break;
        }

        export.ObligationPersons.Update.ListEndReason.PromptField =
          import.ObligationPersons.Item.ListEndReason.PromptField;
        export.ObligationPersons.Update.Common.SelectChar =
          import.ObligationPersons.Item.Common.SelectChar;
        export.ObligationPersons.Update.LegalActionPerson.Assign(
          import.ObligationPersons.Item.LegalActionPerson);
        export.ObligationPersons.Update.Case1.Number =
          import.ObligationPersons.Item.Case1.Number;
        export.ObligationPersons.Update.CaseRole.Assign(
          import.ObligationPersons.Item.CaseRole);
        MoveCsePersonsWorkSet(import.ObligationPersons.Item.CsePersonsWorkSet,
          export.ObligationPersons.Update.CsePersonsWorkSet);

        if (Equal(export.ObligationPersons.Item.LegalActionPerson.EndDate,
          local.MaxDate.Date))
        {
          export.ObligationPersons.Update.LegalActionPerson.EndDate =
            local.Initialize.EndDate;
        }

        // --- First unprotect all the enterable fields; then protect based on 
        // the debt set up
        var field1 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "accountType");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;

        var field2 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "accountType");

        field2.Color = "green";
        field2.Highlighting = Highlighting.Underscore;
        field2.Protected = false;

        var field3 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "effectiveDate");

        field3.Color = "green";
        field3.Highlighting = Highlighting.Underscore;
        field3.Protected = false;

        var field4 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "currentAmount");

        field4.Color = "green";
        field4.Highlighting = Highlighting.Underscore;
        field4.Protected = false;

        var field5 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "arrearsAmount");

        field5.Color = "green";
        field5.Highlighting = Highlighting.Underscore;
        field5.Protected = false;

        var field6 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "judgementAmount");

        field6.Color = "green";
        field6.Highlighting = Highlighting.Underscore;
        field6.Protected = false;

        var field7 =
          GetField(export.ObligationPersons.Item.LegalActionPerson, "endDate");

        field7.Color = "green";
        field7.Highlighting = Highlighting.Underscore;
        field7.Protected = false;

        var field8 =
          GetField(export.ObligationPersons.Item.LegalActionPerson, "endReason");
          

        field8.Color = "green";
        field8.Highlighting = Highlighting.Underscore;
        field8.Protected = false;

        if (Equal(entities.LegalActionDetail.CreatedBy, "CONVERSN") && Equal
          (Date(entities.LegalActionDetail.LastUpdatedTstamp), Now().Date))
        {
          // Allow the user one opportunity to update conversion information.
          // In the update cab, the legal detail created_by will be changed to '
          // CONVERSX'.
          export.ObligationPersons.Next();

          continue;
        }

        // *** Obligee amounts fields should be for reference only. Per Jan 
        // Brigham 2/3/98.
        if (AsChar(export.ObligationPersons.Item.LegalActionPerson.AccountType) ==
          'E')
        {
          var field9 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "currentAmount");

          field9.Color = "cyan";
          field9.Highlighting = Highlighting.Underscore;
          field9.Protected = true;

          var field10 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "arrearsAmount");

          field10.Color = "cyan";
          field10.Highlighting = Highlighting.Underscore;
          field10.Protected = true;

          var field11 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "judgementAmount");

          field11.Color = "cyan";
          field11.Highlighting = Highlighting.Underscore;
          field11.Protected = true;
        }

        // ***  Once an obligation is established, protect the account type, 
        // effective date, and current,
        // arrears, and judgement amounts.
        if (AsChar(local.OblgSetUpForLdet.Flag) == 'Y')
        {
          var field9 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "accountType");

          field9.Color = "cyan";
          field9.Highlighting = Highlighting.Underscore;
          field9.Protected = true;

          var field10 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "effectiveDate");

          field10.Color = "cyan";
          field10.Highlighting = Highlighting.Underscore;
          field10.Protected = true;

          var field11 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "currentAmount");

          field11.Color = "cyan";
          field11.Highlighting = Highlighting.Underscore;
          field11.Protected = true;

          var field12 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "arrearsAmount");

          field12.Color = "cyan";
          field12.Highlighting = Highlighting.Underscore;
          field12.Protected = true;

          var field13 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "judgementAmount");

          field13.Color = "cyan";
          field13.Highlighting = Highlighting.Underscore;
          field13.Protected = true;

          // ***  If the obligation is still active then protect the end date 
          // and reason.
          if (AsChar(local.OblgActiveForLdet.Flag) == 'Y')
          {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // This was apparently changed at some point so that the end date 
            // and reason are NOT
            // protected.  This whole IF could be deleted but I am leaving it 
            // for future reference.
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
          }
        }

        export.ObligationPersons.Next();
      }
    }

Test1:

    if (Equal(global.Command, "EXIT"))
    {
      if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
      {
        // -- WR 20127 The user must return to LDET when adding J class legal 
        // details.
        ExitState = "LE0000_MUST_RETURN_TO_LDET";
      }
      else
      {
        ExitState = "ECO_LNK_RETURN_TO_MENU";
      }

      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
      {
        // -- WR 20127 The user must add LOPS info before returning to LDET for 
        // J class legal details.
        ExitState = "LE0000_LOPS_REQUIRED";

        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (export.ObligationPersons.Item.LegalActionPerson.Identifier != 0)
          {
            ExitState = "ACO_NE0000_RETURN";

            goto Test2;
          }
        }
      }
      else
      {
        ExitState = "ACO_NE0000_RETURN";
      }

Test2:

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
      {
        // -- WR 20127 The user must return to LDET when adding J class legal 
        // details.
        ExitState = "LE0000_MUST_RETURN_TO_LDET";
      }
      else
      {
        UseScCabSignoff();
      }

      return;
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
      ExitState = "LE0000_CANT_NEXTTRAN_INTO";

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
      {
        // -- WR 20127 The user must return to LDET when adding J class legal 
        // details.
        ExitState = "LE0000_MUST_RETURN_TO_LDET";
      }
      else
      {
        // ---------------------------------------------
        // User is going out of this screen to another
        // ---------------------------------------------
        // ---------------------------------------------
        // Set up local next_tran_info for saving the current values for the 
        // next screen
        // ---------------------------------------------
        local.NextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;
        local.NextTranInfo.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber ?? "";
        UseScCabNextTranPut1();
      }

      return;
    }

    // mjr
    // -----------------------------------------------------------
    // 01/04/1999
    // Changed security to check CRUD actions only.
    // ------------------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
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
    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    // ---------------------------------------------
    //             E D I T    L O G I C
    // ---------------------------------------------
    for(export.CseCases.Index = 0; export.CseCases.Index < export
      .CseCases.Count; ++export.CseCases.Index)
    {
      if (!export.CseCases.CheckSize())
      {
        break;
      }

      if (!IsEmpty(export.CseCases.Item.Cse.Number))
      {
        if (!ReadCase())
        {
          var field = GetField(export.CseCases.Item.Cse, "number");

          field.Error = true;

          ExitState = "CASE_NF";

          return;
        }
      }
    }

    export.CseCases.CheckIndex();

    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    // mjr---> Added check for PrintRet
    if (!Equal(global.Command, "PRINTRET"))
    {
      if (!ReadLegalActionDetailLegalAction())
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";

        return;
      }
    }

    // -------------------------------------------------------------
    // Command ADD is treated as command UPDATE. ADD is
    // provided only for consistency with other legal screens
    // -------------------------------------------------------------
    if (Equal(global.Command, "ADD"))
    {
      local.CommandWasAdd.Flag = "Y";
      global.Command = "UPDATE";
    }
    else
    {
      local.CommandWasAdd.Flag = "N";
    }

    if (!Equal(global.Command, "LIST") && !
      Equal(global.Command, "FIRSTIME") && !
      Equal(global.Command, "LISTPERS") && !
      Equal(global.Command, "DISPLAY") && !Equal(global.Command, "NAME") && !
      Equal(global.Command, "RETNAME") && !
      Equal(global.Command, "PRINTRET") && !Equal(global.Command, "FROMLDET"))
    {
      // mjr---> Added check for PrintRet
      export.CseCases.Index = 0;
      export.CseCases.CheckSize();

      if (IsEmpty(export.CseCases.Item.Cse.Number))
      {
        var field = GetField(export.CseCases.Item.Cse, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }

    // -----------------------------------------------------
    // At least one row must be selected on Update or Delete
    // -----------------------------------------------------
    local.TotalSelected.Count = 0;

    for(export.ObligationPersons.Index = 0; export.ObligationPersons.Index < export
      .ObligationPersons.Count; ++export.ObligationPersons.Index)
    {
      if (AsChar(export.ObligationPersons.Item.Common.SelectChar) == 'S')
      {
        ++local.TotalSelected.Count;
      }
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (local.TotalSelected.Count < 1)
      {
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          var field =
            GetField(export.ObligationPersons.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }
      }
    }

    // ---------------------------------------------
    // Validate Sel.
    // ---------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      for(export.ObligationPersons.Index = 0; export.ObligationPersons.Index < export
        .ObligationPersons.Count; ++export.ObligationPersons.Index)
      {
        switch(AsChar(export.ObligationPersons.Item.Common.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            break;
          default:
            var field =
              GetField(export.ObligationPersons.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        // ---------------------------------------------
        // Validate Type.
        // ---------------------------------------------
        if (AsChar(export.ObligationPersons.Item.Common.SelectChar) == 'S')
        {
          // -----------------------------------------------------------------
          // An edit that checked group_export legal_action_person
          // account_type for nonspace has been removed in order to
          // allow specifying just case role only. The corresponding exit
          // state it was required_data_missing.
          // -----------------------------------------------------------------
          // -----------------------------------------------------------------
          // Allow blank account type. This will permit the user to specify
          // only the case role so that legal action person is created
          // only once but the la person la case role will be created
          // multiple times. If no legal action person record exists
          // already, the update action block will trap the error and return
          // the error.
          // -----------------------------------------------------------------
          switch(AsChar(export.ObligationPersons.Item.LegalActionPerson.
            AccountType))
          {
            case ' ':
              break;
            case 'E':
              // Obligee
              break;
            case 'R':
              // Obligor
              // -----------------------------------------------------------------
              // PR# 74353: 9/21/99 - bud adams  -  On Joint & Several
              //   obligations, e.g., the same child will be displayed more
              //   than once, but a specific child can be selected no more
              //   once for any obligation.
              // -----------------------------------------------------------------
              if (Equal(global.Command, "UPDATE"))
              {
                for(import.ObligationPersons.Index = 0; import
                  .ObligationPersons.Index < import.ObligationPersons.Count; ++
                  import.ObligationPersons.Index)
                {
                  if (Equal(import.ObligationPersons.Item.CsePersonsWorkSet.
                    Number,
                    export.ObligationPersons.Item.CsePersonsWorkSet.Number))
                  {
                    if (AsChar(import.ObligationPersons.Item.LegalActionPerson.
                      AccountType) == 'R' && !
                      Lt(Now().Date,
                      import.ObligationPersons.Item.CaseRole.EndDate))
                    {
                      ++local.Dummy.Count;
                    }

                    if (local.Dummy.Count > 1)
                    {
                      local.SelectedCh.Number =
                        export.ObligationPersons.Item.CsePersonsWorkSet.Number;
                      ExitState = "LE0000_SELECT_OBLIGOR_ONLY_ONCE";

                      goto AfterCycle;
                    }
                    else
                    {
                    }
                  }
                }
              }

              break;
            case 'S':
              // Supported Child
              // -----------------------------------------------------------------
              // PR# 74353: 9/21/99 - bud adams  -  On Joint & Several
              //   obligations, e.g., the same child will be displayed more
              //   than once, but a specific child can be selected no more
              //   once for any obligation.
              // -----------------------------------------------------------------
              if (Equal(global.Command, "UPDATE"))
              {
                for(import.ObligationPersons.Index = 0; import
                  .ObligationPersons.Index < import.ObligationPersons.Count; ++
                  import.ObligationPersons.Index)
                {
                  if (Equal(import.ObligationPersons.Item.CsePersonsWorkSet.
                    Number,
                    export.ObligationPersons.Item.CsePersonsWorkSet.Number))
                  {
                    if (AsChar(import.ObligationPersons.Item.LegalActionPerson.
                      AccountType) == 'S' && !
                      Lt(Now().Date,
                      import.ObligationPersons.Item.CaseRole.EndDate))
                    {
                      ++local.Dummy.Count;
                    }
                  }

                  if (local.Dummy.Count > 1)
                  {
                    local.SelectedCh.Number =
                      export.ObligationPersons.Item.CsePersonsWorkSet.Number;
                    ExitState = "LE0000_SELECT_CHILDREN_ONLY_ONCE";

                    goto AfterCycle;
                  }
                }
              }

              break;
            default:
              var field =
                GetField(export.ObligationPersons.Item.LegalActionPerson,
                "accountType");

              field.Error = true;

              ExitState = "LE0000_INVALID_OBLIG_PERSON_TYPE";

              return;
          }

          if (!Lt(local.Initialize.EffectiveDate,
            export.ObligationPersons.Item.LegalActionPerson.EffectiveDate))
          {
            export.ObligationPersons.Update.LegalActionPerson.EffectiveDate =
              entities.LegalActionDetail.EffectiveDate;
          }

          if (!Lt(local.Initialize.EndDate,
            export.ObligationPersons.Item.LegalActionPerson.EndDate))
          {
            if (Lt(entities.LegalActionDetail.EndDate, local.MaxDate.Date))
            {
              export.ObligationPersons.Update.LegalActionPerson.EndDate =
                entities.LegalActionDetail.EndDate;
            }
          }

          if (Lt(local.Initialize.EndDate,
            export.ObligationPersons.Item.LegalActionPerson.EndDate))
          {
            if (Lt(export.ObligationPersons.Item.LegalActionPerson.EndDate,
              export.ObligationPersons.Item.LegalActionPerson.EffectiveDate))
            {
              ExitState = "LE0000_END_DATE_LT_EFF_DATE";

              var field =
                GetField(export.ObligationPersons.Item.LegalActionPerson,
                "endDate");

              field.Error = true;

              return;
            }
          }

          if (!IsEmpty(export.ObligationPersons.Item.LegalActionPerson.EndReason)
            || Lt
            (local.Initialize.EndDate,
            export.ObligationPersons.Item.LegalActionPerson.EndDate))
          {
            if (!Lt(local.Initialize.EndDate,
              export.ObligationPersons.Item.LegalActionPerson.EndDate))
            {
              var field =
                GetField(export.ObligationPersons.Item.LegalActionPerson,
                "endDate");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            if (IsEmpty(export.ObligationPersons.Item.LegalActionPerson.
              EndReason))
            {
              var field =
                GetField(export.ObligationPersons.Item.LegalActionPerson,
                "endReason");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            local.Code.CodeName = "LEGAL OBLIGATION END REASON";
            local.CodeValue.Cdvalue =
              export.ObligationPersons.Item.LegalActionPerson.EndReason ?? Spaces
              (10);
            UseCabValidateCodeValue1();

            if (AsChar(local.ValidCode.Flag) == 'N')
            {
              ExitState = "LE0000_INVALID_LOPS_END_REASON";

              var field =
                GetField(export.ObligationPersons.Item.LegalActionPerson,
                "endReason");

              field.Error = true;

              return;
            }
          }

          // --05/10/17  GVandy CQ48108 (IV-D PEP Changes) Do not allow Supporte
          // person on an EP
          //   legal detail to be end dated or deleted if the child has 
          // paternity info locked.
          if (Equal(global.Command, "DELETE") || Lt
            (local.Initialize.EndDate,
            export.ObligationPersons.Item.LegalActionPerson.EndDate))
          {
            if (Equal(export.ObligationType.Code, "EP") && AsChar
              (export.ObligationPersons.Item.LegalActionPerson.AccountType) == 'S'
              )
            {
              if (ReadCsePerson2())
              {
                var field =
                  GetField(export.ObligationPersons.Item.Common, "selectChar");

                field.Error = true;

                if (Equal(global.Command, "DELETE"))
                {
                  ExitState = "LE0000_CANNOT_DELETE_PAT_LOCKED";
                }
                else
                {
                  ExitState = "LE0000_CANNOT_END_DT_PAT_LOCKED";
                }
              }
            }
          }

          if (Equal(global.Command, "UPDATE"))
          {
            local.Ogligee.Count = 0;

            if (AsChar(export.ObligationPersons.Item.LegalActionPerson.
              AccountType) == 'E')
            {
              if (export.ObligationPersons.Item.LegalActionPerson.CurrentAmount.
                GetValueOrDefault() > 0)
              {
                var field =
                  GetField(export.ObligationPersons.Item.LegalActionPerson,
                  "currentAmount");

                field.Error = true;

                ++local.Ogligee.Count;
              }

              if (export.ObligationPersons.Item.LegalActionPerson.ArrearsAmount.
                GetValueOrDefault() > 0)
              {
                var field =
                  GetField(export.ObligationPersons.Item.LegalActionPerson,
                  "arrearsAmount");

                field.Error = true;

                ++local.Ogligee.Count;
              }

              if (export.ObligationPersons.Item.LegalActionPerson.
                JudgementAmount.GetValueOrDefault() > 0)
              {
                var field =
                  GetField(export.ObligationPersons.Item.LegalActionPerson,
                  "judgementAmount");

                field.Error = true;

                ++local.Ogligee.Count;
              }

              if (local.Ogligee.Count >= 1)
              {
                ExitState = "LE0000_NO_AMOUNTS_FOR_OBLIGEE";

                return;
              }
            }
          }

          local.Dummy.Count = 0;
        }
      }

AfterCycle:

      // Part of PR# 74353  -  bud adams  -  9/21/99
      if (IsExitState("LE0000_SELECT_CHILDREN_ONLY_ONCE") || IsExitState
        ("LE0000_SELECT_OBLIGOR_ONLY_ONCE"))
      {
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (Equal(export.ObligationPersons.Item.CsePersonsWorkSet.Number,
            local.SelectedCh.Number))
          {
            var field =
              GetField(export.ObligationPersons.Item.CsePersonsWorkSet, "number");
              

            field.Color = "yellow";
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }

          if (AsChar(export.ObligationPersons.Item.Common.SelectChar) == 'S')
          {
            var field =
              GetField(export.ObligationPersons.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "CPAT":
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (IsEmpty(export.ObligationPersons.Item.Common.SelectChar))
          {
            continue;
          }

          export.Selected.Number = export.ObligationPersons.Item.Case1.Number;

          if (AsChar(export.ObligationPersons.Item.LegalActionPerson.AccountType)
            == 'S')
          {
            break;
          }
        }

        if (IsEmpty(export.Selected.Number))
        {
          export.CseCases.Index = 0;
          export.CseCases.CheckSize();

          export.Selected.Number = export.CseCases.Item.Cse.Number;
        }

        ExitState = "ECO_LNK_TO_CPAT";

        break;
      case "ROLE":
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (IsEmpty(export.ObligationPersons.Item.Common.SelectChar))
          {
            continue;
          }

          export.Selected.Number = export.ObligationPersons.Item.Case1.Number;

          if (AsChar(export.ObligationPersons.Item.LegalActionPerson.AccountType)
            == 'S')
          {
            break;
          }
        }

        if (IsEmpty(export.Selected.Number))
        {
          export.CseCases.Index = 0;
          export.CseCases.CheckSize();

          export.Selected.Number = export.CseCases.Item.Cse.Number;
        }

        ExitState = "ECO_LNK_TO_ROLE";

        break;
      case "RETLINK":
        // -- No processing required.
        break;
      case "FROMLDET":
        // -- 09/18/01 WR 20127 This command is sent from LDET when the user is 
        // adding a legal detail for a J class legal action.  In this case LOPS
        // is mandatory.  The user cannot leave the screen without adding the
        // LOPS information and the user must return to LDET to continue.
        export.HiddenReturnRequired.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "HELPRTN":
        // ---------------------------------------------
        //                H E L P R T N
        // ---------------------------------------------
        // ----------------------------------------------------------
        // Upon return from the list of reference codes and if a
        // selection has been made from that list, the command
        // returned is HELPRTN
        // ----------------------------------------------------------
        break;
      case "PRINT":
        // mjr
        // --------------------------------------------------
        // 04/28/1999
        // Changed Print function to automatically determine which
        // document the user is Printing.  This makes the flow to DOCM
        // unnecessary.  At this time, I do not have the flow in my subset,
        // so this will need to be removed later.  Also, the RETDOCM
        // command has been removed.
        // If a CH is selected, the document is CHINFCCO and an AP must be
        // selected (and the AR must be the State of KS).
        // If an AP is selected, the document is APINSUCO.
        // If the AR is selected, the document is ARINSUCO.
        // ----------------------------------------------------------------
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (IsEmpty(export.ObligationPersons.Item.Common.SelectChar))
          {
            continue;
          }

          if (Equal(export.ObligationPersons.Item.CaseRole.Type1, "CH"))
          {
            local.SelectedCh.Number =
              export.ObligationPersons.Item.CsePersonsWorkSet.Number;
            local.SelectedCase.Number =
              export.ObligationPersons.Item.Case1.Number;

            // mjr
            // --------------------------------------------------
            // 04/28/1999
            // If a CH is selected, the document is CHINFCCO and an AP must be
            // selected (and the AR must be the State of KS).
            // ----------------------------------------------------------------
            local.Document.Name = "CHINFCCO";
          }
          else
          {
            local.SelectedCsePersonsWorkSet.Number =
              export.ObligationPersons.Item.CsePersonsWorkSet.Number;

            // mjr--->  Don't override the CH case number
            if (IsEmpty(local.SelectedCase.Number))
            {
              local.SelectedCase.Number =
                export.ObligationPersons.Item.Case1.Number;
            }

            if (Equal(export.ObligationPersons.Item.CaseRole.Type1, "AP"))
            {
              // mjr
              // --------------------------------------------------
              // 04/28/1999
              // If an AP is selected, the document is APINSUCO.
              // ----------------------------------------------------------------
              local.Document.Name = "APINSUCO";
            }
            else
            {
              // mjr
              // --------------------------------------------------
              // 04/28/1999
              // If the AR is selected, the document is ARINSUCO.
              // ----------------------------------------------------------------
              local.Document.Name = "ARINSUCO";
            }
          }
        }

        if (IsEmpty(local.Document.Name))
        {
          for(export.ObligationPersons.Index = 0; export
            .ObligationPersons.Index < export.ObligationPersons.Count; ++
            export.ObligationPersons.Index)
          {
            var field =
              GetField(export.ObligationPersons.Item.Common, "selectChar");

            field.Error = true;

            break;
          }

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.Standard.NextTransaction = "DKEY";
        local.NextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + local.Document.Name;
        local.NextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;

        // mjr---> "LDET:" should be replaced with
        // 	local sp_doc_literal id_legal_action_detail
        local.NextTranInfo.MiscText1 = "LDET:" + NumberToString
          (export.LegalActionDetail.Number, 14, 2);

        if (!IsEmpty(local.SelectedCase.Number))
        {
          local.NextTranInfo.CaseNumber = local.SelectedCase.Number;
        }

        if (!IsEmpty(local.SelectedCsePersonsWorkSet.Number))
        {
          local.Print.Text50 = TrimEnd(local.SpDocLiteral.IdPrNumber) + local
            .SelectedCsePersonsWorkSet.Number;
          local.NextTranInfo.MiscText1 =
            TrimEnd(local.NextTranInfo.MiscText1) + ";" + local.Print.Text50;
        }

        if (!IsEmpty(local.SelectedCh.Number))
        {
          local.Print.Text50 = TrimEnd(local.SpDocLiteral.IdChNumber) + local
            .SelectedCh.Number;
          local.NextTranInfo.MiscText1 =
            TrimEnd(local.NextTranInfo.MiscText1) + ";" + local.Print.Text50;
        }

        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "RETDOCM":
        break;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 01/04/1999
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
        local.Position.Count = Find(local.NextTranInfo.MiscText1, "LDET:");

        if (local.Position.Count > 0)
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(Substring(
              local.NextTranInfo.MiscText1, 50, local.Position.Count + 5, 2));
          export.LegalActionDetail.Number =
            (int)local.BatchConvertNumToText.Number15;
        }

        global.Command = "DISPLAY";

        break;
      case "HELP":
        // --------------------------------------------------------------
        // All logic pertaining to using the IEF help facility should be
        // placed here.
        // At this time, this is not available.
        // -------------------------------------------------------------
        break;
      case "NAME":
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        return;
      case "RETNAME":
        if (IsEmpty(import.Selected.Number))
        {
          // --- No selection has been made.
          break;
        }

        local.Dummy.Flag = "N";

        export.ObligationPersons.Index = export.ObligationPersons.Count;
        export.ObligationPersons.CheckIndex();

        do
        {
          if (export.ObligationPersons.IsFull)
          {
            break;
          }

          MoveCsePersonsWorkSet(import.Selected,
            export.ObligationPersons.Update.CsePersonsWorkSet);
          export.ObligationPersons.Update.Common.SelectChar = "S";

          var field1 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "accountType");

          field1.Protected = false;
          field1.Focused = true;

          export.ObligationPersons.Update.ListEndReason.PromptField = "";
          local.Dummy.Flag = "Y";

          if (!ReadCsePerson1())
          {
            ExitState = "CSE_PERSON_NF";

            var field6 =
              GetField(export.ObligationPersons.Item.CsePersonsWorkSet, "number");
              

            field6.Color = "red";
            field6.Protected = true;

            var field7 =
              GetField(export.ObligationPersons.Item.CsePersonsWorkSet,
              "formattedName");

            field7.Color = "red";
            field7.Protected = true;

            export.ObligationPersons.Next();

            return;
          }

          // *********************************************
          // RCG - 10/29/97 H00030861 - Return from Name
          // **************************
          var field2 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "effectiveDate");

          field2.Color = "green";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = false;

          var field3 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "currentAmount");

          field3.Color = "green";
          field3.Highlighting = Highlighting.Underscore;
          field3.Protected = false;

          var field4 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "arrearsAmount");

          field4.Color = "green";
          field4.Highlighting = Highlighting.Underscore;
          field4.Protected = false;

          var field5 =
            GetField(export.ObligationPersons.Item.LegalActionPerson,
            "judgementAmount");

          field5.Color = "green";
          field5.Highlighting = Highlighting.Underscore;
          field5.Protected = false;

          // 8/22/2000        GVandy		H00101595	Do not allow a non-case related 
          // person to be added as an obligor for "J" class legal actions.
          // Display informational message when returning from NAME indicating
          // that the person is not related to the case.
          ExitState = "LE0000_NON_CASE_RELATED_PERSON";
          export.ObligationPersons.Next();

          break;

          export.ObligationPersons.Next();
        }
        while(AsChar(local.Dummy.Flag) != 'Y');

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Action performed later below.
        // ---------------------------------------------
        break;
      case "FIRSTIME":
        global.Command = "DISPLAY";

        break;
      case "LISTPERS":
        local.CheckEmpty.Count = 0;

        for(export.CseCases.Index = 0; export.CseCases.Index < export
          .CseCases.Count; ++export.CseCases.Index)
        {
          if (!export.CseCases.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.CseCases.Item.Cse.Number))
          {
            ++local.CheckEmpty.Count;

            break;
          }
        }

        export.CseCases.CheckIndex();

        if (local.CheckEmpty.Count == 0)
        {
          UseLeCabRelatedCseCasesFLact();
        }

        UseLeLopsListLopsAndAllRoles();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST":
        // ---------------------------------------------
        // Count how many prompts were selected.
        // ---------------------------------------------
        local.TotalSelected.Count = 0;

        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (AsChar(export.ObligationPersons.Item.ListEndReason.PromptField) ==
            'S')
          {
            ++local.TotalSelected.Count;
          }
        }

        switch(local.TotalSelected.Count)
        {
          case 0:
            // ---------------------------------------------
            // A Prompt must be selected when PF4 List is
            // pressed.
            // ---------------------------------------------
            for(export.ObligationPersons.Index = 0; export
              .ObligationPersons.Index < export.ObligationPersons.Count; ++
              export.ObligationPersons.Index)
            {
              var field =
                GetField(export.ObligationPersons.Item.ListEndReason,
                "promptField");

              field.Error = true;
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            break;
          case 1:
            break;
          default:
            // ---------------------------------------------
            // Only one Prompt can be selected at one time.
            // ---------------------------------------------
            for(export.ObligationPersons.Index = 0; export
              .ObligationPersons.Index < export.ObligationPersons.Count; ++
              export.ObligationPersons.Index)
            {
              if (!IsEmpty(export.ObligationPersons.Item.ListEndReason.
                PromptField))
              {
                var field =
                  GetField(export.ObligationPersons.Item.ListEndReason,
                  "promptField");

                field.Error = true;
              }
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            return;
        }

        export.DisplayActiveCasesOnly.Flag = "Y";

        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (AsChar(export.ObligationPersons.Item.ListEndReason.PromptField) ==
            'S')
          {
            export.Code.CodeName = "LEGAL OBLIGATION END REASON";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
        }

        break;
      case "RETCDVL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // ---------------------------------------------
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (AsChar(export.ObligationPersons.Item.ListEndReason.PromptField) ==
            'S')
          {
            export.ObligationPersons.Update.ListEndReason.PromptField = "";

            if (!IsEmpty(import.DlgflwSelected.Cdvalue))
            {
              export.ObligationPersons.Update.LegalActionPerson.EndReason =
                import.DlgflwSelected.Cdvalue;
            }

            var field =
              GetField(export.ObligationPersons.Item.LegalActionPerson,
              "endReason");

            field.Protected = false;
            field.Focused = true;
          }
        }

        return;
      case "UPDATE":
        // ---------------------------------------------
        // Update the selected occurrence of the
        // Obligation Person.
        // ---------------------------------------------
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (IsEmpty(export.ObligationPersons.Item.Common.SelectChar))
          {
            continue;
          }

          if (Equal(export.ObligationType.Code, "718B") && AsChar
            (export.ObligationPersons.Item.LegalActionPerson.AccountType) == 'E'
            )
          {
            // ***************************************************************
            // If Obligation type = 718B, only obligee allowed is State of
            // Kansas.
            // ***************************************************************
            local.Code.CodeName = "STATE OBLIGEE PERSON NUMBER";
            local.CodeValue.Cdvalue =
              export.ObligationPersons.Item.CsePersonsWorkSet.Number;
            UseCabValidateCodeValue2();

            switch(local.ValidCode.Count)
            {
              case 1:
                ExitState = "CODE_NF";

                break;
              case 2:
                ExitState = "LE0000_KANSAS_MUST_BE_OBLIGEE";

                break;
              default:
                break;
            }
          }

          if (AsChar(export.LegalAction.Classification) == 'J' && IsEmpty
            (export.ObligationPersons.Item.CaseRole.Type1) && AsChar
            (export.ObligationPersons.Item.LegalActionPerson.AccountType) == 'R'
            )
          {
            // 8/22/2000        GVandy		H00101595	Do not allow a non-case 
            // related person to be added as an obligor for "J" class legal
            // actions.
            var field =
              GetField(export.ObligationPersons.Item.LegalActionPerson,
              "accountType");

            field.Error = true;

            ExitState = "LE0000_OBLIGOR_MUST_BE_ON_CASE";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          UseLeLopsUpdateLopsDetails();

          if (IsExitState("LEGAL_ACTION_NF"))
          {
          }
          else if (IsExitState("LEGAL_ACTION_DETAIL_NF"))
          {
          }
          else if (IsExitState("CO0000_LEGAL_ACTION_PERSON_AE"))
          {
            var field =
              GetField(export.ObligationPersons.Item.Common, "selectChar");

            field.Error = true;
          }
          else if (IsExitState("CSE_PERSON_AE"))
          {
            var field =
              GetField(export.ObligationPersons.Item.Common, "selectChar");

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
        }

        UseLeLopsValidateAddedLopsInfo1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("LE0000_MORE_THAN_TWO_OBLIGORS") || IsExitState
            ("LE0000_ONE_OBLIGOR_FOR_OBLG_TYPE"))
          {
            for(export.ObligationPersons.Index = 0; export
              .ObligationPersons.Index < export.ObligationPersons.Count; ++
              export.ObligationPersons.Index)
            {
              if (AsChar(export.ObligationPersons.Item.LegalActionPerson.
                AccountType) == 'R')
              {
                var field =
                  GetField(export.ObligationPersons.Item.LegalActionPerson,
                  "accountType");

                field.Error = true;
              }
            }
          }

          if (IsExitState("LE0000_MORE_THAN_ONE_OBLIGEE"))
          {
            for(export.ObligationPersons.Index = 0; export
              .ObligationPersons.Index < export.ObligationPersons.Count; ++
              export.ObligationPersons.Index)
            {
              if (AsChar(export.ObligationPersons.Item.LegalActionPerson.
                AccountType) == 'E')
              {
                var field =
                  GetField(export.ObligationPersons.Item.LegalActionPerson,
                  "accountType");

                field.Error = true;
              }
            }
          }

          UseEabRollbackCics();

          return;
        }

        UseLeLopsListOnlyRelvntCroles();

        if (AsChar(local.CommandWasAdd.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "RETURN":
        local.TotalSelected.Count = 0;

        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (!IsEmpty(export.ObligationPersons.Item.Common.SelectChar))
          {
            ++local.TotalSelected.Count;
          }
        }

        if (local.TotalSelected.Count > 1)
        {
          for(export.ObligationPersons.Index = 0; export
            .ObligationPersons.Index < export.ObligationPersons.Count; ++
            export.ObligationPersons.Index)
          {
            if (!IsEmpty(export.ObligationPersons.Item.Common.SelectChar))
            {
              var field =
                GetField(export.ObligationPersons.Item.Common, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "DELETE":
        // ---------------------------------------------
        // Delete the selected occurrence of the
        // Obligation Person and blank out the fields.
        // ---------------------------------------------
        for(export.ObligationPersons.Index = 0; export
          .ObligationPersons.Index < export.ObligationPersons.Count; ++
          export.ObligationPersons.Index)
        {
          if (IsEmpty(export.ObligationPersons.Item.Common.SelectChar))
          {
            continue;
          }

          // *** Fix for PR# H00082508 By Anand Katuri 1/3/2000
          // The following Read Each is introduced to delete a person (AP/CH) on
          // LOPS only if no Financial  Obligation is established. Also, the
          // replaced READ is commented out below.
          if (ReadObligation())
          {
            // *** Delete AR any time
            if (AsChar(export.ObligationPersons.Item.LegalActionPerson.
              AccountType) == 'E')
            {
              goto Read;
            }

            ExitState = "LE0000_DEBT_PREVENTS_DELETION";

            var field =
              GetField(export.ObligationPersons.Item.Common, "selectChar");

            field.Error = true;

            UseEabRollbackCics();

            return;
          }
          else
          {
            // *** Proceed to Delete
          }

Read:

          UseLeDeleteLegalRoleAndPerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field =
              GetField(export.ObligationPersons.Item.Common, "selectChar");

            field.Error = true;

            UseEabRollbackCics();

            return;
          }
        }

        UseLeLopsListOnlyRelvntCroles();
        UseLeLopsValidateAddedLopsInfo2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("LE0000_TOTAL_NOT_TALLY_LDET_OBGR"))
          {
            ExitState = "LE0000_CORRECT_OBLG_TOTAL";
          }

          if (IsExitState("LE0000_TOTALS_NO_TALLY_FOR_SUPP"))
          {
            ExitState = "LE0000_CORRECT_SUPP_PER_TOTAL";
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadLegalAction())
      {
        MoveLegalAction2(entities.LegalAction, export.LegalAction);
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }

      // ---------------------------------------------
      // Use the code_value table to obtain the
      // description for the legal_action action_taken
      // ---------------------------------------------
      UseLeGetActionTakenDescription();

      if (ReadLegalActionDetail())
      {
        export.LegalActionDetail.Assign(entities.LegalActionDetail);
      }
      else
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";

        return;
      }

      if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
      {
        export.ObligationType.Code =
          entities.LegalActionDetail.NonFinOblgType ?? Spaces(7);
      }
      else if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
      {
        if (ReadObligationType())
        {
          export.ObligationType.Code = entities.ObligationType.Code;
        }
      }
      else
      {
        ExitState = "LE0000_INVALID_LDET_TYPE";

        return;
      }

      if (ReadTribunal())
      {
        export.Tribunal.Assign(entities.Tribunal);
      }
      else
      {
        ExitState = "TRIBUNAL_NF";

        return;
      }

      if (ReadFips())
      {
        export.Fips.Assign(entities.Fips);
      }
      else if (ReadFipsTribAddress())
      {
        export.FipsTribAddress.Country = entities.Foreign.Country;
      }

      local.CheckEmpty.Count = 0;

      for(export.CseCases.Index = 0; export.CseCases.Index < export
        .CseCases.Count; ++export.CseCases.Index)
      {
        if (!export.CseCases.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.CseCases.Item.Cse.Number))
        {
          local.CheckEmpty.Count = (int)((long)local.CheckEmpty.Count + 1);

          break;
        }
      }

      export.CseCases.CheckIndex();

      if (local.CheckEmpty.Count == 0)
      {
        UseLeCabRelatedCseCasesFLact();
      }

      UseLeGetPetitionerRespondent();
      UseLeLopsListOnlyRelvntCroles();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // mjr
        // -----------------------------------------------
        // 01/05/1999
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
          // mjr---> Determines the appropriate exitstate for the Print process
          local.Print.Text50 = local.NextTranInfo.MiscText2 ?? Spaces(50);
          UseSpPrintDecodeReturnCode();
          local.NextTranInfo.MiscText2 = local.Print.Text50;
        }
      }

      // *** if no records are found go ahead and list all persons and return 
      // message of add
      if (IsExitState("LE0000_NO_RECORDS_PRESS_PF17"))
      {
        UseLeLopsListLopsAndAllRoles();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "LE0000_OBL_PERSON_ROLE_NF";
        }
      }

      if (export.CseCases.IsEmpty)
      {
        export.CseCases.Index = 0;
        export.CseCases.CheckSize();

        export.CseCases.Update.Cse.Number = "";
      }
    }
    else
    {
    }

    UseLeCabCheckOblgSetUpFLdet();

    // 09/30/02 GVandy WR020120 -- Finance archive changes and screen field 
    // protection logic simplification below.
    if (ReadObligationTypeLegalActionLegalActionDetail())
    {
      local.ObligationType.Classification =
        entities.ObligationType.Classification;
    }
    else
    {
      local.ObligationType.Classification = "";
    }

    for(export.ObligationPersons.Index = 0; export.ObligationPersons.Index < export
      .ObligationPersons.Count; ++export.ObligationPersons.Index)
    {
      // --- First unprotect all the enterable fields; then protect based on the
      // debt set up
      var field1 =
        GetField(export.ObligationPersons.Item.LegalActionPerson, "accountType");
        

      field1.Color = "green";
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;

      var field2 =
        GetField(export.ObligationPersons.Item.LegalActionPerson, "accountType");
        

      field2.Color = "green";
      field2.Highlighting = Highlighting.Underscore;
      field2.Protected = false;

      var field3 =
        GetField(export.ObligationPersons.Item.LegalActionPerson,
        "effectiveDate");

      field3.Color = "green";
      field3.Highlighting = Highlighting.Underscore;
      field3.Protected = false;

      var field4 =
        GetField(export.ObligationPersons.Item.LegalActionPerson,
        "currentAmount");

      field4.Color = "green";
      field4.Highlighting = Highlighting.Underscore;
      field4.Protected = false;

      var field5 =
        GetField(export.ObligationPersons.Item.LegalActionPerson,
        "arrearsAmount");

      field5.Color = "green";
      field5.Highlighting = Highlighting.Underscore;
      field5.Protected = false;

      var field6 =
        GetField(export.ObligationPersons.Item.LegalActionPerson,
        "judgementAmount");

      field6.Color = "green";
      field6.Highlighting = Highlighting.Underscore;
      field6.Protected = false;

      var field7 =
        GetField(export.ObligationPersons.Item.LegalActionPerson, "endDate");

      field7.Color = "green";
      field7.Highlighting = Highlighting.Underscore;
      field7.Protected = false;

      var field8 =
        GetField(export.ObligationPersons.Item.LegalActionPerson, "endReason");

      field8.Color = "green";
      field8.Highlighting = Highlighting.Underscore;
      field8.Protected = false;

      if (Equal(entities.LegalActionDetail.CreatedBy, "CONVERSN") && Equal
        (Date(entities.LegalActionDetail.LastUpdatedTstamp), Now().Date))
      {
        // Allow the user one opportunity to update conversion information.
        // In the update cab, the legal detail created_by will be changed to '
        // CONVERSX'.
        continue;
      }

      // *** Obligee amounts fields should be for reference only. Per Jan 
      // Brigham 2/3/98.
      if (AsChar(export.ObligationPersons.Item.LegalActionPerson.AccountType) ==
        'E')
      {
        var field9 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "currentAmount");

        field9.Color = "cyan";
        field9.Highlighting = Highlighting.Underscore;
        field9.Protected = true;

        var field10 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "arrearsAmount");

        field10.Color = "cyan";
        field10.Highlighting = Highlighting.Underscore;
        field10.Protected = true;

        var field11 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "judgementAmount");

        field11.Color = "cyan";
        field11.Highlighting = Highlighting.Underscore;
        field11.Protected = true;
      }

      // ***  Once an obligation is established, protect the account type, 
      // effective date, and current,
      // arrears, and judgement amounts.
      if (AsChar(local.OblgSetUpForLdet.Flag) == 'Y')
      {
        var field9 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "accountType");

        field9.Color = "cyan";
        field9.Highlighting = Highlighting.Underscore;
        field9.Protected = true;

        var field10 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "effectiveDate");

        field10.Color = "cyan";
        field10.Highlighting = Highlighting.Underscore;
        field10.Protected = true;

        var field11 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "currentAmount");

        field11.Color = "cyan";
        field11.Highlighting = Highlighting.Underscore;
        field11.Protected = true;

        var field12 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "arrearsAmount");

        field12.Color = "cyan";
        field12.Highlighting = Highlighting.Underscore;
        field12.Protected = true;

        var field13 =
          GetField(export.ObligationPersons.Item.LegalActionPerson,
          "judgementAmount");

        field13.Color = "cyan";
        field13.Highlighting = Highlighting.Underscore;
        field13.Protected = true;

        // ***  If the obligation is still active then protect the end date and 
        // reason.
        if (AsChar(local.OblgActiveForLdet.Flag) == 'Y')
        {
          // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
          // This was apparently changed at some point so that the end date and 
          // reason are NOT
          // protected.  This whole IF could be deleted but I am leaving it for 
          // future reference.
          // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }
      }
    }

    // -----  Make sure that one blank entry exists at the end so that the user 
    // can enter the next cse case number. Note that the 'unused entries are
    // protected' on the screen and this option is applied by IEF to both the
    // repeating groups.
    if (export.CseCases.IsEmpty)
    {
      export.CseCases.Index = 0;
      export.CseCases.CheckSize();

      export.CseCases.Update.Cse.Number = "";
    }
    else
    {
      export.CseCases.Index = export.CseCases.Count - 1;
      export.CseCases.CheckSize();

      if (IsEmpty(export.CseCases.Item.Cse.Number))
      {
        // --- A blank entry already exists.
      }
      else if (export.CseCases.Index + 1 < Export.CseCasesGroup.Capacity)
      {
        ++export.CseCases.Index;
        export.CseCases.CheckSize();

        export.CseCases.Update.Cse.Number = "";
      }
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveCseCases1(Export.CseCasesGroup source,
    LeLopsListLopsAndAllRoles.Import.CseCasesGroup target)
  {
    target.Cse.Number = source.Cse.Number;
  }

  private static void MoveCseCases2(Export.CseCasesGroup source,
    LeLopsListOnlyRelvntCroles.Import.CseCasesGroup target)
  {
    target.Detail.Number = source.Cse.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.ForeignFipsState = source.ForeignFipsState;
    target.ForeignFipsCounty = source.ForeignFipsCounty;
    target.ForeignFipsLocation = source.ForeignFipsLocation;
    target.ForeignOrderNumber = source.ForeignOrderNumber;
    target.Identifier = source.Identifier;
    target.LastModificationReviewDate = source.LastModificationReviewDate;
    target.AttorneyApproval = source.AttorneyApproval;
    target.ApprovalSentDate = source.ApprovalSentDate;
    target.PetitionerApproval = source.PetitionerApproval;
    target.ApprovalReceivedDate = source.ApprovalReceivedDate;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.EndDate = source.EndDate;
    target.ForeignOrderRegistrationDate = source.ForeignOrderRegistrationDate;
    target.UresaSentDate = source.UresaSentDate;
    target.UresaAcknowledgedDate = source.UresaAcknowledgedDate;
    target.UifsaSentDate = source.UifsaSentDate;
    target.UifsaAcknowledgedDate = source.UifsaAcknowledgedDate;
    target.InitiatingState = source.InitiatingState;
    target.InitiatingCounty = source.InitiatingCounty;
    target.RespondingState = source.RespondingState;
    target.RespondingCounty = source.RespondingCounty;
    target.OrderAuthority = source.OrderAuthority;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.LongArmStatuteIndicator = source.LongArmStatuteIndicator;
    target.PaymentLocation = source.PaymentLocation;
    target.EstablishmentCode = source.EstablishmentCode;
    target.DismissedWithoutPrejudiceInd = source.DismissedWithoutPrejudiceInd;
    target.DismissalCode = source.DismissalCode;
    target.RefileDate = source.RefileDate;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligationPersons1(LeLopsListLopsAndAllRoles.Export.
    ObligationPersonsGroup source, Export.ObligationPersonsGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.ListEndReason.PromptField = source.ListEndReason.PromptField;
    target.Common.SelectChar = source.Common.SelectChar;
    target.LegalActionPerson.Assign(source.LegalActionPerson);
    target.Case1.Number = source.Case1.Number;
    target.CaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet(source.CsePersonsWorkSet, target.CsePersonsWorkSet);
  }

  private static void MoveObligationPersons2(LeLopsListOnlyRelvntCroles.Export.
    ObligationPersonsGroup source, Export.ObligationPersonsGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.ListEndReason.PromptField = source.ListEndReason.PromptField;
    target.Common.SelectChar = source.Common.SelectChar;
    target.LegalActionPerson.Assign(source.LegalActionPerson);
    target.Case1.Number = source.Case1.Number;
    target.CaseRole.Assign(source.CaseRole);
    MoveCsePersonsWorkSet(source.CsePersonsWorkSet, target.CsePersonsWorkSet);
  }

  private static void MoveRelatedCseCases(LeCabRelatedCseCasesFLact.Export.
    RelatedCseCasesGroup source, Export.CseCasesGroup target)
  {
    target.Cse.Number = source.DetailRelated.Number;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdAdminAction = source.IdAdminAction;
    target.IdChNumber = source.IdChNumber;
    target.IdDocument = source.IdDocument;
    target.IdObligationAdminAction = source.IdObligationAdminAction;
    target.IdPrNumber = source.IdPrNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Count = useExport.ReturnCode.Count;
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

  private void UseLeCabCheckOblgSetUpFLdet()
  {
    var useImport = new LeCabCheckOblgSetUpFLdet.Import();
    var useExport = new LeCabCheckOblgSetUpFLdet.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(LeCabCheckOblgSetUpFLdet.Execute, useImport, useExport);

    local.OblgSetUpForLdet.Flag = useExport.OblgSetUpForLdet.Flag;
    local.OblgActiveForLdet.Flag = useExport.OblgActiveForLdet.Flag;
  }

  private void UseLeCabRelatedCseCasesFLact()
  {
    var useImport = new LeCabRelatedCseCasesFLact.Import();
    var useExport = new LeCabRelatedCseCasesFLact.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeCabRelatedCseCasesFLact.Execute, useImport, useExport);

    useExport.RelatedCseCases.CopyTo(export.CseCases, MoveRelatedCseCases);
  }

  private void UseLeDeleteLegalRoleAndPerson()
  {
    var useImport = new LeDeleteLegalRoleAndPerson.Import();
    var useExport = new LeDeleteLegalRoleAndPerson.Export();

    useImport.Zdel.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionPerson.Identifier =
      export.ObligationPersons.Item.LegalActionPerson.Identifier;
    useImport.Case1.Number = export.ObligationPersons.Item.Case1.Number;
    MoveCaseRole(export.ObligationPersons.Item.CaseRole, useImport.CaseRole);
    useImport.CsePersonsWorkSet.Number =
      export.ObligationPersons.Item.CsePersonsWorkSet.Number;

    Call(LeDeleteLegalRoleAndPerson.Execute, useImport, useExport);
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = export.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.CodeValue.Description;
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

  private void UseLeLopsListLopsAndAllRoles()
  {
    var useImport = new LeLopsListLopsAndAllRoles.Import();
    var useExport = new LeLopsListLopsAndAllRoles.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    export.CseCases.CopyTo(useImport.CseCases, MoveCseCases1);

    Call(LeLopsListLopsAndAllRoles.Execute, useImport, useExport);

    useExport.ObligationPersons.CopyTo(
      export.ObligationPersons, MoveObligationPersons1);
  }

  private void UseLeLopsListOnlyRelvntCroles()
  {
    var useImport = new LeLopsListOnlyRelvntCroles.Import();
    var useExport = new LeLopsListOnlyRelvntCroles.Export();

    export.CseCases.CopyTo(useImport.CseCases, MoveCseCases2);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(LeLopsListOnlyRelvntCroles.Execute, useImport, useExport);

    export.CountTotalSupported.Count = useExport.CountTotalSupported.Count;
    useExport.ObligationPersons.CopyTo(
      export.ObligationPersons, MoveObligationPersons2);
  }

  private void UseLeLopsUpdateLopsDetails()
  {
    var useImport = new LeLopsUpdateLopsDetails.Import();
    var useExport = new LeLopsUpdateLopsDetails.Export();

    MoveCaseRole(export.ObligationPersons.Item.CaseRole, useImport.CaseRole);
    useImport.Case1.Number = export.ObligationPersons.Item.Case1.Number;
    useImport.CsePersonsWorkSet.Number =
      export.ObligationPersons.Item.CsePersonsWorkSet.Number;
    MoveLegalAction1(export.LegalAction, useImport.LegalAction);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalActionPerson.Assign(
      export.ObligationPersons.Item.LegalActionPerson);

    Call(LeLopsUpdateLopsDetails.Execute, useImport, useExport);
  }

  private void UseLeLopsValidateAddedLopsInfo1()
  {
    var useImport = new LeLopsValidateAddedLopsInfo.Import();
    var useExport = new LeLopsValidateAddedLopsInfo.Export();

    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.ObligationType.Code = export.ObligationType.Code;

    Call(LeLopsValidateAddedLopsInfo.Execute, useImport, useExport);
  }

  private void UseLeLopsValidateAddedLopsInfo2()
  {
    var useImport = new LeLopsValidateAddedLopsInfo.Import();
    var useExport = new LeLopsValidateAddedLopsInfo.Export();

    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeLopsValidateAddedLopsInfo.Execute, useImport, useExport);
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

    useImport.CsePersonsWorkSet.Number = import.Selected.Number;
    useImport.LegalAction.Assign(import.LegalAction);
    useImport.Case1.Number = import.CseCases.Item.Cse.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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

    useImport.WorkArea.Text50 = local.Print.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.Print.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CseCases.Item.Cse.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
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
          export.ObligationPersons.Item.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Child.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          export.ObligationPersons.Item.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.Child.PaternityLockInd = db.GetNullableString(reader, 2);
        entities.Child.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
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

  private bool ReadFipsTribAddress()
  {
    entities.Foreign.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Foreign.Identifier = db.GetInt32(reader, 0);
        entities.Foreign.Country = db.GetNullableString(reader, 1);
        entities.Foreign.TrbId = db.GetNullableInt32(reader, 2);
        entities.Foreign.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedBy = db.GetString(reader, 5);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", export.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 4);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 7);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 9);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 10);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionDetailLegalAction()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", export.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 4);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 7);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 9);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 10);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 11);
        entities.LegalAction.Classification = db.GetString(reader, 12);
        entities.LegalAction.ActionTaken = db.GetString(reader, 13);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 14);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 15);
        entities.LegalAction.CreatedBy = db.GetString(reader, 16);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 17);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 18);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", export.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 4);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationTypeLegalActionLegalActionDetail()
  {
    entities.ObligationType.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadObligationTypeLegalActionLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", export.LegalActionDetail.Number);
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 4);
        entities.LegalAction.Classification = db.GetString(reader, 5);
        entities.LegalAction.ActionTaken = db.GetString(reader, 6);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedBy = db.GetString(reader, 9);
        entities.LegalAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 11);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 12);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 13);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 14);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 15);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 16);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 17);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 18);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 19);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 20);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 21);
        entities.ObligationType.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
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
    /// <summary>A CseCasesGroup group.</summary>
    [Serializable]
    public class CseCasesGroup
    {
      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Case1 Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 cse;
    }

    /// <summary>A ObligationPersonsGroup group.</summary>
    [Serializable]
    public class ObligationPersonsGroup
    {
      /// <summary>
      /// A value of ListEndReason.
      /// </summary>
      [JsonPropertyName("listEndReason")]
      public Standard ListEndReason
      {
        get => listEndReason ??= new();
        set => listEndReason = value;
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
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
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
      /// A value of CaseRole.
      /// </summary>
      [JsonPropertyName("caseRole")]
      public CaseRole CaseRole
      {
        get => caseRole ??= new();
        set => caseRole = value;
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
      /// A value of ZdelOupImportActiveSupported.
      /// </summary>
      [JsonPropertyName("zdelOupImportActiveSupported")]
      public Common ZdelOupImportActiveSupported
      {
        get => zdelOupImportActiveSupported ??= new();
        set => zdelOupImportActiveSupported = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Standard listEndReason;
      private Common common;
      private LegalActionPerson legalActionPerson;
      private Case1 case1;
      private CaseRole caseRole;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Common zdelOupImportActiveSupported;
    }

    /// <summary>
    /// A value of CountTotalSupported.
    /// </summary>
    [JsonPropertyName("countTotalSupported")]
    public Common CountTotalSupported
    {
      get => countTotalSupported ??= new();
      set => countTotalSupported = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// Gets a value of CseCases.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesGroup> CseCases => cseCases ??= new(
      CseCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CseCases for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases")]
    [Computed]
    public IList<CseCasesGroup> CseCases_Json
    {
      get => cseCases;
      set => CseCases.Assign(value);
    }

    /// <summary>
    /// A value of PromptCase.
    /// </summary>
    [JsonPropertyName("promptCase")]
    public Common PromptCase
    {
      get => promptCase ??= new();
      set => promptCase = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Classification.
    /// </summary>
    [JsonPropertyName("classification")]
    public WorkArea Classification
    {
      get => classification ??= new();
      set => classification = value;
    }

    /// <summary>
    /// Gets a value of ObligationPersons.
    /// </summary>
    [JsonIgnore]
    public Array<ObligationPersonsGroup> ObligationPersons =>
      obligationPersons ??= new(ObligationPersonsGroup.Capacity);

    /// <summary>
    /// Gets a value of ObligationPersons for json serialization.
    /// </summary>
    [JsonPropertyName("obligationPersons")]
    [Computed]
    public IList<ObligationPersonsGroup> ObligationPersons_Json
    {
      get => obligationPersons;
      set => ObligationPersons.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of HiddenReturnRequired.
    /// </summary>
    [JsonPropertyName("hiddenReturnRequired")]
    public Common HiddenReturnRequired
    {
      get => hiddenReturnRequired ??= new();
      set => hiddenReturnRequired = value;
    }

    private Common countTotalSupported;
    private CsePersonsWorkSet selected;
    private ObligationType obligationType;
    private CodeValue actionTaken;
    private Document document;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private Array<CseCasesGroup> cseCases;
    private Common promptCase;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private WorkArea classification;
    private Array<ObligationPersonsGroup> obligationPersons;
    private CodeValue dlgflwSelected;
    private Common hiddenReturnRequired;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CseCasesGroup group.</summary>
    [Serializable]
    public class CseCasesGroup
    {
      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Case1 Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 cse;
    }

    /// <summary>A ObligationPersonsGroup group.</summary>
    [Serializable]
    public class ObligationPersonsGroup
    {
      /// <summary>
      /// A value of ListEndReason.
      /// </summary>
      [JsonPropertyName("listEndReason")]
      public Standard ListEndReason
      {
        get => listEndReason ??= new();
        set => listEndReason = value;
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
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
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
      /// A value of CaseRole.
      /// </summary>
      [JsonPropertyName("caseRole")]
      public CaseRole CaseRole
      {
        get => caseRole ??= new();
        set => caseRole = value;
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
      /// A value of ZdelOupExportActiveSupported.
      /// </summary>
      [JsonPropertyName("zdelOupExportActiveSupported")]
      public Common ZdelOupExportActiveSupported
      {
        get => zdelOupExportActiveSupported ??= new();
        set => zdelOupExportActiveSupported = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Standard listEndReason;
      private Common common;
      private LegalActionPerson legalActionPerson;
      private Case1 case1;
      private CaseRole caseRole;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Common zdelOupExportActiveSupported;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Case1 Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

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
    /// A value of CountTotalSupported.
    /// </summary>
    [JsonPropertyName("countTotalSupported")]
    public Common CountTotalSupported
    {
      get => countTotalSupported ??= new();
      set => countTotalSupported = value;
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
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Document Filter
    {
      get => filter ??= new();
      set => filter = value;
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
    /// Gets a value of CseCases.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesGroup> CseCases => cseCases ??= new(
      CseCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CseCases for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases")]
    [Computed]
    public IList<CseCasesGroup> CseCases_Json
    {
      get => cseCases;
      set => CseCases.Assign(value);
    }

    /// <summary>
    /// A value of PromptCase.
    /// </summary>
    [JsonPropertyName("promptCase")]
    public Common PromptCase
    {
      get => promptCase ??= new();
      set => promptCase = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Classification.
    /// </summary>
    [JsonPropertyName("classification")]
    public WorkArea Classification
    {
      get => classification ??= new();
      set => classification = value;
    }

    /// <summary>
    /// Gets a value of ObligationPersons.
    /// </summary>
    [JsonIgnore]
    public Array<ObligationPersonsGroup> ObligationPersons =>
      obligationPersons ??= new(ObligationPersonsGroup.Capacity);

    /// <summary>
    /// Gets a value of ObligationPersons for json serialization.
    /// </summary>
    [JsonPropertyName("obligationPersons")]
    [Computed]
    public IList<ObligationPersonsGroup> ObligationPersons_Json
    {
      get => obligationPersons;
      set => ObligationPersons.Assign(value);
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
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
    /// A value of HiddenReturnRequired.
    /// </summary>
    [JsonPropertyName("hiddenReturnRequired")]
    public Common HiddenReturnRequired
    {
      get => hiddenReturnRequired ??= new();
      set => hiddenReturnRequired = value;
    }

    private Case1 selected;
    private Common docmProtectFilter;
    private Common countTotalSupported;
    private ObligationType obligationType;
    private CodeValue actionTaken;
    private Document filter;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private Array<CseCasesGroup> cseCases;
    private Common promptCase;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private WorkArea classification;
    private Array<ObligationPersonsGroup> obligationPersons;
    private Common displayActiveCasesOnly;
    private Code code;
    private Common hiddenReturnRequired;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OblgActiveForLdet.
    /// </summary>
    [JsonPropertyName("oblgActiveForLdet")]
    public Common OblgActiveForLdet
    {
      get => oblgActiveForLdet ??= new();
      set => oblgActiveForLdet = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Ogligee.
    /// </summary>
    [JsonPropertyName("ogligee")]
    public Common Ogligee
    {
      get => ogligee ??= new();
      set => ogligee = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public WorkArea Print
    {
      get => print ??= new();
      set => print = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedCh.
    /// </summary>
    [JsonPropertyName("selectedCh")]
    public CsePersonsWorkSet SelectedCh
    {
      get => selectedCh ??= new();
      set => selectedCh = value;
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
    /// A value of OnacDebtDetailRetired.
    /// </summary>
    [JsonPropertyName("onacDebtDetailRetired")]
    public Common OnacDebtDetailRetired
    {
      get => onacDebtDetailRetired ??= new();
      set => onacDebtDetailRetired = value;
    }

    /// <summary>
    /// A value of AccrualInstDiscontinued.
    /// </summary>
    [JsonPropertyName("accrualInstDiscontinued")]
    public Common AccrualInstDiscontinued
    {
      get => accrualInstDiscontinued ??= new();
      set => accrualInstDiscontinued = value;
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
    /// A value of NetCurrent.
    /// </summary>
    [JsonPropertyName("netCurrent")]
    public Common NetCurrent
    {
      get => netCurrent ??= new();
      set => netCurrent = value;
    }

    /// <summary>
    /// A value of NetJudgement.
    /// </summary>
    [JsonPropertyName("netJudgement")]
    public Common NetJudgement
    {
      get => netJudgement ??= new();
      set => netJudgement = value;
    }

    /// <summary>
    /// A value of OblgSetUpForLdet.
    /// </summary>
    [JsonPropertyName("oblgSetUpForLdet")]
    public Common OblgSetUpForLdet
    {
      get => oblgSetUpForLdet ??= new();
      set => oblgSetUpForLdet = value;
    }

    /// <summary>
    /// A value of ZdelLocalCount.
    /// </summary>
    [JsonPropertyName("zdelLocalCount")]
    public Common ZdelLocalCount
    {
      get => zdelLocalCount ??= new();
      set => zdelLocalCount = value;
    }

    /// <summary>
    /// A value of PassTo.
    /// </summary>
    [JsonPropertyName("passTo")]
    public Case1 PassTo
    {
      get => passTo ??= new();
      set => passTo = value;
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
    /// A value of AllowLopsUpdate.
    /// </summary>
    [JsonPropertyName("allowLopsUpdate")]
    public Common AllowLopsUpdate
    {
      get => allowLopsUpdate ??= new();
      set => allowLopsUpdate = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of SelectedApOrArCount.
    /// </summary>
    [JsonPropertyName("selectedApOrArCount")]
    public Common SelectedApOrArCount
    {
      get => selectedApOrArCount ??= new();
      set => selectedApOrArCount = value;
    }

    /// <summary>
    /// A value of SelectedApCount.
    /// </summary>
    [JsonPropertyName("selectedApCount")]
    public Common SelectedApCount
    {
      get => selectedApCount ??= new();
      set => selectedApCount = value;
    }

    /// <summary>
    /// A value of SelectedChCount.
    /// </summary>
    [JsonPropertyName("selectedChCount")]
    public Common SelectedChCount
    {
      get => selectedChCount ??= new();
      set => selectedChCount = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of CommandWasAdd.
    /// </summary>
    [JsonPropertyName("commandWasAdd")]
    public Common CommandWasAdd
    {
      get => commandWasAdd ??= new();
      set => commandWasAdd = value;
    }

    /// <summary>
    /// A value of CheckEmpty.
    /// </summary>
    [JsonPropertyName("checkEmpty")]
    public Common CheckEmpty
    {
      get => checkEmpty ??= new();
      set => checkEmpty = value;
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
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public LegalActionPerson Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
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

    private Common oblgActiveForLdet;
    private Case1 selectedCase;
    private Document document;
    private Common ogligee;
    private BatchConvertNumToText batchConvertNumToText;
    private WorkArea print;
    private Common position;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private CsePersonsWorkSet selectedCh;
    private SpDocLiteral spDocLiteral;
    private Common onacDebtDetailRetired;
    private Common accrualInstDiscontinued;
    private ObligationType obligationType;
    private Common netCurrent;
    private Common netJudgement;
    private Common oblgSetUpForLdet;
    private Common zdelLocalCount;
    private Case1 passTo;
    private DateWorkArea current;
    private Common allowLopsUpdate;
    private Common dummy;
    private Common selectedApOrArCount;
    private Common selectedApCount;
    private Common selectedChCount;
    private DateWorkArea maxDate;
    private DateWorkArea dateWorkArea;
    private Common commandWasAdd;
    private Common checkEmpty;
    private TextWorkArea textWorkArea;
    private LegalActionPerson initialize;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common totalSelected;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private CsePerson child;
    private Obligation obligation;
    private Code code;
    private CodeValue codeValue;
    private CsePerson csePerson;
    private ObligationTransaction obligationTransaction;
    private FipsTribAddress foreign;
    private Fips fips;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Case1 case1;
    private LegalActionPerson legalActionPerson;
  }
#endregion
}

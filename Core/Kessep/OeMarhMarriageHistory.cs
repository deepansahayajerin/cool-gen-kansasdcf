// Program: OE_MARH_MARRIAGE_HISTORY, ID: 371884481, model: 746.
// Short name: SWEMARHP
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
/// A program: OE_MARH_MARRIAGE_HISTORY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure step facilitates maintenance of Marriage history details for 
/// a CSE Person.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeMarhMarriageHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MARH_MARRIAGE_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMarhMarriageHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMarhMarriageHistory.
  /// </summary>
  public OeMarhMarriageHistory(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This procedure step facilitates maintenance of Marriage History details.
    // PROCESSING:
    // To create a new set of marriage details the user will enter the CSE 
    // Person and the spouse (either another CSE Person or a Contact) details,
    // the marriage details and press CREATE key. The system will validate the
    // details and update the database.
    // To update a Marriage history, the user will first display the required 
    // marriage history record, will then make the necessary modifications and
    // press UPDATE key.
    // To delete a Marriage history, the user will first display the required 
    // marriage history record, will then press DELETE key.
    // The details of a marriage can be displayed in one of the two ways.
    // 1. The user can enter the CSE Person number alongwith a date optionally. 
    // The system will display the marriage details applicable as of that date.
    // If no date is supplied, Current Date is assumed.
    // 2. The user can link to LIST MARRIAGE HISTORY screen, select a marriage 
    // history record and return back to the maintenance screen.
    // ACTION BLOCKS:
    // The following action blocks are called from this procedure step:
    // 	OI_DISPLAY_MARRIAGE_HISTORY
    // 	OI_CREATE_MARRIAGE_HISTORY
    // 	OI_UPDATE_MARRIAGE_HISTORY
    // 	OI_DELETE_MARRIAGE_HISTORY
    // 	OI_VALIDATE_MARRIAGE_HISTORY
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R U -
    // 	CONTACT			- R U -
    // 	MARRIAGE_HISTORY	C R U D
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	01/04/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	01/04/95		Initial coding
    // Lofton	02/12/96		Retrofit changes.
    // Welborn 06/26/96		Add call to Left Pad EAB.
    // Marchman 11/8/96		Add new security and next tran.
    // JFCaillouet  06/03/99           Added PF9 Key
    // *********************************************
    // 	
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    export.Case1.Number = import.Case1.Number;
    export.PrimeCsePerson.Number = import.PrimeCsePerson.Number;
    export.PrimeCsePersonsWorkSet.FormattedName =
      import.PrimeCsePersonsWorkSet.FormattedName;

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.ListPrimeCsePersons.PromptField =
      import.ListPrimeCsePersons.PromptField;
    export.ListSpouseCsePersons.PromptField =
      import.ListSpouseCsePersons.PromptField;
    export.ListMarriageStateCodes.PromptField =
      import.ListMarriageStateCodes.PromptField;
    export.ListMarriageCountry.PromptField =
      import.ListMarriageCountry.PromptField;
    export.ListDivorceStateCodes.PromptField =
      import.ListDivorceStateCodes.PromptField;
    export.ListDivorceCountry.PromptField =
      import.ListDivorceCountry.PromptField;
    export.SpouseCsePerson.Number = import.SpouseCsePerson.Number;
    export.SpouseCsePersonsWorkSet.FormattedName =
      import.SpouseCsePersonsWorkSet.FormattedName;
    export.SpouseContact.Assign(import.SpouseContact);
    export.MarriageHistory.Assign(import.MarriageHistory);
    MoveCodeValue(import.DispMarriageCountry, export.DispMarriageCountry);
    MoveCodeValue(import.DispDivorceCountry, export.DispDivorceCountry);
    export.HiddenSelectedPrime.Number = import.HiddenSelectedPrime.Number;
    export.HiddenSelectedSpouse.Number = import.HiddenSelectdSpouse.Number;
    export.HiddenSelectedContact.Assign(import.HiddenSelectedContact);
    export.HiddenSelectedMarriageHistory.Assign(
      import.HiddenSelectedMarriageHistory);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    MoveMarriageHistory(import.UpdateTimestamp, export.UpdateTimestamp);
    local.DispSpecificMhistRec.Flag = "N";

    if (!IsEmpty(import.Case1.Number))
    {
      local.TextWorkArea.Text10 = import.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(import.PrimeCsePerson.Number))
    {
      local.TextWorkArea.Text10 = import.PrimeCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.PrimeCsePerson.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(import.SpouseCsePerson.Number))
    {
      local.TextWorkArea.Text10 = import.SpouseCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.SpouseCsePerson.Number = local.TextWorkArea.Text10;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.PrimeCsePerson.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.PrimeCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    if (Equal(global.Command, "RSMHIST"))
    {
      if (IsEmpty(import.HiddenSelectdSpouse.Number) && import
        .HiddenSelectedContact.ContactNumber == 0)
      {
        // ---------------------------------------------
        // No marriage history record selected.
        // ---------------------------------------------
        export.HiddenDisplayPerformed.Flag = "N";

        return;
      }

      export.PrimeCsePerson.Number = import.HiddenSelectedPrime.Number;
      export.SpouseCsePerson.Number = import.HiddenSelectdSpouse.Number;
      export.SpouseContact.Assign(import.HiddenSelectedContact);
      export.MarriageHistory.Assign(import.HiddenSelectedMarriageHistory);
      export.HiddenSelectedPrime.Number = import.HiddenSelectedPrime.Number;
      export.HiddenSelectedSpouse.Number = import.HiddenSelectdSpouse.Number;
      export.HiddenSelectedContact.Assign(import.HiddenSelectedContact);
      export.HiddenSelectedMarriageHistory.Assign(
        import.HiddenSelectedMarriageHistory);

      // ---------------------------------------------
      // Set the command to DISPLAY so that user can do an update/delete on the 
      // selected record.
      // ---------------------------------------------
      local.DispSpecificMhistRec.Flag = "Y";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (AsChar(export.ListPrimeCsePersons.PromptField) == 'S')
      {
        if (!IsEmpty(import.Selected.Number))
        {
          export.PrimeCsePerson.Number = import.Selected.Number;
          export.PrimeCsePersonsWorkSet.FormattedName =
            import.Selected.FormattedName;
        }

        export.ListPrimeCsePersons.PromptField = "";

        // ---------------------------------------------
        // Set command to display so that the details are immediately displayed.
        // ---------------------------------------------
        if (!IsEmpty(export.PrimeCsePerson.Number))
        {
          global.Command = "DISPLAY";
        }
      }
      else if (AsChar(export.ListSpouseCsePersons.PromptField) == 'S')
      {
        if (!IsEmpty(import.Selected.Number))
        {
          export.SpouseCsePerson.Number = import.Selected.Number;
          export.SpouseCsePersonsWorkSet.FormattedName =
            import.Selected.FormattedName;
        }

        export.ListSpouseCsePersons.PromptField = "";
      }

      if (AsChar(export.ListSpouseCsePersons.PromptField) == 'S')
      {
        var field = GetField(export.ListSpouseCsePersons, "promptField");

        field.Error = true;
      }

      if (AsChar(export.ListMarriageStateCodes.PromptField) == 'S')
      {
        var field = GetField(export.ListMarriageStateCodes, "promptField");

        field.Error = true;
      }

      if (AsChar(export.ListMarriageCountry.PromptField) == 'S')
      {
        var field = GetField(export.ListMarriageCountry, "promptField");

        field.Error = true;
      }

      if (AsChar(export.ListDivorceStateCodes.PromptField) == 'S')
      {
        var field = GetField(export.ListDivorceStateCodes, "promptField");

        field.Error = true;
      }

      if (AsChar(export.ListDivorceCountry.PromptField) == 'S')
      {
        var field = GetField(export.ListDivorceCountry, "promptField");

        field.Error = true;
      }
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "MARL") || Equal
      (global.Command, "RETCOMP") || Equal(global.Command, "RETNAME") || Equal
      (global.Command, "RETCDVL") || Equal(global.Command, "RTSELCNT") || Equal
      (global.Command, "RMCONT"))
    {
    }
    else
    {
      // **** begin group C ****
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // **** end   group C ****
    }

    switch(TrimEnd(global.Command))
    {
      case "RETCOMP":
        break;
      case "RETNAME":
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        if (!IsEmpty(export.ListPrimeCsePersons.PromptField) && AsChar
          (export.ListPrimeCsePersons.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListPrimeCsePersons, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListSpouseCsePersons.PromptField) && AsChar
          (export.ListSpouseCsePersons.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListSpouseCsePersons, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListMarriageStateCodes.PromptField) && AsChar
          (export.ListMarriageStateCodes.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListMarriageStateCodes, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListMarriageCountry.PromptField) && AsChar
          (export.ListMarriageCountry.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListMarriageCountry, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListDivorceStateCodes.PromptField) && AsChar
          (export.ListDivorceStateCodes.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListDivorceStateCodes, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListDivorceCountry.PromptField) && AsChar
          (export.ListDivorceCountry.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListDivorceCountry, "promptField");

          field.Error = true;
        }

        if (IsEmpty(export.ListDivorceCountry.PromptField) && IsEmpty
          (export.ListDivorceStateCodes.PromptField) && IsEmpty
          (export.ListMarriageCountry.PromptField) && IsEmpty
          (export.ListMarriageStateCodes.PromptField) && IsEmpty
          (export.ListPrimeCsePersons.PromptField) && IsEmpty
          (export.ListSpouseCsePersons.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.ListPrimeCsePersons, "promptField");

          field1.Error = true;

          var field2 = GetField(export.ListSpouseCsePersons, "promptField");

          field2.Error = true;

          var field3 = GetField(export.ListMarriageStateCodes, "promptField");

          field3.Error = true;

          var field4 = GetField(export.ListMarriageCountry, "promptField");

          field4.Error = true;

          var field5 = GetField(export.ListDivorceStateCodes, "promptField");

          field5.Error = true;

          var field6 = GetField(export.ListDivorceCountry, "promptField");

          field6.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(export.ListPrimeCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          break;
        }

        if (AsChar(export.ListSpouseCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          break;
        }

        if (AsChar(export.ListMarriageStateCodes.PromptField) == 'S')
        {
          export.SelectedForList.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.ListMarriageCountry.PromptField) == 'S')
        {
          export.SelectedForList.CodeName = "COUNTRY CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.ListDivorceStateCodes.PromptField) == 'S')
        {
          export.SelectedForList.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.ListDivorceCountry.PromptField) == 'S')
        {
          export.SelectedForList.CodeName = "COUNTRY CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETCDVL":
        global.Command = "DISPLAY";

        if (AsChar(export.ListMarriageStateCodes.PromptField) == 'S')
        {
          export.ListMarriageStateCodes.PromptField = "";

          if (IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            var field =
              GetField(export.MarriageHistory, "marriageCertificateState");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.MarriageHistory.MarriageCertificateState =
              import.DlgflwSelected.Cdvalue;

            var field = GetField(export.MarriageHistory, "marriageCountry");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.ListMarriageCountry.PromptField) == 'S')
        {
          export.ListMarriageCountry.PromptField = "";

          if (IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.MarriageHistory, "marriageCountry");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.MarriageHistory.MarriageCountry =
              import.DlgflwSelected.Cdvalue;
            export.DispMarriageCountry.Description =
              import.DlgflwSelected.Description;

            var field = GetField(export.MarriageHistory, "separationDate");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.ListDivorceStateCodes.PromptField) == 'S')
        {
          export.ListDivorceStateCodes.PromptField = "";

          if (IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.MarriageHistory, "divorceState");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.MarriageHistory.DivorceState = import.DlgflwSelected.Cdvalue;

            var field = GetField(export.MarriageHistory, "divorceCountry");

            field.Protected = false;
            field.Focused = true;
          }
        }
        else if (AsChar(export.ListDivorceCountry.PromptField) == 'S')
        {
          export.ListDivorceCountry.PromptField = "";

          if (IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.MarriageHistory, "divorceCountry");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.MarriageHistory.DivorceCountry =
              import.DlgflwSelected.Cdvalue;
            export.DispDivorceCountry.Description =
              import.DlgflwSelected.Description;

            var field = GetField(export.PrimeCsePerson, "number");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.ListMarriageStateCodes.PromptField) == 'S')
        {
          var field = GetField(export.ListMarriageStateCodes, "promptField");

          field.Error = true;
        }

        if (AsChar(export.ListMarriageCountry.PromptField) == 'S')
        {
          var field = GetField(export.ListMarriageCountry, "promptField");

          field.Error = true;
        }

        if (AsChar(export.ListDivorceStateCodes.PromptField) == 'S')
        {
          var field = GetField(export.ListDivorceStateCodes, "promptField");

          field.Error = true;
        }

        if (AsChar(export.ListDivorceCountry.PromptField) == 'S')
        {
          var field = GetField(export.ListDivorceCountry, "promptField");

          field.Error = true;
        }

        break;
      case "PCOL":
        ExitState = "ECO_LNK_TO_LIST_CONTACT";

        return;
      case "RTSELCNT":
        // ---------------------------------------------
        // Returned from Person Contact List - PCOL
        // ---------------------------------------------
        if (import.HiddenSelectedContact.ContactNumber == 0)
        {
          break;
        }

        export.PrimeCsePerson.Number = import.HiddenSelectedPrime.Number;
        export.SpouseContact.Assign(import.HiddenSelectedContact);
        ExitState = "ACO_NN0000_ALL_OK";

        break;
      case "PCON":
        ExitState = "ECO_LNK_TO_MAINTAIN_CONTACT";

        return;
      case "RMCONT":
        // ---------------------------------------------
        // Returned from Maintain Person Contact - PCON
        // ---------------------------------------------
        if (import.HiddenSelectedContact.ContactNumber == 0)
        {
          break;
        }

        export.SpouseContact.Assign(import.HiddenSelectedContact);
        ExitState = "ACO_NN0000_ALL_OK";

        break;
      case "MARL":
        export.LinkLmhistSelctedPrime.Number = export.PrimeCsePerson.Number;
        ExitState = "ECO_LNK_TO_LIST_MARRIAGE_HISTORY";

        return;
      case "RSMHIST":
        break;
      case "DISPLAY":
        if (!IsEmpty(export.PrimeCsePerson.Number) && !
          IsEmpty(export.Case1.Number))
        {
          UseOeCabCheckCaseMember();

          switch(AsChar(local.Work.Flag))
          {
            case 'C':
              var field1 = GetField(export.Case1, "number");

              field1.Error = true;

              ExitState = "CASE_NF";

              break;
            case 'P':
              var field2 = GetField(export.PrimeCsePerson, "number");

              field2.Error = true;

              export.PrimeCsePersonsWorkSet.FormattedName = "";
              ExitState = "CSE_PERSON_NF";

              break;
            case 'R':
              var field3 = GetField(export.Case1, "number");

              field3.Error = true;

              var field4 = GetField(export.PrimeCsePerson, "number");

              field4.Error = true;

              ExitState = "OE0000_CASE_MEMBER_NE";

              break;
            default:
              break;
          }

          if (!IsEmpty(local.Work.Flag))
          {
            return;
          }
        }

        if (!IsEmpty(export.PrimeCsePerson.Number))
        {
          UseOeMarhDisplayMarriageHistory();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ---------------------------------------------
            // Save these in hidden values so that the user can perform update/
            // delete on this record.
            export.HiddenDisplayPerformed.Flag = "Y";
            export.HiddenSelectedPrime.Number = export.PrimeCsePerson.Number;
            export.HiddenSelectedSpouse.Number = export.SpouseCsePerson.Number;
            export.HiddenSelectedContact.Assign(export.SpouseContact);
            export.HiddenSelectedMarriageHistory.Assign(export.MarriageHistory);
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else
          {
            if (IsExitState("CSE_PERSON_NF"))
            {
              var field = GetField(export.PrimeCsePerson, "number");

              field.Error = true;
            }

            export.HiddenDisplayPerformed.Flag = "N";
          }
        }
        else
        {
          ExitState = "CSE_PERSON_NO_REQUIRED";

          var field = GetField(export.PrimeCsePerson, "number");

          field.Error = true;
        }

        break;
      case "CREATE":
        export.HiddenDisplayPerformed.Flag = "N";
        local.UserAction.Command = global.Command;
        UseOeMarhValidatMarriageHistory2();

        if (local.LastErrorEntryNo.Count > 0)
        {
          // One or more validation error occurred. Setting exit state messages 
          // are done below.
          UseEabRollbackCics();

          break;
        }

        UseOeMarhCreateMarriageHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        export.HiddenSelectedMarriageHistory.Assign(export.MarriageHistory);
        export.HiddenSelectedPrime.Number = export.PrimeCsePerson.Number;
        export.HiddenSelectedSpouse.Number = export.SpouseCsePerson.Number;
        export.HiddenSelectedContact.Assign(export.SpouseContact);
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Make sure that:
        //   - previous action was DISPLAY and
        //   - CSE Person No has not been changed.
        // ---------------------------------------------
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE") || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y' || export
          .HiddenSelectedMarriageHistory.Identifier != export
          .MarriageHistory.Identifier || !
          Equal(export.HiddenSelectedPrime.Number, export.PrimeCsePerson.Number) ||
          export.HiddenSelectedContact.ContactNumber != export
          .SpouseContact.ContactNumber)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        local.UserAction.Command = global.Command;
        UseOeMarhValidatMarriageHistory1();

        if (local.LastErrorEntryNo.Count > 0)
        {
          // One or more validation error occurred. Setting exit state messages 
          // are done below.
          UseEabRollbackCics();

          break;
        }

        UseOeMarhUpdateMarriageHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        export.HiddenSelectedMarriageHistory.Assign(export.MarriageHistory);
        export.HiddenSelectedPrime.Number = export.PrimeCsePerson.Number;
        export.HiddenSelectedSpouse.Number = export.SpouseCsePerson.Number;
        export.HiddenSelectedContact.Assign(export.SpouseContact);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        // ---------------------------------------------
        // Make sure that:
        //   - previous action was DISPLAY and
        //   - CSE Person No has not been changed.
        // ---------------------------------------------
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE") || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y' || export
          .HiddenSelectedMarriageHistory.Identifier != export
          .MarriageHistory.Identifier || !
          Equal(export.HiddenSelectedPrime.Number, export.PrimeCsePerson.Number) ||
          export.HiddenSelectedContact.ContactNumber != export
          .SpouseContact.ContactNumber)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        // ---------------------------------------------
        // A valid marriage_history has been selected for delete.
        // ---------------------------------------------
        local.UserAction.Command = global.Command;
        UseOeMarhValidatMarriageHistory3();

        if (local.LastErrorEntryNo.Count > 0)
        {
          // One or more validation error occurred. Setting exit state messages 
          // are done below.
          UseEabRollbackCics();

          break;
        }

        export.HiddenDisplayPerformed.Flag = "N";
        UseOeMarhDeleteMarriageHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        // ---------------------------------------------
        // Clear the hidden views representing original record displayed for 
        // update/delete.
        // ---------------------------------------------
        export.SpouseCsePerson.Number = local.BlankCsePerson.Number;
        export.HiddenSelectedSpouse.Number = local.BlankCsePerson.Number;
        export.SpouseCsePersonsWorkSet.FormattedName = "";
        export.SpouseContact.Assign(local.BlankContact);
        export.HiddenSelectedContact.Assign(local.BlankContact);
        export.MarriageHistory.Assign(local.BlankMarriageHistory);
        export.HiddenSelectedMarriageHistory.Assign(local.BlankMarriageHistory);
        MoveMarriageHistory(local.BlankMarriageHistory, export.UpdateTimestamp);
        MoveCodeValue(local.CodeValue, export.DispDivorceCountry);
        MoveCodeValue(local.CodeValue, export.DispMarriageCountry);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

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

    export.HiddenPrevUserAction.Command = global.Command;

    local.ErrorCodes.Index = local.LastErrorEntryNo.Count - 1;
    local.ErrorCodes.CheckSize();

    while(local.ErrorCodes.Index >= 0)
    {
      switch(local.ErrorCodes.Item.EntryErrorCode.Count)
      {
        case 1:
          if (IsEmpty(export.PrimeCsePerson.Number))
          {
            ExitState = "CSE_PERSON_NO_REQUIRED";
          }
          else
          {
            ExitState = "CSE_PERSON_NF";
          }

          var field1 = GetField(export.PrimeCsePerson, "number");

          field1.Error = true;

          break;
        case 2:
          ExitState = "OE0070_SPOUSE_DETAILS_NOT_SUPP";

          var field2 = GetField(export.SpouseContact, "nameFirst");

          field2.Error = true;

          var field3 = GetField(export.SpouseContact, "nameLast");

          field3.Error = true;

          var field4 = GetField(export.SpouseCsePerson, "number");

          field4.Error = true;

          break;
        case 3:
          var field5 = GetField(export.SpouseContact, "nameFirst");

          field5.Error = true;

          var field6 = GetField(export.SpouseContact, "nameLast");

          field6.Error = true;

          var field7 = GetField(export.SpouseCsePerson, "number");

          field7.Error = true;

          ExitState = "OE0011_BOTH_CSE_PRS_N_CNT_SUPP";

          break;
        case 4:
          ExitState = "OE0194_INVALID_SPOUSE_ENTERED";

          var field8 = GetField(export.SpouseContact, "nameLast");

          field8.Error = true;

          var field9 = GetField(export.SpouseContact, "nameFirst");

          field9.Error = true;

          var field10 = GetField(export.SpouseContact, "middleInitial");

          field10.Error = true;

          break;
        case 5:
          ExitState = "OE0025_INVALID_CONTACT_NO";

          var field11 = GetField(export.SpouseContact, "contactNumber");

          field11.Error = true;

          break;
        case 6:
          ExitState = "OE0071_NOT_RETRVD_CURR_MARR_H";

          var field12 = GetField(export.SpouseCsePerson, "number");

          field12.Error = true;

          break;
        case 7:
          ExitState = "OE0069_SPOUS_BOTH_CSE_P_N_CONT";

          var field13 = GetField(export.SpouseCsePerson, "number");

          field13.Error = true;

          break;
        case 8:
          ExitState = "OE0061_NF_MARRIAGE_HISTORY";

          var field14 = GetField(export.SpouseCsePerson, "number");

          field14.Error = true;

          break;
        case 9:
          ExitState = "OE0064_NF_SPOUSE_CSE_PERSON";

          var field15 = GetField(export.SpouseCsePerson, "number");

          field15.Error = true;

          break;
        case 10:
          ExitState = "OE0061_NF_MARRIAGE_HISTORY";

          var field16 = GetField(export.SpouseCsePerson, "number");

          field16.Error = true;

          break;
        case 11:
          ExitState = "OE0058_NF_CONTACT";

          var field17 = GetField(export.SpouseCsePerson, "number");

          field17.Error = true;

          break;
        case 12:
          ExitState = "MARRIAGE_CITY_REQD";

          var field18 =
            GetField(export.MarriageHistory, "marriageCertificateCity");

          field18.Error = true;

          break;
        case 13:
          ExitState = "OE0037_INVALID_MARR_STATE";

          var field19 =
            GetField(export.MarriageHistory, "marriageCertificateState");

          field19.Error = true;

          break;
        case 14:
          ExitState = "MARRIAGE_STATE_REQD";

          var field20 =
            GetField(export.MarriageHistory, "marriageCertificateState");

          field20.Error = true;

          break;
        case 15:
          ExitState = "OE0029_INVALID_DIVORCE_STATE";

          var field21 = GetField(export.MarriageHistory, "divorceState");

          field21.Error = true;

          break;
        case 16:
          ExitState = "OE0039_INVALID_MARR_DATE";

          var field22 = GetField(export.MarriageHistory, "marriageDate");

          field22.Error = true;

          break;
        case 17:
          ExitState = "OE0038_INVALID_MARR_COUNTRY";

          var field23 = GetField(export.MarriageHistory, "marriageCountry");

          field23.Error = true;

          break;
        case 18:
          ExitState = "OE0027_INVALID_DIVORCE_COUNTRY";

          var field24 = GetField(export.MarriageHistory, "divorceCountry");

          field24.Error = true;

          break;
        case 19:
          ExitState = "OE0003_INVALID_SEPARATION_DATE";

          var field25 = GetField(export.MarriageHistory, "separationDate");

          field25.Error = true;

          break;
        case 20:
          ExitState = "OE0002_INVALID_DIVORCE_DATE";

          var field26 = GetField(export.MarriageHistory, "divorceDate");

          field26.Error = true;

          break;
        case 21:
          ExitState = "OE0111_INVALID_DIV_PETITION_DT";

          var field27 = GetField(export.MarriageHistory, "divorcePetitionDate");

          field27.Error = true;

          break;
        case 22:
          ExitState = "OE0122_DUPL_MARH_RECORD";

          var field28 = GetField(export.PrimeCsePerson, "number");

          field28.Error = true;

          break;
        case 23:
          ExitState = "OE0123_INVALID_DIV_PENDING_IND";

          var field29 = GetField(export.MarriageHistory, "divorcePendingInd");

          field29.Error = true;

          break;
        case 24:
          ExitState = "DIVORCE_CITY_REQD";

          var field30 = GetField(export.MarriageHistory, "divorceCity");

          field30.Error = true;

          break;
        case 25:
          ExitState = "DIVORCE_STATE_REQD";

          var field31 = GetField(export.MarriageHistory, "divorceState");

          field31.Error = true;

          break;
        case 26:
          ExitState = "SPOUSE_FIRST_NAME_REQD";

          var field32 = GetField(export.SpouseContact, "nameFirst");

          field32.Error = true;

          break;
        case 27:
          ExitState = "SPOUSE_LAST_NAME_REQD";

          var field33 = GetField(export.SpouseContact, "nameLast");

          field33.Error = true;

          break;
        default:
          ExitState = "OE0004_UNKNOWN_ERROR_CODE";

          break;
      }

      --local.ErrorCodes.Index;
      local.ErrorCodes.CheckSize();
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveContact(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveErrorCodes(OeMarhValidatMarriageHistory.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.EntryErrorCode.Count = source.EntryErrorCode.Count;
  }

  private static void MoveMarriageHistory(MarriageHistory source,
    MarriageHistory target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
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

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.PrimeCsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Work.Flag = useExport.Work.Flag;
  }

  private void UseOeMarhCreateMarriageHistory()
  {
    var useImport = new OeMarhCreateMarriageHistory.Import();
    var useExport = new OeMarhCreateMarriageHistory.Export();

    useImport.New1.Assign(export.MarriageHistory);
    useImport.Prime.Number = export.PrimeCsePerson.Number;
    useImport.SpouseCsePerson.Number = export.SpouseCsePerson.Number;
    useImport.SpouseContact.Assign(export.SpouseContact);

    Call(OeMarhCreateMarriageHistory.Execute, useImport, useExport);

    export.MarriageHistory.Assign(useExport.New1);
    MoveMarriageHistory(useExport.UpdateTimestamps, export.UpdateTimestamp);
    export.SpouseContact.Assign(useExport.Spouse);
  }

  private void UseOeMarhDeleteMarriageHistory()
  {
    var useImport = new OeMarhDeleteMarriageHistory.Import();
    var useExport = new OeMarhDeleteMarriageHistory.Export();

    useImport.Prime.Number = export.PrimeCsePerson.Number;
    useImport.Existing.Identifier =
      export.HiddenSelectedMarriageHistory.Identifier;

    Call(OeMarhDeleteMarriageHistory.Execute, useImport, useExport);
  }

  private void UseOeMarhDisplayMarriageHistory()
  {
    var useImport = new OeMarhDisplayMarriageHistory.Import();
    var useExport = new OeMarhDisplayMarriageHistory.Export();

    useImport.DispSpecificMhistRec.Flag = local.DispSpecificMhistRec.Flag;
    useImport.Selected.Identifier = export.MarriageHistory.Identifier;
    useImport.SelectedPrime.Number = export.PrimeCsePerson.Number;

    Call(OeMarhDisplayMarriageHistory.Execute, useImport, useExport);

    MoveCodeValue(useExport.DivorceCountry, export.DispDivorceCountry);
    MoveCodeValue(useExport.MarriageCountry, export.DispMarriageCountry);
    MoveMarriageHistory(useExport.UpdateTimestamps, export.UpdateTimestamp);
    export.SpouseCsePersonsWorkSet.FormattedName =
      useExport.SpouseCsePersonsWorkSet.FormattedName;
    export.PrimeCsePersonsWorkSet.FormattedName =
      useExport.PrimeCsePersonsWorkSet.FormattedName;
    export.MarriageHistory.Assign(useExport.MarriageHistory);
    MoveContact(useExport.SpouseContact, export.SpouseContact);
    export.SpouseCsePerson.Number = useExport.SpouseCsePerson.Number;
    export.PrimeCsePerson.Number = useExport.PrimeCsePerson.Number;
  }

  private void UseOeMarhUpdateMarriageHistory()
  {
    var useImport = new OeMarhUpdateMarriageHistory.Import();
    var useExport = new OeMarhUpdateMarriageHistory.Export();

    useImport.Prime.Number = export.PrimeCsePerson.Number;
    useImport.ExistingSpouseCsePerson.Number =
      export.HiddenSelectedSpouse.Number;
    useImport.ExistingSpouseContact.Assign(export.HiddenSelectedContact);
    useImport.Existing.Assign(export.HiddenSelectedMarriageHistory);
    useImport.NewSpouseCsePerson.Number = export.SpouseCsePerson.Number;
    useImport.NewSpouseContact.Assign(export.SpouseContact);
    useImport.New1.Assign(export.MarriageHistory);

    Call(OeMarhUpdateMarriageHistory.Execute, useImport, useExport);

    export.SpouseCsePersonsWorkSet.FormattedName =
      useExport.NewSpouseCsePersonsWorkSet.FormattedName;
    MoveMarriageHistory(useExport.UpdateTimestamp, export.UpdateTimestamp);
    export.SpouseContact.Assign(useExport.NewSpouseContact);
  }

  private void UseOeMarhValidatMarriageHistory1()
  {
    var useImport = new OeMarhValidatMarriageHistory.Import();
    var useExport = new OeMarhValidatMarriageHistory.Export();

    useImport.Spouse.FormattedName =
      export.SpouseCsePersonsWorkSet.FormattedName;
    useImport.PrimeCsePersonsWorkSet.FormattedName =
      export.PrimeCsePersonsWorkSet.FormattedName;
    useImport.CurrentSpouseCsePerson.Number = import.HiddenSelectdSpouse.Number;
    useImport.CurrentSpouseContact.ContactNumber =
      import.HiddenSelectedContact.ContactNumber;
    useImport.Current.Assign(import.HiddenSelectedMarriageHistory);
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.PrimeCsePerson.Number = export.PrimeCsePerson.Number;
    useImport.NewSpouseCsePerson.Number = export.SpouseCsePerson.Number;
    useImport.NewSpouseContact.Assign(export.SpouseContact);
    useImport.New1.Assign(export.MarriageHistory);

    Call(OeMarhValidatMarriageHistory.Execute, useImport, useExport);

    export.SpouseCsePersonsWorkSet.FormattedName =
      useExport.SpouseCsePersonsWorkSet.FormattedName;
    export.SpouseCsePerson.Number = useExport.SpouseCsePerson.Number;
    export.PrimeCsePersonsWorkSet.FormattedName =
      useExport.PrimeCsePersonsWorkSet.FormattedName;
    export.PrimeCsePerson.Number = useExport.PrimeCsePerson.Number;
    MoveCodeValue(useExport.DivorceCountry, export.DispDivorceCountry);
    MoveCodeValue(useExport.MarriageCountry, export.DispMarriageCountry);
    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseOeMarhValidatMarriageHistory2()
  {
    var useImport = new OeMarhValidatMarriageHistory.Import();
    var useExport = new OeMarhValidatMarriageHistory.Export();

    useImport.Spouse.FormattedName =
      export.SpouseCsePersonsWorkSet.FormattedName;
    useImport.PrimeCsePersonsWorkSet.FormattedName =
      export.PrimeCsePersonsWorkSet.FormattedName;
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.PrimeCsePerson.Number = export.PrimeCsePerson.Number;
    useImport.NewSpouseCsePerson.Number = export.SpouseCsePerson.Number;
    useImport.NewSpouseContact.Assign(export.SpouseContact);
    useImport.New1.Assign(export.MarriageHistory);

    Call(OeMarhValidatMarriageHistory.Execute, useImport, useExport);

    export.SpouseCsePersonsWorkSet.FormattedName =
      useExport.SpouseCsePersonsWorkSet.FormattedName;
    export.SpouseCsePerson.Number = useExport.SpouseCsePerson.Number;
    export.PrimeCsePersonsWorkSet.FormattedName =
      useExport.PrimeCsePersonsWorkSet.FormattedName;
    export.PrimeCsePerson.Number = useExport.PrimeCsePerson.Number;
    MoveCodeValue(useExport.DivorceCountry, export.DispDivorceCountry);
    MoveCodeValue(useExport.MarriageCountry, export.DispMarriageCountry);
    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseOeMarhValidatMarriageHistory3()
  {
    var useImport = new OeMarhValidatMarriageHistory.Import();
    var useExport = new OeMarhValidatMarriageHistory.Export();

    useImport.CurrentSpouseCsePerson.Number =
      export.HiddenSelectedSpouse.Number;
    useImport.CurrentSpouseContact.ContactNumber =
      export.HiddenSelectedContact.ContactNumber;
    useImport.Current.Assign(export.HiddenSelectedMarriageHistory);
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.PrimeCsePerson.Number = export.PrimeCsePerson.Number;

    Call(OeMarhValidatMarriageHistory.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.PrimeCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
    /// <summary>
    /// A value of ListSpouseCsePersons.
    /// </summary>
    [JsonPropertyName("listSpouseCsePersons")]
    public Standard ListSpouseCsePersons
    {
      get => listSpouseCsePersons ??= new();
      set => listSpouseCsePersons = value;
    }

    /// <summary>
    /// A value of SelectedForList.
    /// </summary>
    [JsonPropertyName("selectedForList")]
    public Code SelectedForList
    {
      get => selectedForList ??= new();
      set => selectedForList = value;
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
    /// A value of ListDivorceStateCodes.
    /// </summary>
    [JsonPropertyName("listDivorceStateCodes")]
    public Standard ListDivorceStateCodes
    {
      get => listDivorceStateCodes ??= new();
      set => listDivorceStateCodes = value;
    }

    /// <summary>
    /// A value of ListDivorceCountry.
    /// </summary>
    [JsonPropertyName("listDivorceCountry")]
    public Standard ListDivorceCountry
    {
      get => listDivorceCountry ??= new();
      set => listDivorceCountry = value;
    }

    /// <summary>
    /// A value of ListMarriageStateCodes.
    /// </summary>
    [JsonPropertyName("listMarriageStateCodes")]
    public Standard ListMarriageStateCodes
    {
      get => listMarriageStateCodes ??= new();
      set => listMarriageStateCodes = value;
    }

    /// <summary>
    /// A value of ListMarriageCountry.
    /// </summary>
    [JsonPropertyName("listMarriageCountry")]
    public Standard ListMarriageCountry
    {
      get => listMarriageCountry ??= new();
      set => listMarriageCountry = value;
    }

    /// <summary>
    /// A value of ListPrimeCsePersons.
    /// </summary>
    [JsonPropertyName("listPrimeCsePersons")]
    public Standard ListPrimeCsePersons
    {
      get => listPrimeCsePersons ??= new();
      set => listPrimeCsePersons = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
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
    /// A value of UpdateTimestamp.
    /// </summary>
    [JsonPropertyName("updateTimestamp")]
    public MarriageHistory UpdateTimestamp
    {
      get => updateTimestamp ??= new();
      set => updateTimestamp = value;
    }

    /// <summary>
    /// A value of SpouseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("spouseCsePersonsWorkSet")]
    public CsePersonsWorkSet SpouseCsePersonsWorkSet
    {
      get => spouseCsePersonsWorkSet ??= new();
      set => spouseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PrimeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("primeCsePersonsWorkSet")]
    public CsePersonsWorkSet PrimeCsePersonsWorkSet
    {
      get => primeCsePersonsWorkSet ??= new();
      set => primeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of DispDivorceCountry.
    /// </summary>
    [JsonPropertyName("dispDivorceCountry")]
    public CodeValue DispDivorceCountry
    {
      get => dispDivorceCountry ??= new();
      set => dispDivorceCountry = value;
    }

    /// <summary>
    /// A value of DispMarriageCountry.
    /// </summary>
    [JsonPropertyName("dispMarriageCountry")]
    public CodeValue DispMarriageCountry
    {
      get => dispMarriageCountry ??= new();
      set => dispMarriageCountry = value;
    }

    /// <summary>
    /// A value of LinkLmhistSelctedPrime.
    /// </summary>
    [JsonPropertyName("linkLmhistSelctedPrime")]
    public CsePerson LinkLmhistSelctedPrime
    {
      get => linkLmhistSelctedPrime ??= new();
      set => linkLmhistSelctedPrime = value;
    }

    /// <summary>
    /// A value of PrimeCsePerson.
    /// </summary>
    [JsonPropertyName("primeCsePerson")]
    public CsePerson PrimeCsePerson
    {
      get => primeCsePerson ??= new();
      set => primeCsePerson = value;
    }

    /// <summary>
    /// A value of SpouseCsePerson.
    /// </summary>
    [JsonPropertyName("spouseCsePerson")]
    public CsePerson SpouseCsePerson
    {
      get => spouseCsePerson ??= new();
      set => spouseCsePerson = value;
    }

    /// <summary>
    /// A value of SpouseContact.
    /// </summary>
    [JsonPropertyName("spouseContact")]
    public Contact SpouseContact
    {
      get => spouseContact ??= new();
      set => spouseContact = value;
    }

    /// <summary>
    /// A value of MarriageHistory.
    /// </summary>
    [JsonPropertyName("marriageHistory")]
    public MarriageHistory MarriageHistory
    {
      get => marriageHistory ??= new();
      set => marriageHistory = value;
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
    /// A value of HiddenSelectedPrime.
    /// </summary>
    [JsonPropertyName("hiddenSelectedPrime")]
    public CsePerson HiddenSelectedPrime
    {
      get => hiddenSelectedPrime ??= new();
      set => hiddenSelectedPrime = value;
    }

    /// <summary>
    /// A value of HiddenSelectdSpouse.
    /// </summary>
    [JsonPropertyName("hiddenSelectdSpouse")]
    public CsePerson HiddenSelectdSpouse
    {
      get => hiddenSelectdSpouse ??= new();
      set => hiddenSelectdSpouse = value;
    }

    /// <summary>
    /// A value of HiddenSelectedContact.
    /// </summary>
    [JsonPropertyName("hiddenSelectedContact")]
    public Contact HiddenSelectedContact
    {
      get => hiddenSelectedContact ??= new();
      set => hiddenSelectedContact = value;
    }

    /// <summary>
    /// A value of HiddenSelectedMarriageHistory.
    /// </summary>
    [JsonPropertyName("hiddenSelectedMarriageHistory")]
    public MarriageHistory HiddenSelectedMarriageHistory
    {
      get => hiddenSelectedMarriageHistory ??= new();
      set => hiddenSelectedMarriageHistory = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private Standard listSpouseCsePersons;
    private Code selectedForList;
    private CodeValue dlgflwSelected;
    private Standard listDivorceStateCodes;
    private Standard listDivorceCountry;
    private Standard listMarriageStateCodes;
    private Standard listMarriageCountry;
    private Standard listPrimeCsePersons;
    private Case1 case1;
    private CsePersonsWorkSet selected;
    private Common hiddenDisplayPerformed;
    private MarriageHistory updateTimestamp;
    private CsePersonsWorkSet spouseCsePersonsWorkSet;
    private CsePersonsWorkSet primeCsePersonsWorkSet;
    private CodeValue dispDivorceCountry;
    private CodeValue dispMarriageCountry;
    private CsePerson linkLmhistSelctedPrime;
    private CsePerson primeCsePerson;
    private CsePerson spouseCsePerson;
    private Contact spouseContact;
    private MarriageHistory marriageHistory;
    private Common hiddenPrevUserAction;
    private CsePerson hiddenSelectedPrime;
    private CsePerson hiddenSelectdSpouse;
    private Contact hiddenSelectedContact;
    private MarriageHistory hiddenSelectedMarriageHistory;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ListSpouseCsePersons.
    /// </summary>
    [JsonPropertyName("listSpouseCsePersons")]
    public Standard ListSpouseCsePersons
    {
      get => listSpouseCsePersons ??= new();
      set => listSpouseCsePersons = value;
    }

    /// <summary>
    /// A value of SelectedForList.
    /// </summary>
    [JsonPropertyName("selectedForList")]
    public Code SelectedForList
    {
      get => selectedForList ??= new();
      set => selectedForList = value;
    }

    /// <summary>
    /// A value of ListDivorceStateCodes.
    /// </summary>
    [JsonPropertyName("listDivorceStateCodes")]
    public Standard ListDivorceStateCodes
    {
      get => listDivorceStateCodes ??= new();
      set => listDivorceStateCodes = value;
    }

    /// <summary>
    /// A value of ListDivorceCountry.
    /// </summary>
    [JsonPropertyName("listDivorceCountry")]
    public Standard ListDivorceCountry
    {
      get => listDivorceCountry ??= new();
      set => listDivorceCountry = value;
    }

    /// <summary>
    /// A value of ListMarriageStateCodes.
    /// </summary>
    [JsonPropertyName("listMarriageStateCodes")]
    public Standard ListMarriageStateCodes
    {
      get => listMarriageStateCodes ??= new();
      set => listMarriageStateCodes = value;
    }

    /// <summary>
    /// A value of ListMarriageCountry.
    /// </summary>
    [JsonPropertyName("listMarriageCountry")]
    public Standard ListMarriageCountry
    {
      get => listMarriageCountry ??= new();
      set => listMarriageCountry = value;
    }

    /// <summary>
    /// A value of ListPrimeCsePersons.
    /// </summary>
    [JsonPropertyName("listPrimeCsePersons")]
    public Standard ListPrimeCsePersons
    {
      get => listPrimeCsePersons ??= new();
      set => listPrimeCsePersons = value;
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
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of UpdateTimestamp.
    /// </summary>
    [JsonPropertyName("updateTimestamp")]
    public MarriageHistory UpdateTimestamp
    {
      get => updateTimestamp ??= new();
      set => updateTimestamp = value;
    }

    /// <summary>
    /// A value of SpouseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("spouseCsePersonsWorkSet")]
    public CsePersonsWorkSet SpouseCsePersonsWorkSet
    {
      get => spouseCsePersonsWorkSet ??= new();
      set => spouseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PrimeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("primeCsePersonsWorkSet")]
    public CsePersonsWorkSet PrimeCsePersonsWorkSet
    {
      get => primeCsePersonsWorkSet ??= new();
      set => primeCsePersonsWorkSet = value;
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
    /// A value of LinkLmhistSelctedPrime.
    /// </summary>
    [JsonPropertyName("linkLmhistSelctedPrime")]
    public CsePerson LinkLmhistSelctedPrime
    {
      get => linkLmhistSelctedPrime ??= new();
      set => linkLmhistSelctedPrime = value;
    }

    /// <summary>
    /// A value of PrimeCsePerson.
    /// </summary>
    [JsonPropertyName("primeCsePerson")]
    public CsePerson PrimeCsePerson
    {
      get => primeCsePerson ??= new();
      set => primeCsePerson = value;
    }

    /// <summary>
    /// A value of SpouseCsePerson.
    /// </summary>
    [JsonPropertyName("spouseCsePerson")]
    public CsePerson SpouseCsePerson
    {
      get => spouseCsePerson ??= new();
      set => spouseCsePerson = value;
    }

    /// <summary>
    /// A value of SpouseContact.
    /// </summary>
    [JsonPropertyName("spouseContact")]
    public Contact SpouseContact
    {
      get => spouseContact ??= new();
      set => spouseContact = value;
    }

    /// <summary>
    /// A value of MarriageHistory.
    /// </summary>
    [JsonPropertyName("marriageHistory")]
    public MarriageHistory MarriageHistory
    {
      get => marriageHistory ??= new();
      set => marriageHistory = value;
    }

    /// <summary>
    /// A value of HiddenSelectedPrime.
    /// </summary>
    [JsonPropertyName("hiddenSelectedPrime")]
    public CsePerson HiddenSelectedPrime
    {
      get => hiddenSelectedPrime ??= new();
      set => hiddenSelectedPrime = value;
    }

    /// <summary>
    /// A value of HiddenSelectedSpouse.
    /// </summary>
    [JsonPropertyName("hiddenSelectedSpouse")]
    public CsePerson HiddenSelectedSpouse
    {
      get => hiddenSelectedSpouse ??= new();
      set => hiddenSelectedSpouse = value;
    }

    /// <summary>
    /// A value of HiddenSelectedContact.
    /// </summary>
    [JsonPropertyName("hiddenSelectedContact")]
    public Contact HiddenSelectedContact
    {
      get => hiddenSelectedContact ??= new();
      set => hiddenSelectedContact = value;
    }

    /// <summary>
    /// A value of HiddenSelectedMarriageHistory.
    /// </summary>
    [JsonPropertyName("hiddenSelectedMarriageHistory")]
    public MarriageHistory HiddenSelectedMarriageHistory
    {
      get => hiddenSelectedMarriageHistory ??= new();
      set => hiddenSelectedMarriageHistory = value;
    }

    /// <summary>
    /// A value of DispDivorceCountry.
    /// </summary>
    [JsonPropertyName("dispDivorceCountry")]
    public CodeValue DispDivorceCountry
    {
      get => dispDivorceCountry ??= new();
      set => dispDivorceCountry = value;
    }

    /// <summary>
    /// A value of DispMarriageCountry.
    /// </summary>
    [JsonPropertyName("dispMarriageCountry")]
    public CodeValue DispMarriageCountry
    {
      get => dispMarriageCountry ??= new();
      set => dispMarriageCountry = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private Standard listSpouseCsePersons;
    private Code selectedForList;
    private Standard listDivorceStateCodes;
    private Standard listDivorceCountry;
    private Standard listMarriageStateCodes;
    private Standard listMarriageCountry;
    private Standard listPrimeCsePersons;
    private Case1 case1;
    private Common hiddenDisplayPerformed;
    private MarriageHistory updateTimestamp;
    private CsePersonsWorkSet spouseCsePersonsWorkSet;
    private CsePersonsWorkSet primeCsePersonsWorkSet;
    private Common hiddenPrevUserAction;
    private CsePerson linkLmhistSelctedPrime;
    private CsePerson primeCsePerson;
    private CsePerson spouseCsePerson;
    private Contact spouseContact;
    private MarriageHistory marriageHistory;
    private CsePerson hiddenSelectedPrime;
    private CsePerson hiddenSelectedSpouse;
    private Contact hiddenSelectedContact;
    private MarriageHistory hiddenSelectedMarriageHistory;
    private CodeValue dispDivorceCountry;
    private CodeValue dispMarriageCountry;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of EntryErrorCode.
      /// </summary>
      [JsonPropertyName("entryErrorCode")]
      public Common EntryErrorCode
      {
        get => entryErrorCode ??= new();
        set => entryErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common entryErrorCode;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of DispSpecificMhistRec.
    /// </summary>
    [JsonPropertyName("dispSpecificMhistRec")]
    public Common DispSpecificMhistRec
    {
      get => dispSpecificMhistRec ??= new();
      set => dispSpecificMhistRec = value;
    }

    /// <summary>
    /// A value of BlankContact.
    /// </summary>
    [JsonPropertyName("blankContact")]
    public Contact BlankContact
    {
      get => blankContact ??= new();
      set => blankContact = value;
    }

    /// <summary>
    /// A value of BlankMarriageHistory.
    /// </summary>
    [JsonPropertyName("blankMarriageHistory")]
    public MarriageHistory BlankMarriageHistory
    {
      get => blankMarriageHistory ??= new();
      set => blankMarriageHistory = value;
    }

    /// <summary>
    /// A value of BlankCsePerson.
    /// </summary>
    [JsonPropertyName("blankCsePerson")]
    public CsePerson BlankCsePerson
    {
      get => blankCsePerson ??= new();
      set => blankCsePerson = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Standard UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Common work;
    private CodeValue codeValue;
    private Common dispSpecificMhistRec;
    private Contact blankContact;
    private MarriageHistory blankMarriageHistory;
    private CsePerson blankCsePerson;
    private Standard userAction;
    private Common lastErrorEntryNo;
    private Array<ErrorCodesGroup> errorCodes;
    private TextWorkArea textWorkArea;
  }
#endregion
}

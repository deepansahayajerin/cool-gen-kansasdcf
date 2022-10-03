// Program: OE_ATTY_PRIVATE_ATTORNEY, ID: 372179071, model: 746.
// Short name: SWEATTYP
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
/// A program: OE_ATTY_PRIVATE_ATTORNEY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure step facilitates maintenance of Private Attorney details of a
/// CSE Person.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeAttyPrivateAttorney: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_ATTY_PRIVATE_ATTORNEY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeAttyPrivateAttorney(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeAttyPrivateAttorney.
  /// </summary>
  public OeAttyPrivateAttorney(IContext context, Import import, Export export):
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
    // This procedure step facilitates maintenance of CSE Person Attorney 
    // details.
    // PROCESSING:
    // To create a new set of Private Attorney details (PERSON_PRIVATE_ATTORNEY 
    // and PRIVATE_ATTORNEY_ADDRESS) the user will enter the attorney details
    // and press CREATE key. The system will validate the details and update the
    // database.
    // To update existing attorney details, the user will first display the 
    // required attorney details and will modify the details and press UPDATE
    // key. The system will validate the details and then update the database.
    // When the effective date of the attorney address is changed, the system 
    // will create a new PRIVATE_ATTORNEY_ADDRESS record.
    // This procedure step also allows to view next and previous attorney 
    // records for the given CSE Person-Case.
    // ACTION BLOCKS:
    // The following action blocks are called from this procedure step:
    // 	OE_CREATE_PRIV_ATTORNEY
    // 	OE_UPDATE_PRIV_ATTORNEY
    // 	OE_DELETE_PRIV_ATTORNEY
    // 	OE_DISPLAY_PRIV_ATTORNEY
    // 	OE_VALIDATE_PRIV_ATTORNEY
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  PERSON_PRIVATE_ATTORNEY	C R U D
    //  PRIVATE_ATTORNEY_ADDRESS	C R U D
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	02/22/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	02/22/95		Initial coding
    // Sid	02/12/96		Rework
    // Regan	06/26/96		Add Left Pad EAB.
    // Marchman 11/08/96               Add new security and next tran
    // Chowdhary	02/19/97	Add case specific header.
    // Madhu  Kumar   05/15/01  Edit check for  zip codes.
    // Andrew Convery    03/08/04   201317  Modified highlighting of zip code 
    // field for
    //                                      
    // error code 37.
    // Andrew Convery  04/27/04 PR# 204444 (#2) - Allow updating of Attorney 
    // fields in
    //                                            
    // all cases.
    //                          PR# 204444 (#1) - Correction in display of 
    // messages at bottom of screen.
    // Andrew Convery 12/01/05  PR# 232681 Allow entry of Attorney without Case 
    // Number
    //                                     
    // No changes in this module.  Changes
    // are in
    // OE_ATTY_VALIDATE_PRIV_ATTORNEY
    // 	
    // 12/05/00 M. Lachowicz           Changed header line
    //                                 
    // WR 298.
    // JHarden    03/17/17  CQ53818  add email address to ATTY screen
    // JHarden    05/27/17  CQ57453  add fields consent, note, and bar #.
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    // =============================================
    // The following three MOVE statements are
    // specifically moved to this place for this
    // screen (ATTY) since we dont want to clear
    // person number
    // =============================================
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, local.CsePersonsWorkSet);
    export.Next.Number = import.Next.Number;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);

    // 12/05/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/05/00 M.L End
    if (!IsEmpty(export.Case1.Number))
    {
      local.TextWorkArea.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
      local.CsePersonsWorkSet.Number = export.CsePerson.Number;
    }

    export.ApInactive.Flag = import.ApInactive.Flag;
    export.CaseOpen.Flag = import.CaseOpen.Flag;

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.PersonPrivateAttorney.Assign(import.PersonPrivateAttorney);
    export.PrivateAttorneyAddress.Assign(import.PrivateAttorneyAddress);

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CaseNumber = export.Next.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      export.Hidden.CsePersonNumberAp = export.CsePerson.Number;
      export.Hidden.CourtCaseNumber =
        export.PersonPrivateAttorney.CourtCaseNumber ?? "";
      export.Hidden.CourtOrderNumber =
        export.PersonPrivateAttorney.CourtCaseNumber ?? "";
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    // PR136902. On 1-25-02 added "Create" to the if statement in the security 
    // cab call. ljb
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "CREATE") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // PR# 204444 (#2) - Commenting out the below will allow updating of Attoney
    // fields
    //              in all cases.
    // **** end   group C ****
    local.HighlightError.Flag = "N";
    export.ListCsePersons.PromptField = import.ListCsePersons.PromptField;
    export.ListStates.PromptField = import.ListStates.PromptField;

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME") && !Equal(global.Command, "RETCDVL"))
    {
      export.ListCsePersons.PromptField = "";
      export.ListStates.PromptField = "";
    }

    MovePersonPrivateAttorney5(import.UpdateStamp, export.UpdateStamp);
    export.Country.Description = import.Country.Description;
    export.HiddenPrevCase.Number = import.HiddenPrevCase.Number;
    export.HiddenPrevCsePerson.Number = import.HiddenPrevCsePerson.Number;
    export.HiddenPrevPersonPrivateAttorney.Identifier =
      import.HiddenPrevPersonPrivateAttorney.Identifier;
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    local.UserAction.Command = global.Command;

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      export.ListCsePersons.PromptField = "";
      export.CsePerson.Number = import.CsePersonsWorkSet.Number;
      local.UserAction.Command = "DISPLAY";
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // Command DISPLAY displays given Attorney rec.
    // Command PREV displays previous Attorney rec.
    // Command NEXT displays next Attorney rec.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV") || Equal(global.Command, "ADDTASK"))
    {
      if (IsEmpty(export.CsePerson.Number))
      {
        MovePersonPrivateAttorney1(local.BlankPersonPrivateAttorney,
          export.PersonPrivateAttorney);
        MovePersonPrivateAttorney5(local.BlankPersonPrivateAttorney,
          export.UpdateStamp);
        export.PrivateAttorneyAddress.Assign(local.BlankPrivateAttorneyAddress);

        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NO_REQUIRED";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ************************************************************************
      // PR#204444 (#1) - Commented out and copied to location below.
      // ************************************************************************
      UseOeAttyDisplayPrivAttorney();

      if (IsEmpty(local.CsePersonsWorkSet.FormattedName) && Equal
        (local.CsePersonsWorkSet.Number, export.CsePersonsWorkSet.Number))
      {
        local.CsePersonsWorkSet.FormattedName =
          export.CsePersonsWorkSet.FormattedName;
      }

      // ************************************************************************
      // PR#204444 (#1) - Moved here from above.
      // ************************************************************************
      UseOeCabCheckCaseMember();

      if (!IsEmpty(local.WorkError.Flag))
      {
        var field1 = GetField(export.Case1, "number");

        field1.Error = true;

        var field2 = GetField(export.CsePerson, "number");

        field2.Error = true;

        ExitState = "OE0000_CASE_MEMBER_NE";

        return;
      }

      if (IsEmpty(export.CsePersonsWorkSet.FormattedName) && (
        Equal(export.CsePersonsWorkSet.Number, export.CsePerson.Number) || Equal
        (export.CsePersonsWorkSet.Number, import.CsePersonsWorkSet.Number)))
      {
        export.CsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;
      }

      if (!IsEmpty(export.Case1.Number))
      {
        UseSiReadOfficeOspHeader();
      }

      export.Next.Number = export.Case1.Number;

      if (AsChar(local.Dismissed.Flag) == 'Y')
      {
        export.DismissedLiteral.Text30 = "Court Case Dismissed";
      }
      else
      {
        export.DismissedLiteral.Text30 = "";
      }

      if (AsChar(local.AttyDisplayed.Flag) == 'Y')
      {
        export.HiddenDisplayPerformed.Flag = "Y";
      }
      else
      {
        export.HiddenDisplayPerformed.Flag = "N";
      }

      if (IsExitState("OE0000_CSE_PERSON_REL_MULT_CASES"))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;
      }
      else if (IsExitState("NO_PRIVATE_ATTORNEY_FOR_PERSON"))
      {
        MovePersonPrivateAttorney1(local.BlankPersonPrivateAttorney,
          export.PersonPrivateAttorney);
        MovePersonPrivateAttorney5(local.BlankPersonPrivateAttorney,
          export.UpdateStamp);
        export.PrivateAttorneyAddress.Assign(local.BlankPrivateAttorneyAddress);

        var field1 = GetField(export.CsePerson, "number");

        field1.Error = true;

        var field2 = GetField(export.Case1, "number");

        field2.Error = true;

        var field3 = GetField(export.PersonPrivateAttorney, "courtCaseNumber");

        field3.Error = true;

        var field4 =
          GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

        field4.Error = true;

        var field5 =
          GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

        field5.Error = true;

        var field6 = GetField(export.PersonPrivateAttorney, "tribCountry");

        field6.Error = true;
      }
      else if (IsExitState("OE0079_NO_MORE_ATTR_TO_DISPLAY"))
      {
        export.PersonPrivateAttorney.Identifier = 0;
      }
      else if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        if (AsChar(export.ApInactive.Flag) == 'Y')
        {
          ExitState = "DISPLAY_OK_FOR_INACTIVE_AP";
        }

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }
      }
      else if (IsExitState("CSE_PERSON_NF"))
      {
        MovePersonPrivateAttorney1(local.BlankPersonPrivateAttorney,
          export.PersonPrivateAttorney);
        MovePersonPrivateAttorney5(local.BlankPersonPrivateAttorney,
          export.UpdateStamp);
        export.PrivateAttorneyAddress.Assign(local.BlankPrivateAttorneyAddress);

        var field = GetField(export.CsePerson, "number");

        field.Error = true;
      }
      else if (IsExitState("CASE_NF"))
      {
        MovePersonPrivateAttorney1(local.BlankPersonPrivateAttorney,
          export.PersonPrivateAttorney);
        MovePersonPrivateAttorney5(local.BlankPersonPrivateAttorney,
          export.UpdateStamp);
        export.PrivateAttorneyAddress.Assign(local.BlankPrivateAttorneyAddress);

        var field = GetField(export.Case1, "number");

        field.Error = true;
      }
      else if (IsExitState("PERSONS_ATTORNEY_NF"))
      {
        export.PersonPrivateAttorney.Identifier =
          import.PersonPrivateAttorney.Identifier;
        MovePersonPrivateAttorney1(local.BlankPersonPrivateAttorney,
          export.PersonPrivateAttorney);
        MovePersonPrivateAttorney5(local.BlankPersonPrivateAttorney,
          export.UpdateStamp);
        export.PrivateAttorneyAddress.Assign(local.BlankPrivateAttorneyAddress);

        var field = GetField(export.PersonPrivateAttorney, "identifier");

        field.Error = true;
      }
      else
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        // --------------------------------------------
        // leave the cursor at the first field.
        // --------------------------------------------
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "ADDTASK":
        // ---------------------------------------------
        // Action already taken.
        // ---------------------------------------------
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Action already taken.
        // ---------------------------------------------
        break;
      case "NEXT":
        // ---------------------------------------------
        // Action already taken.
        // ---------------------------------------------
        break;
      case "PREV":
        // ---------------------------------------------
        // Action already taken.
        // ---------------------------------------------
        break;
      case "LIST":
        if (!IsEmpty(export.ListCsePersons.PromptField) && AsChar
          (export.ListCsePersons.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListCsePersons, "promptField");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.ListStates.PromptField) && AsChar
          (export.ListStates.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListStates, "promptField");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListCsePersons.PromptField) == 'S' && AsChar
          (export.ListStates.PromptField) == 'S')
        {
          var field1 = GetField(export.ListCsePersons, "promptField");

          field1.Error = true;

          var field2 = GetField(export.ListStates, "promptField");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.ListCsePersons.PromptField) == 'S')
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

        if (AsChar(export.ListStates.PromptField) == 'S')
        {
          export.ListCode.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETCDVL":
        if (AsChar(export.ListStates.PromptField) == 'S')
        {
          export.ListStates.PromptField = "";

          if (IsEmpty(import.Selected.Cdvalue))
          {
            var field = GetField(export.PrivateAttorneyAddress, "state");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.PrivateAttorneyAddress.State = import.Selected.Cdvalue;

            var field = GetField(export.PrivateAttorneyAddress, "zipCode5");

            field.Protected = false;
            field.Focused = true;
          }
        }

        global.Command = "DISPLAY";

        break;
      case "CREATE":
        export.HiddenDisplayPerformed.Flag = "N";
        export.PersonPrivateAttorney.Identifier = 0;

        // CQ57453
        local.ErrorCodes.Index = -1;
        local.ErrorCodes.Count = 0;

        if (!IsEmpty(export.PersonPrivateAttorney.ConsentIndicator) && AsChar
          (export.PersonPrivateAttorney.ConsentIndicator) != 'Y' && AsChar
          (export.PersonPrivateAttorney.ConsentIndicator) != 'N')
        {
          ++local.ErrorCodes.Index;
          local.ErrorCodes.CheckSize();

          local.ErrorCodes.Update.DetailErrorCode.Count = 46;
          local.HighlightError.Flag = "Y";
          local.LastErrorEntryNo.Count = local.ErrorCodes.Index + 1;

          break;
        }

        UseOeAttyValidatePrivAttorney();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // ---------------------------------------------
          // One or more errors encountered. Errors are highlighted below.
          // ---------------------------------------------
          local.HighlightError.Flag = "Y";

          break;
        }

        // The following exit state statement is required to clear any adabas 
        // error exit state set by adabas externals.
        ExitState = "ACO_NN0000_ALL_OK";
        UseOeAttyCreatePrivAttorney();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // Fatal error encountered during create.
          // ---------------------------------------------
          UseEabRollbackCics();

          break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        export.HiddenDisplayPerformed.Flag = "Y";

        break;
      case "UPDATE":
        // CQ57453
        export.HiddenConsent.ConsentIndicator =
          export.PersonPrivateAttorney.ConsentIndicator ?? "";

        if (!Equal(export.CsePerson.Number, import.HiddenPrevCsePerson.Number) ||
          !Equal(export.Case1.Number, import.HiddenPrevCase.Number) || export
          .PersonPrivateAttorney.Identifier != import
          .HiddenPrevPersonPrivateAttorney.Identifier || AsChar
          (import.HiddenDisplayPerformed.Flag) == 'N' || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(import.HiddenPrevUserAction.Command, "UPDATE") && !
          Equal(import.HiddenPrevUserAction.Command, "CREATE") && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        // CQ57453
        local.ErrorCodes.Index = -1;
        local.ErrorCodes.Count = 0;

        if (!IsEmpty(import.HiddenConsent.ConsentIndicator) && IsEmpty
          (export.PersonPrivateAttorney.ConsentIndicator))
        {
          ++local.ErrorCodes.Index;
          local.ErrorCodes.CheckSize();

          local.ErrorCodes.Update.DetailErrorCode.Count = 46;
          local.HighlightError.Flag = "Y";
          local.LastErrorEntryNo.Count = local.ErrorCodes.Index + 1;

          break;
        }

        if (!IsEmpty(export.PersonPrivateAttorney.ConsentIndicator) && AsChar
          (export.PersonPrivateAttorney.ConsentIndicator) != 'Y' && AsChar
          (export.PersonPrivateAttorney.ConsentIndicator) != 'N')
        {
          ++local.ErrorCodes.Index;
          local.ErrorCodes.CheckSize();

          local.ErrorCodes.Update.DetailErrorCode.Count = 46;
          local.HighlightError.Flag = "Y";
          local.LastErrorEntryNo.Count = local.ErrorCodes.Index + 1;

          break;
        }

        UseOeAttyValidatePrivAttorney();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // ---------------------------------------------
          // One or more errors encountered. Errors are highlighted below.
          // ---------------------------------------------
          local.HighlightError.Flag = "Y";

          break;
        }

        // The following exit state statement is required to clear any adabas 
        // error exit state set by adabas externals.
        ExitState = "ACO_NN0000_ALL_OK";
        UseOeAttyUpdatePrivAttorney();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // Fatal error encountered during update.
          // ---------------------------------------------
          UseEabRollbackCics();

          break;
        }

        // CQ57453
        export.HiddenConsent.ConsentIndicator =
          export.PersonPrivateAttorney.ConsentIndicator ?? "";
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        // ****************************************************************
        // * Per Jan Brigham, attorney delete functionality will be disabled
        // 2/4/99
        // **************************************************
        // R. Jean
        // ****************************************************************************************
        // 5-30-2007 Per PR 197619, delete functionality was requested to be 
        // activated
        // ***************************************************************************************
        if (!Equal(export.CsePerson.Number, import.HiddenPrevCsePerson.Number) ||
          !Equal(export.Case1.Number, import.HiddenPrevCase.Number) || export
          .PersonPrivateAttorney.Identifier != import
          .HiddenPrevPersonPrivateAttorney.Identifier || AsChar
          (import.HiddenDisplayPerformed.Flag) == 'N' || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(import.HiddenPrevUserAction.Command, "UPDATE") && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT"))
        {
          ExitState = "CO0000_DISPLAY_BEF_UPD_OR_DEL";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        export.HiddenDisplayPerformed.Flag = "N";
        UseOeAttyValidatePrivAttorney();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // ---------------------------------------------
          // One or more errors encountered. Errors are highlighted below.
          // ---------------------------------------------
          local.HighlightError.Flag = "Y";

          break;
        }

        // The following exit state statement is required to clear any adabas 
        // error exit state set by adabas externals.
        ExitState = "ACO_NN0000_ALL_OK";
        UseOeAttyDeletePrivAttorney();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // Fatal error encountered during delete.
          // ---------------------------------------------
          UseEabRollbackCics();

          break;
        }

        MovePersonPrivateAttorney1(local.BlankPersonPrivateAttorney,
          export.PersonPrivateAttorney);
        MovePersonPrivateAttorney5(local.BlankPersonPrivateAttorney,
          export.UpdateStamp);
        export.HiddenPrevPersonPrivateAttorney.Identifier =
          local.BlankPersonPrivateAttorney.Identifier;
        export.PrivateAttorneyAddress.Assign(local.BlankPrivateAttorneyAddress);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    export.HiddenPrevCsePerson.Number = export.CsePerson.Number;
    export.HiddenPrevCase.Number = export.Case1.Number;
    export.HiddenPrevPersonPrivateAttorney.Identifier =
      export.PersonPrivateAttorney.Identifier;

    if (AsChar(export.HiddenDisplayPerformed.Flag) == 'N')
    {
      export.HiddenPrevUserAction.Command = "INVALID";
    }
    else
    {
      export.HiddenPrevUserAction.Command = global.Command;
    }

    UseCabSetMaximumDiscontinueDate1();

    if (Equal(export.PersonPrivateAttorney.DateDismissed,
      local.InitZeroSetToMax.Date))
    {
      local.DateWorkArea.Date = export.PersonPrivateAttorney.DateDismissed;
      UseCabSetMaximumDiscontinueDate2();
      export.PersonPrivateAttorney.DateDismissed = local.DateWorkArea.Date;
    }

    // ---------------------------------------------
    // If any validation error encountered, highlight the errors encountered.
    // ---------------------------------------------
    if (AsChar(local.HighlightError.Flag) == 'Y')
    {
      local.ErrorCodes.Index = local.LastErrorEntryNo.Count - 1;
      local.ErrorCodes.CheckSize();

      while(local.ErrorCodes.Index >= 0)
      {
        switch(local.ErrorCodes.Item.DetailErrorCode.Count)
        {
          case 1:
            ExitState = "CSE_PERSON_NF";

            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;

            break;
          case 2:
            ExitState = "PERSONS_ATTORNEY_NF";

            var field2 = GetField(export.PersonPrivateAttorney, "identifier");

            field2.Error = true;

            break;
          case 3:
            ExitState = "PRIVATE_ATTORNEY_ADDRESS_NF";

            var field3 = GetField(export.PrivateAttorneyAddress, "street1");

            field3.Error = true;

            break;
          case 4:
            ExitState = "OE0000_DISCONTINUE_DATE_INVALID";

            var field4 =
              GetField(export.PersonPrivateAttorney, "dateDismissed");

            field4.Error = true;

            break;
          case 5:
            ExitState = "OE0072_INVALID_ATTR_LAST_NAME";

            var field5 = GetField(export.PersonPrivateAttorney, "lastName");

            field5.Error = true;

            break;
          case 6:
            ExitState = "OE0073_INVALID_ATTR_1ST_NAME";

            var field6 = GetField(export.PersonPrivateAttorney, "firstName");

            field6.Error = true;

            break;
          case 7:
            ExitState = "OE0074_INVALID_ATTORNEY_MI";

            var field7 =
              GetField(export.PersonPrivateAttorney, "middleInitial");

            field7.Error = true;

            break;
          case 8:
            ExitState = "OE0075_INVALID_ATTR_ADDR_ST1";

            var field8 = GetField(export.PrivateAttorneyAddress, "street1");

            field8.Error = true;

            break;
          case 9:
            ExitState = "OE0076_INVALID_ATTR_CITY";

            var field9 = GetField(export.PrivateAttorneyAddress, "city");

            field9.Error = true;

            break;
          case 10:
            var field10 = GetField(export.PrivateAttorneyAddress, "state");

            field10.Error = true;

            if (IsEmpty(export.PrivateAttorneyAddress.State))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
            else
            {
              ExitState = "OE0077_INVALID_ATTR_ADDR_STATE";
            }

            break;
          case 11:
            var field11 = GetField(export.PrivateAttorneyAddress, "zipCode5");

            field11.Error = true;

            ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

            break;
          case 12:
            ExitState = "CASE_NF";

            var field12 = GetField(export.Case1, "number");

            field12.Error = true;

            break;
          case 13:
            ExitState = "OE0136_ATTY_OR_FIRM_NAME_REQD";

            var field13 = GetField(export.PersonPrivateAttorney, "firstName");

            field13.Error = true;

            var field14 = GetField(export.PersonPrivateAttorney, "lastName");

            field14.Error = true;

            var field15 = GetField(export.PersonPrivateAttorney, "firmName");

            field15.Error = true;

            break;
          case 14:
            ExitState = "OE0137_CURR_ATTY_NOT_DISMISSED";

            var field16 = GetField(export.CsePerson, "number");

            field16.Error = true;

            break;
          case 15:
            ExitState = "OE0110_ZIP_CODE_REQD";

            var field17 = GetField(export.PrivateAttorneyAddress, "zipCode5");

            field17.Error = true;

            break;
          case 16:
            ExitState = "OE0000_ONLY_CURR_ATTY_B_CHGD_DEL";

            var field18 = GetField(export.PersonPrivateAttorney, "identifier");

            field18.Color = "red";
            field18.Intensity = Intensity.High;
            field18.Highlighting = Highlighting.ReverseVideo;
            field18.Protected = true;

            var field19 = GetField(export.Case1, "number");

            field19.Error = true;

            var field20 = GetField(export.CsePerson, "number");

            field20.Error = true;

            break;
          case 17:
            ExitState = "OE0000_ATTY_RETND_DT_GT_CURR_DT";

            var field21 =
              GetField(export.PersonPrivateAttorney, "dateRetained");

            field21.Error = true;

            break;
          case 18:
            ExitState = "OE0000_PHONE_AREA_REQD";

            var field22 =
              GetField(export.PersonPrivateAttorney, "phoneAreaCode");

            field22.Error = true;

            break;
          case 19:
            ExitState = "OE0000_PHONE_NBR_REQD";

            var field23 = GetField(export.PersonPrivateAttorney, "phone");

            field23.Error = true;

            break;
          case 20:
            ExitState = "OE0000_FAX_AREA_CODE_REQD";

            var field24 =
              GetField(export.PersonPrivateAttorney, "faxNumberAreaCode");

            field24.Error = true;

            break;
          case 21:
            ExitState = "OE0000_FAX_NO_REQD";

            var field25 = GetField(export.PersonPrivateAttorney, "faxNumber");

            field25.Error = true;

            break;
          case 22:
            ExitState = "OE0000_NO_ATTY_FOR_LEGAL_ACTION";

            var field26 =
              GetField(export.PersonPrivateAttorney, "courtCaseNumber");

            field26.Error = true;

            var field27 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field27.Error = true;

            var field28 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field28.Error = true;

            var field29 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field29.Error = true;

            break;
          case 23:
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            var field30 =
              GetField(export.PersonPrivateAttorney, "courtCaseNumber");

            field30.Error = true;

            break;
          case 24:
            ExitState = "OE0000_CT_ST_AND_CNTRY_NOT_ALLOW";

            var field31 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field31.Error = true;

            var field32 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field32.Error = true;

            var field33 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field33.Error = true;

            break;
          case 25:
            ExitState = "OE0000_CT_ST_OR_CNTRY_REQUIRED";

            var field34 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field34.Error = true;

            var field35 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field35.Error = true;

            var field36 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field36.Error = true;

            break;
          case 26:
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            var field37 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field37.Error = true;

            break;
          case 27:
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            var field38 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field38.Error = true;

            break;
          case 28:
            ExitState = "INVALID_STATE_ABBREVIATION";

            var field39 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field39.Error = true;

            break;
          case 29:
            ExitState = "INVALID_COUNTY_ABBREVIATION";

            var field40 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field40.Error = true;

            var field41 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field41.Error = true;

            break;
          case 30:
            ExitState = "OE0000_INVALID_COUNTRY_CODE";

            var field42 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field42.Error = true;

            break;
          case 31:
            ExitState = "OE0000_CRT_CASE_NOT_REL_CSE_PERS";

            var field43 =
              GetField(export.PersonPrivateAttorney, "courtCaseNumber");

            field43.Error = true;

            var field44 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field44.Error = true;

            var field45 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field45.Error = true;

            var field46 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field46.Error = true;

            break;
          case 32:
            ExitState = "OE0000_OTHR_ATTY_ASSND_CRT_CASE";

            var field47 =
              GetField(export.PersonPrivateAttorney, "courtCaseNumber");

            field47.Error = true;

            var field48 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field48.Error = true;

            var field49 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field49.Error = true;

            var field50 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field50.Error = true;

            break;
          case 33:
            ExitState = "OE0000_CANT_CHG_COURT_CASE_INFO";

            var field51 =
              GetField(export.PersonPrivateAttorney, "courtCaseNumber");

            field51.Error = true;

            var field52 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field52.Error = true;

            var field53 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field53.Error = true;

            var field54 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field54.Error = true;

            break;
          case 34:
            ExitState = "OE0000_CANT_ENTER_ID_FOR_ADD";

            var field55 = GetField(export.PersonPrivateAttorney, "identifier");

            field55.Error = true;

            break;
          case 35:
            ExitState = "OE0000_COURT_CASE_DISMISSED";

            var field56 =
              GetField(export.PersonPrivateAttorney, "courtCaseNumber");

            field56.Error = true;

            var field57 =
              GetField(export.PersonPrivateAttorney, "fipsCountyAbbreviation");

            field57.Error = true;

            var field58 =
              GetField(export.PersonPrivateAttorney, "fipsStateAbbreviation");

            field58.Error = true;

            var field59 = GetField(export.PersonPrivateAttorney, "tribCountry");

            field59.Error = true;

            break;
          case 36:
            ExitState = "OE0000_WITHDR_DT_CANT_GT_CURR_DT";

            var field60 =
              GetField(export.PersonPrivateAttorney, "dateDismissed");

            field60.Error = true;

            break;
          case 37:
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            if (IsEmpty(export.PrivateAttorneyAddress.City))
            {
              var field = GetField(export.PrivateAttorneyAddress, "city");

              field.Error = true;
            }

            if (IsEmpty(export.PrivateAttorneyAddress.State))
            {
              var field = GetField(export.PrivateAttorneyAddress, "state");

              field.Error = true;
            }

            // 201317 - 03/08/04 - Andrew Convery.
            break;
          case 38:
            var field61 = GetField(export.PrivateAttorneyAddress, "zipCode5");

            field61.Error = true;

            ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

            break;
          case 39:
            var field62 = GetField(export.PrivateAttorneyAddress, "zipCode5");

            field62.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            break;
          case 40:
            var field63 = GetField(export.PrivateAttorneyAddress, "zipCode5");

            field63.Error = true;

            ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

            break;
          case 41:
            var field64 = GetField(export.PrivateAttorneyAddress, "zipCode4");

            field64.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            break;
          case 42:
            var field65 = GetField(export.PrivateAttorneyAddress, "zipCode4");

            field65.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            break;
          case 43:
            break;
          case 44:
            var field66 =
              GetField(export.PersonPrivateAttorney, "courtCaseNumber");

            field66.Error = true;

            ExitState = "LE0000_CANT_DELETE_LEGAL_ACTION";

            break;
          case 45:
            var field67 =
              GetField(export.PersonPrivateAttorney, "emailAddress");

            field67.Error = true;

            ExitState = "INVALID_EMAIL_ADDRESS";

            break;
          case 46:
            var field68 =
              GetField(export.PersonPrivateAttorney, "consentIndicator");

            field68.Error = true;

            ExitState = "ACO_NI0000_ENTER_Y_OR_N";

            break;
          default:
            ExitState = "OE0004_UNKNOWN_ERROR_CODE";

            break;
        }

        --local.ErrorCodes.Index;
        local.ErrorCodes.CheckSize();
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveErrorCodes(OeAttyValidatePrivAttorney.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MovePersonPrivateAttorney1(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.FaxNumberAreaCode = source.FaxNumberAreaCode;
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.FaxExt = source.FaxExt;
    target.PhoneExt = source.PhoneExt;
    target.Identifier = source.Identifier;
    target.DateRetained = source.DateRetained;
    target.DateDismissed = source.DateDismissed;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FirmName = source.FirmName;
    target.Phone = source.Phone;
    target.FaxNumber = source.FaxNumber;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.EmailAddress = source.EmailAddress;
    target.BarNumber = source.BarNumber;
    target.ConsentIndicator = source.ConsentIndicator;
    target.Note = source.Note;
  }

  private static void MovePersonPrivateAttorney2(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.Identifier = source.Identifier;
    target.DateRetained = source.DateRetained;
    target.DateDismissed = source.DateDismissed;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FirmName = source.FirmName;
    target.Phone = source.Phone;
    target.FaxNumber = source.FaxNumber;
    target.EmailAddress = source.EmailAddress;
  }

  private static void MovePersonPrivateAttorney3(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.Identifier = source.Identifier;
    target.DateRetained = source.DateRetained;
    target.DateDismissed = source.DateDismissed;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MovePersonPrivateAttorney4(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.FipsStateAbbreviation = source.FipsStateAbbreviation;
    target.FipsCountyAbbreviation = source.FipsCountyAbbreviation;
    target.TribCountry = source.TribCountry;
  }

  private static void MovePersonPrivateAttorney5(PersonPrivateAttorney source,
    PersonPrivateAttorney target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.InitZeroSetToMax.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.InitZeroSetToMax.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
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

  private void UseOeAttyCreatePrivAttorney()
  {
    var useImport = new OeAttyCreatePrivAttorney.Import();
    var useExport = new OeAttyCreatePrivAttorney.Export();

    useImport.PrivateAttorneyAddress.Assign(export.PrivateAttorneyAddress);
    useImport.PersonPrivateAttorney.Assign(export.PersonPrivateAttorney);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeAttyCreatePrivAttorney.Execute, useImport, useExport);

    MovePersonPrivateAttorney5(useExport.UpdateStamp, export.UpdateStamp);
    MovePersonPrivateAttorney3(useExport.PersonPrivateAttorney,
      export.PersonPrivateAttorney);
  }

  private void UseOeAttyDeletePrivAttorney()
  {
    var useImport = new OeAttyDeletePrivAttorney.Import();
    var useExport = new OeAttyDeletePrivAttorney.Export();

    MovePersonPrivateAttorney2(export.PersonPrivateAttorney,
      useImport.PersonPrivateAttorney);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeAttyDeletePrivAttorney.Execute, useImport, useExport);
  }

  private void UseOeAttyDisplayPrivAttorney()
  {
    var useImport = new OeAttyDisplayPrivAttorney.Import();
    var useExport = new OeAttyDisplayPrivAttorney.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    MovePersonPrivateAttorney4(export.PersonPrivateAttorney,
      useImport.PersonPrivateAttorney);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeAttyDisplayPrivAttorney.Execute, useImport, useExport);

    local.AttyDisplayed.Flag = useExport.AttyDisplayed.Flag;
    MovePersonPrivateAttorney5(useExport.UpdateStamp, export.UpdateStamp);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.PersonPrivateAttorney.Assign(useExport.PersonPrivateAttorney);
    export.PrivateAttorneyAddress.Assign(useExport.PrivateAttorneyAddress);
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.Case1.Number = useExport.Case1.Number;
    local.Dismissed.Flag = useExport.CourtCaseDismissed.Flag;
  }

  private void UseOeAttyUpdatePrivAttorney()
  {
    var useImport = new OeAttyUpdatePrivAttorney.Import();
    var useExport = new OeAttyUpdatePrivAttorney.Export();

    useImport.PrivateAttorneyAddress.Assign(export.PrivateAttorneyAddress);
    useImport.PersonPrivateAttorney.Assign(export.PersonPrivateAttorney);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeAttyUpdatePrivAttorney.Execute, useImport, useExport);

    MovePersonPrivateAttorney5(useExport.UpdateStamp, export.UpdateStamp);
    MovePersonPrivateAttorney3(useExport.PersonPrivateAttorney,
      export.PersonPrivateAttorney);
  }

  private void UseOeAttyValidatePrivAttorney()
  {
    var useImport = new OeAttyValidatePrivAttorney.Import();
    var useExport = new OeAttyValidatePrivAttorney.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.PrivateAttorneyAddress.Assign(export.PrivateAttorneyAddress);
    useImport.PersonPrivateAttorney.Assign(export.PersonPrivateAttorney);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeAttyValidatePrivAttorney.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.ApInactive.Flag = useExport.CaseRoleInactive.Flag;
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
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

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Case1.Number = import.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
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
    /// A value of HiddenConsent.
    /// </summary>
    [JsonPropertyName("hiddenConsent")]
    public PersonPrivateAttorney HiddenConsent
    {
      get => hiddenConsent ??= new();
      set => hiddenConsent = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of UpdateStamp.
    /// </summary>
    [JsonPropertyName("updateStamp")]
    public PersonPrivateAttorney UpdateStamp
    {
      get => updateStamp ??= new();
      set => updateStamp = value;
    }

    /// <summary>
    /// A value of ListStates.
    /// </summary>
    [JsonPropertyName("listStates")]
    public Standard ListStates
    {
      get => listStates ??= new();
      set => listStates = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of HiddenPrevPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("hiddenPrevPersonPrivateAttorney")]
    public PersonPrivateAttorney HiddenPrevPersonPrivateAttorney
    {
      get => hiddenPrevPersonPrivateAttorney ??= new();
      set => hiddenPrevPersonPrivateAttorney = value;
    }

    /// <summary>
    /// A value of HiddenPrevCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevCase")]
    public Case1 HiddenPrevCase
    {
      get => hiddenPrevCase ??= new();
      set => hiddenPrevCase = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
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
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
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

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ApInactive.
    /// </summary>
    [JsonPropertyName("apInactive")]
    public Common ApInactive
    {
      get => apInactive ??= new();
      set => apInactive = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of DismissedLiteral.
    /// </summary>
    [JsonPropertyName("dismissedLiteral")]
    public TextWorkArea DismissedLiteral
    {
      get => dismissedLiteral ??= new();
      set => dismissedLiteral = value;
    }

    private PersonPrivateAttorney hiddenConsent;
    private WorkArea headerLine;
    private PersonPrivateAttorney updateStamp;
    private Standard listStates;
    private Standard listCsePersons;
    private CodeValue selected;
    private Common hiddenDisplayPerformed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ScrollingAttributes scrollingAttributes;
    private PersonPrivateAttorney hiddenPrevPersonPrivateAttorney;
    private Case1 hiddenPrevCase;
    private CsePerson hiddenPrevCsePerson;
    private Common hiddenPrevUserAction;
    private CodeValue country;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private CsePerson csePerson;
    private Case1 case1;
    private PersonPrivateAttorney personPrivateAttorney;
    private Standard nexttran;
    private NextTranInfo hidden;
    private Standard standard;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common apInactive;
    private Common caseOpen;
    private TextWorkArea dismissedLiteral;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HiddenConsent.
    /// </summary>
    [JsonPropertyName("hiddenConsent")]
    public PersonPrivateAttorney HiddenConsent
    {
      get => hiddenConsent ??= new();
      set => hiddenConsent = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of UpdateStamp.
    /// </summary>
    [JsonPropertyName("updateStamp")]
    public PersonPrivateAttorney UpdateStamp
    {
      get => updateStamp ??= new();
      set => updateStamp = value;
    }

    /// <summary>
    /// A value of ListStates.
    /// </summary>
    [JsonPropertyName("listStates")]
    public Standard ListStates
    {
      get => listStates ??= new();
      set => listStates = value;
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
    /// A value of ListCode.
    /// </summary>
    [JsonPropertyName("listCode")]
    public Code ListCode
    {
      get => listCode ??= new();
      set => listCode = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of HiddenPrevPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("hiddenPrevPersonPrivateAttorney")]
    public PersonPrivateAttorney HiddenPrevPersonPrivateAttorney
    {
      get => hiddenPrevPersonPrivateAttorney ??= new();
      set => hiddenPrevPersonPrivateAttorney = value;
    }

    /// <summary>
    /// A value of HiddenPrevCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevCase")]
    public Case1 HiddenPrevCase
    {
      get => hiddenPrevCase ??= new();
      set => hiddenPrevCase = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
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
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
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

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ApInactive.
    /// </summary>
    [JsonPropertyName("apInactive")]
    public Common ApInactive
    {
      get => apInactive ??= new();
      set => apInactive = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of DismissedLiteral.
    /// </summary>
    [JsonPropertyName("dismissedLiteral")]
    public TextWorkArea DismissedLiteral
    {
      get => dismissedLiteral ??= new();
      set => dismissedLiteral = value;
    }

    private PersonPrivateAttorney hiddenConsent;
    private WorkArea headerLine;
    private PersonPrivateAttorney updateStamp;
    private Standard listStates;
    private Standard listCsePersons;
    private Code listCode;
    private Common hiddenDisplayPerformed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ScrollingAttributes scrollingAttributes;
    private PersonPrivateAttorney hiddenPrevPersonPrivateAttorney;
    private Case1 hiddenPrevCase;
    private CsePerson hiddenPrevCsePerson;
    private Common hiddenPrevUserAction;
    private CodeValue country;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private CsePerson csePerson;
    private Case1 case1;
    private PersonPrivateAttorney personPrivateAttorney;
    private Standard nexttran;
    private NextTranInfo hidden;
    private Standard standard;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common apInactive;
    private Common caseOpen;
    private Common ap;
    private TextWorkArea dismissedLiteral;
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
    /// A value of AttyDisplayed.
    /// </summary>
    [JsonPropertyName("attyDisplayed")]
    public Common AttyDisplayed
    {
      get => attyDisplayed ??= new();
      set => attyDisplayed = value;
    }

    /// <summary>
    /// A value of TransactionFailed.
    /// </summary>
    [JsonPropertyName("transactionFailed")]
    public Common TransactionFailed
    {
      get => transactionFailed ??= new();
      set => transactionFailed = value;
    }

    /// <summary>
    /// A value of InitZeroSetToMax.
    /// </summary>
    [JsonPropertyName("initZeroSetToMax")]
    public DateWorkArea InitZeroSetToMax
    {
      get => initZeroSetToMax ??= new();
      set => initZeroSetToMax = value;
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
    /// A value of BlankPrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("blankPrivateAttorneyAddress")]
    public PrivateAttorneyAddress BlankPrivateAttorneyAddress
    {
      get => blankPrivateAttorneyAddress ??= new();
      set => blankPrivateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of BlankPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("blankPersonPrivateAttorney")]
    public PersonPrivateAttorney BlankPersonPrivateAttorney
    {
      get => blankPersonPrivateAttorney ??= new();
      set => blankPersonPrivateAttorney = value;
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
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
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
    /// A value of AttorneyRecFound.
    /// </summary>
    [JsonPropertyName("attorneyRecFound")]
    public Common AttorneyRecFound
    {
      get => attorneyRecFound ??= new();
      set => attorneyRecFound = value;
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

    /// <summary>
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
    }

    /// <summary>
    /// A value of Dismissed.
    /// </summary>
    [JsonPropertyName("dismissed")]
    public Common Dismissed
    {
      get => dismissed ??= new();
      set => dismissed = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common attyDisplayed;
    private Common transactionFailed;
    private DateWorkArea initZeroSetToMax;
    private DateWorkArea dateWorkArea;
    private PrivateAttorneyAddress blankPrivateAttorneyAddress;
    private PersonPrivateAttorney blankPersonPrivateAttorney;
    private Common lastErrorEntryNo;
    private Common highlightError;
    private Common userAction;
    private Common attorneyRecFound;
    private Array<ErrorCodesGroup> errorCodes;
    private TextWorkArea textWorkArea;
    private Common workError;
    private Common dismissed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("existingPrivateAttorneyAddress")]
    public PrivateAttorneyAddress ExistingPrivateAttorneyAddress
    {
      get => existingPrivateAttorneyAddress ??= new();
      set => existingPrivateAttorneyAddress = value;
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
    /// A value of ExistingPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("existingPersonPrivateAttorney")]
    public PersonPrivateAttorney ExistingPersonPrivateAttorney
    {
      get => existingPersonPrivateAttorney ??= new();
      set => existingPersonPrivateAttorney = value;
    }

    private PrivateAttorneyAddress existingPrivateAttorneyAddress;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
    private PersonPrivateAttorney existingPersonPrivateAttorney;
  }
#endregion
}

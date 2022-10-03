// Program: OE_PCON_PERSON_CONTACT, ID: 371891480, model: 746.
// Short name: SWEPCONP
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
/// A program: OE_PCON_PERSON_CONTACT.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure facilitates maintenance of Contact details of a CSE Person.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OePconPersonContact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PCON_PERSON_CONTACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePconPersonContact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePconPersonContact.
  /// </summary>
  public OePconPersonContact(IContext context, Import import, Export export):
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
    // This procedure step facilitates maintenance of Contact details.
    // PROCESSING:
    // To create a new set of contact details (CONTACT, CONTACT_ADDRESS and 
    // CONTACT_DETAIL) the user will enter the contact details and press CREATE
    // key. The system will validate the details and update the database.
    // To update existing contact details, the user will first display the 
    // required contact details and will modify the details and press UPDATE
    // key. The system will validate the details and then update the database.
    // User can go to LIST CSE PERSON CONTACT to list contacts and select a 
    // contact.
    // When the effective date of the contact address is changed, the system 
    // will create a new CONTACT_ADDRESS record.
    // This procedure step also allows to Add/Update CONTACT_DETAIL records and 
    // provides scrolling.
    // ACTION BLOCKS:
    // The following action blocks are called from this procedure step:
    // 	OI_MCONT_CREATE_CONTACT_DETAILS
    // 	OI_MCONT_UPDATE_CONTACT_DETAILS
    // 	OI_MCONT_DELETE_CONTACT_DETAILS
    // 	OI_MCONT_DISPLAY_CONTACT_DETAILS
    // 	OI_MCONT_VALIDATE_CONTACT_DETAIL
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			C R U D
    // 	CONTACT_ADDRESS		C R U D
    // 	CONTACT_DETAIL		C R U D
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	01/26/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE		DESCRIPTION
    // Govind		01/26/95	Initial coding
    // DPV		12/14/95	Added call to task engine
    // 				w/triggering event
    // Sherri Newman	2/7/96		Retrofit
    // Lofton		02/20/96	Retrofit
    // Siraj Konkader	14 June 1996	Removed task engine calls.
    // 				Added print function.
    // Regan Welborn	26 June 1996	Added call to EAB for left-padding
    // 				0's in CSE PERSON Number and CASE
    // 				Number.
    // R. Marchman	11/13/1996	Add new security and next tran.
    // M Ramirez	12/15/1998	Revised print process
    // M Ramirez	12/15/1998	Changed security to check on CRUD
    // 				actions only.
    // 
    // S Johnson       02/17/1999      Changed the data flow mapping to
    // 				COMP from import_work
    // 				cse_persons_work_set view to
    // 				import_selected
    // 				cse_persons_work_set (what code was
    // 				expecting)
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process.
    // 04/05/2000      W.Campbell      Disabled existing call to
    //                                 
    // Security Cab and added a
    //                                 
    // new call with view matching
    //                                 
    // changed to match the export
    //                                 
    // views of case and cse_person.
    //                                 
    // Work done on WR#000162
    //                                 
    // for PRWORA - Family Violence.
    // 05/15/2001      Madhu Kumar     PR #116889 Edit check  for zip codes.
    // 01/31/2003      Ed Lyman        WR20311 Health Insurance.
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpDocSetLiterals();

    // =============================================
    // The following two MOVE statements are
    // specifically moved to this place for this
    // screen (PCON) since we dont want to clear
    // person number
    // =============================================
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.Work.Number = import.Selected.Number;

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    if (!IsEmpty(import.Case1.Number))
    {
      local.TextWorkArea.Text10 = import.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(import.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = import.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
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
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      UseScCabNextTranPut1();

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
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberAp))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumberAp ?? Spaces(10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberObligor))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumberObligor ?? Spaces
          (10);
      }

      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    local.CurrentUserAction.Command = global.Command;
    export.ListStateCodeStandard.PromptField =
      import.ListStateCodeStandard.PromptField;
    export.ListPersonNo.PromptField = import.ListPersonNo.PromptField;

    // --------------------------------------------------------------
    // Beginning Of Change
    // 4.16.100 TC # 12
    // New 'if' statement added.
    // -------------------------------------------------------------
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL") && !
      Equal(global.Command, "RETNAME") && !Equal(global.Command, "RETCOMP"))
    {
      export.ListStateCodeStandard.PromptField = "";
      export.ListPersonNo.PromptField = "";
    }

    // --------------------------------------------------------------
    // End Of Change
    // 4.16.100 TC # 12
    // -------------------------------------------------------------
    export.Contact.Assign(import.Contact);
    export.ContactAddress.Assign(import.ContactAddress);
    export.Country.Description = import.Country.Description;
    export.HiddenPrevCsePerson.Number = import.HiddenPrevCsePerson.Number;
    MoveContact3(import.HiddenPrevContact, export.HiddenPrevContact);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.HrespForHealthIns.Flag = import.HrespForHealthIns.Flag;

    // *****June 10 1996 ASK: Added line below
    MoveContact4(import.LastUpdated, export.LastUpdated);

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        MoveContactDetail(import.Import1.Item.DetailContactDetail,
          export.Export1.Update.DetailContactDetail);
        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      // ----------------------------------------------------------
      // Beginning Of Change
      // 4.16.100 TC # 12
      // ----------------------------------------------------------
      if (AsChar(export.ListStateCodeStandard.PromptField) == 'S')
      {
        var field = GetField(export.ListStateCodeStandard, "promptField");

        field.Color = "red";
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = false;
      }

      // ----------------------------------------------------------
      // End Of Change
      // 4.16.100 TC # 12
      // ----------------------------------------------------------
      // ---------------------------------------------------------
      // Beginning Of Change
      // 4.16.100 TC # 15
      // --------------------------------------------------------
      if (!IsEmpty(import.Selected.Number))
      {
        export.CsePerson.Number = import.Selected.Number;
      }

      export.ListPersonNo.PromptField = "";

      // ---------------------------------------------------------
      // End Of Change
      // 4.16.100 TC # 15
      // --------------------------------------------------------
      global.Command = "DISPLAY";
    }

    // **** begin group C ****
    // to validate action level security
    // mjr---> Changed security to check on CRUD actions only.
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // **** end   group C ****
    local.CurrentUserAction.Command = global.Command;
    local.HighlightValidationError.Flag = "N";

    switch(TrimEnd(global.Command))
    {
      case "RETPCOL":
        // ----------------------------------------------------------
        // Beginning Of Change
        // 4.16.100 TC # 27
        // ----------------------------------------------------------
        local.CurrentUserAction.Command = "DISPLAY";
        global.Command = "DISPLAY";

        // ----------------------------------------------------------
        // End Of Change
        // 4.16.100 TC # 27
        // ----------------------------------------------------------
        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "LIST":
        if (AsChar(export.HrespForHealthIns.Flag) == 'Y')
        {
          var field = GetField(export.Contact, "relationshipToCsePerson");

          field.Color = "cyan";
          field.Protected = true;
        }

        // ----------------------------------------------------------
        // Beginning Of Change
        // 4.16.100 TC # 13
        // ----------------------------------------------------------
        if (!IsEmpty(export.ListPersonNo.PromptField) && AsChar
          (export.ListPersonNo.PromptField) != 'S' && !
          IsEmpty(export.ListStateCodeStandard.PromptField) && AsChar
          (export.ListStateCodeStandard.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field3 = GetField(export.ListPersonNo, "promptField");

          field3.Error = true;

          var field4 = GetField(export.ListStateCodeStandard, "promptField");

          field4.Color = "red";
          field4.Highlighting = Highlighting.ReverseVideo;
          field4.Protected = false;

          return;
        }

        // ----------------------------------------------------------
        // End Of Change
        // 4.16.100 TC # 13
        // ----------------------------------------------------------
        if (!IsEmpty(export.ListPersonNo.PromptField) && AsChar
          (export.ListPersonNo.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListPersonNo, "promptField");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.ListStateCodeStandard.PromptField) && AsChar
          (export.ListStateCodeStandard.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListStateCodeStandard, "promptField");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListPersonNo.PromptField) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          return;
        }

        if (AsChar(export.ListStateCodeStandard.PromptField) == 'S')
        {
          export.ListStateCodeCode.CodeName = "STATE CODE";
          export.StartingStateCode.Cdvalue = export.ContactAddress.State ?? Spaces
            (10);
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        // ----------------------------------------------------------
        // Beginning Of Change
        // 4.16.100 TC # 11
        // ----------------------------------------------------------
        var field1 = GetField(export.ListPersonNo, "promptField");

        field1.Error = true;

        var field2 = GetField(export.ListStateCodeStandard, "promptField");

        field2.Color = "red";
        field2.Highlighting = Highlighting.ReverseVideo;
        field2.Protected = false;

        // ----------------------------------------------------------
        // End Of Change
        // 4.16.100 TC # 11
        // ----------------------------------------------------------
        return;
      case "RETCDVL":
        global.Command = "DISPLAY";
        local.CurrentUserAction.Command = "DISPLAY";
        export.ListStateCodeStandard.PromptField = "";

        if (IsEmpty(import.SelectedState.Cdvalue))
        {
          var field = GetField(export.ContactAddress, "state");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          local.ReturnRetcdvl.Flag = "Y";
          export.ContactAddress.State = import.SelectedState.Cdvalue;

          var field = GetField(export.ContactAddress, "zipCode5");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "PCOL":
        ExitState = "ECO_XFR_TO_PCOL_CONTACT_LIST";

        return;
      case "RTSELCNT":
        if (import.Contact.ContactNumber == 0)
        {
          // ---------------------------------------------
          // User pressed CANCEL in List Contact Screen
          // ---------------------------------------------
          export.Contact.CompanyName = "";
          export.Contact.FaxAreaCode = 0;
          export.Contact.Fax = 0;
          export.Contact.HomePhoneAreaCode = 0;
          export.Contact.HomePhone = 0;
          export.Contact.WorkPhoneAreaCode = 0;
          export.Contact.WorkPhone = 0;
          export.Contact.MiddleInitial = "";
          export.Contact.NameFirst = "";
          export.Contact.NameLast = "";
          export.Contact.NameTitle = "";
          export.Contact.RelationshipToCsePerson = "";
          export.Contact.WorkPhone = 0;
          export.ContactAddress.City = "";
          export.ContactAddress.State = "";
          export.ContactAddress.Street1 = "";
          export.ContactAddress.Street2 = "";
          export.ContactAddress.Zip3 = "";
          export.ContactAddress.ZipCode4 = "";
          export.ContactAddress.ZipCode5 = "";

          export.Export1.Index = 0;
          export.Export1.Clear();

          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            export.Export1.Next();

            break;

            export.Export1.Next();
          }

          export.HiddenPrevUserAction.Command = "CANCEL";

          return;
        }

        // ---------------------------------------------
        // Display the details of Contact selected
        // ---------------------------------------------
        local.CurrentUserAction.Command = "DISPLAY";
        UseOePconDisplayContactDetails();

        if (AsChar(export.HrespForHealthIns.Flag) == 'Y')
        {
          var field = GetField(export.Contact, "relationshipToCsePerson");

          field.Color = "cyan";
          field.Protected = true;
        }

        if (AsChar(local.ContactDisplayed.Flag) == 'Y')
        {
          export.HiddenPrevCsePerson.Number = export.CsePerson.Number;
          MoveContact3(export.Contact, export.HiddenPrevContact);

          // ---------------------------------------------
          // Set the command value to DISPLAY so that the last_user_action is 
          // set to DISPLAY. This will enable the user to do an update or delete
          // on this selected record.
          // ---------------------------------------------
          global.Command = "DISPLAY";
          export.HiddenDisplayPerformed.Flag = "Y";
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          export.HiddenDisplayPerformed.Flag = "N";
        }

        break;
      case "DISPLAY":
        if (!IsEmpty(export.Case1.Number))
        {
          UseOeCabCheckCaseMember();

          switch(AsChar(local.Work.Flag))
          {
            case 'C':
              var field3 = GetField(export.Case1, "number");

              field3.Error = true;

              ExitState = "CASE_NF";

              break;
            case 'P':
              var field4 = GetField(export.CsePerson, "number");

              field4.Error = true;

              export.CsePersonsWorkSet.FormattedName = "";
              ExitState = "CSE_PERSON_NF";

              break;
            case 'R':
              var field5 = GetField(export.Case1, "number");

              field5.Error = true;

              var field6 = GetField(export.CsePerson, "number");

              field6.Error = true;

              ExitState = "OE0000_CASE_MEMBER_NE";

              break;
            default:
              break;
          }

          if (!IsEmpty(local.Work.Flag))
          {
            export.Contact.Assign(local.InitializedContact);
            export.ContactAddress.Assign(local.InitializedContactAddress);

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              MoveContactDetail(local.InitializedContactDetail,
                export.Export1.Update.DetailContactDetail);
            }

            return;
          }
        }

        break;
      case "PREV":
        // ---------------------------------------------
        // Action already taken
        // ---------------------------------------------
        break;
      case "NEXT":
        // ---------------------------------------------
        // Action already taken
        // ---------------------------------------------
        break;
      case "CREATE":
        // --------------------------------------------------------
        // Beginning Of Change
        // 4.16.100 TC # 16
        // --------------------------------------------------------
        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NO_REQUIRED";

          return;
        }

        // --------------------------------------------------------
        // End Of Change
        // 4.16.100 TC # 16
        // --------------------------------------------------------
        export.HiddenDisplayPerformed.Flag = "N";
        UseOePconValidateContactDetail1();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // One or more errors encountered. Errors are highlighted outside the 
          // CASE statement.
          local.HighlightValidationError.Flag = "Y";

          break;
        }

        // Validations are successful. Create the CONTACT records.
        // The following exit state statement is required to clear any adabas 
        // error exit states set by adabas externals.
        ExitState = "ACO_NN0000_ALL_OK";
        UseOePconCreateContactDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // A fatal error encountered in database update.
          UseEabRollbackCics();

          break;
        }

        export.HiddenDisplayPerformed.Flag = "Y";
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        if (Equal(export.HiddenPrevUserAction.Command, "DISPLAY") || Equal
          (export.HiddenPrevUserAction.Command, "PREV") || Equal
          (export.HiddenPrevUserAction.Command, "NEXT") || Equal
          (export.HiddenPrevUserAction.Command, "CREATE") || Equal
          (export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          // ---------------------------------------------
          // It is okay if last action was DISPLAY or PREV or NEXT or UPDATE.
          // Otherwise it is an error.
          // ---------------------------------------------
        }
        else
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (!Equal(export.CsePerson.Number, export.HiddenPrevCsePerson.Number) ||
          export.Contact.ContactNumber != export
          .HiddenPrevContact.ContactNumber || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HrespForHealthIns.Flag) == 'Y')
        {
          if (!Equal(export.Contact.RelationshipToCsePerson,
            export.HiddenPrevContact.RelationshipToCsePerson))
          {
            ExitState = "OE0203_UPDATE_NOT_ALLOWED";
            export.Contact.RelationshipToCsePerson =
              export.HiddenPrevContact.RelationshipToCsePerson ?? "";

            var field = GetField(export.Contact, "relationshipToCsePerson");

            field.Color = "cyan";
            field.Protected = true;

            return;
          }
        }

        UseOePconValidateContactDetail1();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // One or more errors encountered. Errors are highlighted outside the 
          // CASE statement.
          local.HighlightValidationError.Flag = "Y";

          break;
        }

        // Validations are successful. Update the CONTACT records.
        // The following exit state statement is required to clear any adabas 
        // error exit states set by adabas externals.
        ExitState = "ACO_NN0000_ALL_OK";
        UseOePconUpdateContactDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // A fatal error encountered in database update.
          UseEabRollbackCics();

          break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        if (AsChar(export.HrespForHealthIns.Flag) == 'Y')
        {
          var field = GetField(export.Contact, "relationshipToCsePerson");

          field.Color = "cyan";
          field.Protected = true;
        }

        break;
      case "DELETE":
        if (Equal(export.HiddenPrevUserAction.Command, "DISPLAY") || Equal
          (export.HiddenPrevUserAction.Command, "PREV") || Equal
          (export.HiddenPrevUserAction.Command, "NEXT"))
        {
          // ---------------------------------------------
          // It is okay if last action was DISPLAY or PREV or NEXT.
          // Otherwise it is an error.
          // ---------------------------------------------
        }
        else
        {
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          break;
        }

        if (!Equal(export.CsePerson.Number, export.HiddenPrevCsePerson.Number) ||
          export.Contact.ContactNumber != export
          .HiddenPrevContact.ContactNumber || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y')
        {
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          break;
        }

        export.HiddenDisplayPerformed.Flag = "N";
        UseOePconValidateContactDetail2();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // One or more errors encountered. Errors are highlighted outside the 
          // CASE statement.
          local.HighlightValidationError.Flag = "Y";

          break;
        }

        // The following exit state statement is required to clear any adabas 
        // error exit states set by adabas externals.
        ExitState = "ACO_NN0000_ALL_OK";
        UseOePconDeleteContactDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // A fatal error encountered in database update.
          UseEabRollbackCics();

          break;
        }

        export.Contact.ContactNumber = 0;
        export.Contact.CompanyName = "";
        export.Contact.FaxAreaCode = 0;
        export.Contact.Fax = 0;
        export.Contact.HomePhoneAreaCode = 0;
        export.Contact.HomePhone = 0;
        export.Contact.WorkPhoneAreaCode = 0;
        export.Contact.WorkPhone = 0;
        export.Contact.MiddleInitial = "";
        export.Contact.NameFirst = "";
        export.Contact.NameLast = "";
        export.Contact.NameTitle = "";
        export.Contact.RelationshipToCsePerson = "";
        export.Contact.WorkPhone = 0;
        export.ContactAddress.City = "";
        export.ContactAddress.State = "";
        export.ContactAddress.Street1 = "";
        export.ContactAddress.Street2 = "";
        export.ContactAddress.Zip3 = "";
        export.ContactAddress.ZipCode4 = "";
        export.ContactAddress.ZipCode5 = "";

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Next();

          break;

          export.Export1.Next();
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "PRINT":
        if (!Equal(export.CsePerson.Number, export.HiddenPrevCsePerson.Number) ||
          export.Contact.ContactNumber != export
          .HiddenPrevContact.ContactNumber || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y' || IsEmpty
          (export.CsePerson.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        local.Document.Name = "GNRLINFO";

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before invoking print process
        // -------------------------------------------------------
        export.Hidden.Assign(local.Null1);
        export.Standard.NextTransaction = "DKEY";
        export.Hidden.MiscText2 = TrimEnd(local.SpDocLiteral.IdDocument) + local
          .Document.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.Hidden.MiscText1 = TrimEnd(local.SpDocLiteral.IdPrNumber) + export
          .CsePerson.Number;
        export.Hidden.CaseNumber = export.Case1.Number;
        local.WorkArea.Text50 = TrimEnd(local.SpDocLiteral.IdContact) + NumberToString
          (export.Contact.ContactNumber, 14, 2);
        export.Hidden.MiscText1 = TrimEnd(export.Hidden.MiscText1) + ";" + local
          .WorkArea.Text50;
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
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPrNumber));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.CsePerson.Number =
          Substring(export.Hidden.MiscText1, local.Position.Count + 7, 10);
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdContact));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchConvertNumToText.Number15 =
          StringToNumber(Substring(
            export.Hidden.MiscText1, 50, local.Position.Count + 8, 2));
        export.Contact.ContactNumber =
          (int)local.BatchConvertNumToText.Number15;
        global.Command = "DISPLAY";
        local.CurrentUserAction.Command = global.Command;

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // mjr
    // ------------------------------------------------
    // 12/15/1998
    // Pulled case of Display out of the main case of command, so
    // that it can execute after command PrintRet.
    // -------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      // ---------------------------------------------
      // On DISPLAY key, Display the details of Contact required
      // On PREV key, display the details of previous Contact.
      // On NEXT key, display the details of next Contact.
      // ---------------------------------------------
      // --------------------------------------------------------
      // Beginning Of Change
      // 4.16.100 TC # 1
      // --------------------------------------------------------
      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NO_REQUIRED";

        return;
      }

      // --------------------------------------------------------
      // End Of Change
      // 4.16.100 TC # 1
      // --------------------------------------------------------
      if (!IsEmpty(export.HiddenPrevCsePerson.Number))
      {
        if (!Equal(export.CsePerson.Number, export.HiddenPrevCsePerson.Number))
        {
          export.Contact.ContactNumber = 0;
        }
      }

      UseOePconDisplayContactDetails();

      if (AsChar(export.HrespForHealthIns.Flag) == 'Y')
      {
        var field = GetField(export.Contact, "relationshipToCsePerson");

        field.Color = "cyan";
        field.Protected = true;
      }

      // --------------------------------------------------------
      // Beginning Of Change
      // 4.16.100 TC # 14
      // --------------------------------------------------------
      if (IsEmpty(export.ContactAddress.State))
      {
        if (AsChar(local.ReturnRetcdvl.Flag) == 'Y')
        {
          export.ContactAddress.State = import.SelectedState.Cdvalue;
          local.ReturnRetcdvl.Flag = "";
        }
      }
      else if (AsChar(local.ReturnRetcdvl.Flag) == 'Y' && !
        Equal(export.ContactAddress.State, import.SelectedState.Cdvalue))
      {
        export.ContactAddress.State = import.SelectedState.Cdvalue;
        local.ReturnRetcdvl.Flag = "";
      }

      // --------------------------------------------------------
      // End Of Change
      // 4.16.100 TC # 14
      // --------------------------------------------------------
      export.HiddenPrevCsePerson.Number = export.CsePerson.Number;
      MoveContact3(export.Contact, export.HiddenPrevContact);

      if (AsChar(local.ContactDisplayed.Flag) == 'Y')
      {
        export.HiddenDisplayPerformed.Flag = "Y";

        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText2, NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count <= 0)
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          // mjr---> Determines the appropriate exitstate for the Print process
          local.WorkArea.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
          UseSpPrintDecodeReturnCode();
          export.Hidden.MiscText2 = local.WorkArea.Text50;
        }
      }
      else
      {
        export.HiddenDisplayPerformed.Flag = "N";

        if (IsExitState("OE0098_NO_MORE_CONTACT"))
        {
          if (Equal(global.Command, "NEXT"))
          {
            export.Contact.ContactNumber = 99;
          }
          else
          {
            export.Contact.ContactNumber = 0;
          }

          export.Contact.CompanyName = "";
          export.Contact.FaxAreaCode = 0;
          export.Contact.Fax = 0;
          export.Contact.HomePhoneAreaCode = 0;
          export.Contact.HomePhone = 0;
          export.Contact.WorkPhoneAreaCode = 0;
          export.Contact.WorkPhone = 0;
          export.Contact.MiddleInitial = "";
          export.Contact.NameFirst = "";
          export.Contact.NameLast = "";
          export.Contact.NameTitle = "";
          export.Contact.RelationshipToCsePerson = "";
          export.Contact.WorkPhone = 0;
          export.ContactAddress.City = "";
          export.ContactAddress.State = "";
          export.ContactAddress.Street1 = "";
          export.ContactAddress.Street2 = "";
          export.ContactAddress.Zip3 = "";
          export.ContactAddress.ZipCode4 = "";
          export.ContactAddress.ZipCode5 = "";

          export.Export1.Index = 0;
          export.Export1.Clear();

          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            export.Export1.Next();

            break;

            export.Export1.Next();
          }
        }
        else
        {
          export.Contact.ContactNumber = 0;

          if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }
        }
      }
    }

    export.HiddenPrevCsePerson.Number = export.CsePerson.Number;
    MoveContact3(export.Contact, export.HiddenPrevContact);
    export.HiddenPrevUserAction.Command = local.CurrentUserAction.Command;

    if (AsChar(local.HighlightValidationError.Flag) == 'Y')
    {
      // Highlight the validation errors now.
      local.Errors.Index = local.Errors.Count - 1;
      local.Errors.CheckSize();

      while(local.Errors.Index >= 0)
      {
        switch(local.Errors.Item.DetailError.Count)
        {
          case 1:
            ExitState = "CSE_PERSON_NF";

            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;

            break;
          case 2:
            ExitState = "CONTACT_NF";

            var field2 = GetField(export.Contact, "contactNumber");

            field2.Error = true;

            break;
          case 3:
            ExitState = "OE0109_STREET_REQD";

            var field3 = GetField(export.ContactAddress, "street1");

            field3.Error = true;

            break;
          case 4:
            ExitState = "OE0108_CITY_REQD";

            var field4 = GetField(export.ContactAddress, "city");

            field4.Error = true;

            break;
          case 5:
            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            var field5 = GetField(export.ContactAddress, "state");

            field5.Error = true;

            break;
          case 6:
            ExitState = "OE0110_ZIP_CODE_REQD";

            var field6 = GetField(export.ContactAddress, "zipCode5");

            field6.Error = true;

            break;
          case 7:
            ExitState = "OE0112_COMPANY_OR_CONT_REQD";

            var field7 = GetField(export.Contact, "companyName");

            field7.Error = true;

            var field8 = GetField(export.Contact, "nameLast");

            field8.Error = true;

            break;
          case 8:
            ExitState = "OE0000_INCOMP_PH_AREA_CODE_REQD";

            var field9 = GetField(export.Contact, "homePhoneAreaCode");

            field9.Error = true;

            break;
          case 9:
            ExitState = "OE0000_INCOMP_PHONE_NBR_REQD";

            var field10 = GetField(export.Contact, "homePhone");

            field10.Error = true;

            break;
          case 10:
            ExitState = "OE0000_INCOMP_PH_AREA_CODE_REQD";

            var field11 = GetField(export.Contact, "workPhoneAreaCode");

            field11.Error = true;

            break;
          case 11:
            ExitState = "OE0000_INCOMP_PHONE_NBR_REQD";

            var field12 = GetField(export.Contact, "workPhone");

            field12.Error = true;

            break;
          case 12:
            ExitState = "OE0000_INCOMP_FAX_AREA_CODE_REQ";

            var field13 = GetField(export.Contact, "faxAreaCode");

            field13.Error = true;

            break;
          case 13:
            ExitState = "OE0000_INCOMP_FAX_FAX_NBR_REQD";

            var field14 = GetField(export.Contact, "fax");

            field14.Error = true;

            break;
          case 14:
            ExitState = "OE0000_INVALID_VERIFIED_DATE";

            var field15 = GetField(export.Contact, "verifiedDate");

            field15.Error = true;

            break;
          case 15:
            var field16 = GetField(export.ContactAddress, "zipCode5");

            field16.Error = true;

            ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

            break;
          case 16:
            ExitState = "OE0000_MARRIAGE_EXISTS_4_CONTACT";

            var field17 = GetField(export.Contact, "contactNumber");

            field17.Error = true;

            break;
          case 17:
            ExitState = "OE0000_HINS_EXISTS_FOR_CONTACT";

            var field18 = GetField(export.Contact, "contactNumber");

            field18.Error = true;

            var field19 = GetField(export.Contact, "relationshipToCsePerson");

            field19.Color = "cyan";
            field19.Protected = true;

            break;
          case 18:
            ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

            var field20 = GetField(export.ContactAddress, "zipCode5");

            field20.Error = true;

            break;
          case 19:
            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            var field21 = GetField(export.ContactAddress, "zipCode5");

            field21.Error = true;

            break;
          case 20:
            ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

            var field22 = GetField(export.ContactAddress, "zipCode5");

            field22.Error = true;

            break;
          case 21:
            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            var field23 = GetField(export.ContactAddress, "zipCode4");

            field23.Error = true;

            break;
          case 22:
            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            var field24 = GetField(export.ContactAddress, "zipCode4");

            field24.Error = true;

            break;
          default:
            break;
        }

        --local.Errors.Index;
        local.Errors.CheckSize();
      }
    }
  }

  private static void MoveContact1(Contact source, Contact target)
  {
    target.VerifiedDate = source.VerifiedDate;
    target.VerifiedUserId = source.VerifiedUserId;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.FaxAreaCode = source.FaxAreaCode;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.FaxExt = source.FaxExt;
    target.Fax = source.Fax;
    target.ContactNumber = source.ContactNumber;
    target.NameTitle = source.NameTitle;
    target.CompanyName = source.CompanyName;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
    target.HomePhone = source.HomePhone;
    target.WorkPhone = source.WorkPhone;
  }

  private static void MoveContact2(Contact source, Contact target)
  {
    target.Fax = source.Fax;
    target.ContactNumber = source.ContactNumber;
    target.NameTitle = source.NameTitle;
    target.CompanyName = source.CompanyName;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
    target.HomePhone = source.HomePhone;
    target.WorkPhone = source.WorkPhone;
  }

  private static void MoveContact3(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
  }

  private static void MoveContact4(Contact source, Contact target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveContactDetail(ContactDetail source,
    ContactDetail target)
  {
    target.Identifier = source.Identifier;
    target.Note = source.Note;
  }

  private static void MoveErrors(OePconValidateContactDetail.Export.
    ErrorsGroup source, Local.ErrorsGroup target)
  {
    target.DetailNoteEntry.Count = source.DetailNoteEntry.Count;
    target.DetailError.Count = source.DetailError.Count;
  }

  private static void MoveExport2(OePconCreateContactDetails.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.ActionEntry = source.DetailAction.ActionEntry;
    MoveContactDetail(source.Detail, target.DetailContactDetail);
  }

  private static void MoveExport3(OePconUpdateContactDetails.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.ActionEntry = source.DetailAction.ActionEntry;
    MoveContactDetail(source.Detail, target.DetailContactDetail);
  }

  private static void MoveExport4(OePconValidateContactDetail.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.ActionEntry = source.DetailAction.ActionEntry;
    MoveContactDetail(source.Detail, target.DetailContactDetail);
  }

  private static void MoveExport5(OePconDisplayContactDetails.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.ActionEntry = source.DetailAction.ActionEntry;
    MoveContactDetail(source.Detail, target.DetailContactDetail);
  }

  private static void MoveExport1ToImport2(Export.ExportGroup source,
    OePconCreateContactDetails.Import.ImportGroup target)
  {
    target.DetailAction.ActionEntry = source.DetailCommon.ActionEntry;
    MoveContactDetail(source.DetailContactDetail, target.Detail);
  }

  private static void MoveExport1ToImport3(Export.ExportGroup source,
    OePconUpdateContactDetails.Import.ImportGroup target)
  {
    target.DetailAction.ActionEntry = source.DetailCommon.ActionEntry;
    MoveContactDetail(source.DetailContactDetail, target.Detail);
  }

  private static void MoveImport1(Import.ImportGroup source,
    OePconValidateContactDetail.Import.ImportGroup target)
  {
    target.Action.ActionEntry = source.DetailCommon.ActionEntry;
    MoveContactDetail(source.DetailContactDetail, target.Detail);
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

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdContact = source.IdContact;
    target.IdDocument = source.IdDocument;
    target.IdPrNumber = source.IdPrNumber;
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
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Work.Flag = useExport.Work.Flag;
    export.Work.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOePconCreateContactDetails()
  {
    var useImport = new OePconCreateContactDetails.Import();
    var useExport = new OePconCreateContactDetails.Export();

    useImport.ContactAddress.Assign(export.ContactAddress);
    useImport.Contact.Assign(export.Contact);
    useImport.CsePerson.Number = export.CsePerson.Number;
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport2);

    Call(OePconCreateContactDetails.Execute, useImport, useExport);

    MoveContact4(useExport.LastUpdatedStamps, export.LastUpdated);
    export.ContactAddress.Assign(useExport.ContactAddress);
    export.Contact.Assign(useExport.Contact);
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
  }

  private void UseOePconDeleteContactDetails()
  {
    var useImport = new OePconDeleteContactDetails.Import();
    var useExport = new OePconDeleteContactDetails.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveContact2(export.Contact, useImport.Contact);

    Call(OePconDeleteContactDetails.Execute, useImport, useExport);
  }

  private void UseOePconDisplayContactDetails()
  {
    var useImport = new OePconDisplayContactDetails.Import();
    var useExport = new OePconDisplayContactDetails.Export();

    useImport.Contact.ContactNumber = export.Contact.ContactNumber;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.UserAction.Command = local.CurrentUserAction.Command;

    Call(OePconDisplayContactDetails.Execute, useImport, useExport);

    export.HrespForHealthIns.Flag = useExport.RespForHealthInsurance.Flag;
    local.ContactDisplayed.Flag = useExport.ContactDisplayed.Flag;
    MoveContact4(useExport.UpdatedStamps, export.LastUpdated);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    export.Work.Assign(useExport.WorkSet);
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.Contact.Assign(useExport.Contact);
    export.ContactAddress.Assign(useExport.ContactAddress);
    useExport.Export1.CopyTo(export.Export1, MoveExport5);
  }

  private void UseOePconUpdateContactDetails()
  {
    var useImport = new OePconUpdateContactDetails.Import();
    var useExport = new OePconUpdateContactDetails.Export();

    useImport.ContactAddress.Assign(export.ContactAddress);
    useImport.Contact.Assign(export.Contact);
    useImport.CsePerson.Number = export.CsePerson.Number;
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport3);

    Call(OePconUpdateContactDetails.Execute, useImport, useExport);

    MoveContact4(useExport.UpdatedStamp, export.LastUpdated);
    export.ContactAddress.Assign(useExport.ContactAddress);
    export.Contact.Assign(useExport.Contact);
    useExport.Export1.CopyTo(export.Export1, MoveExport3);
  }

  private void UseOePconValidateContactDetail1()
  {
    var useImport = new OePconValidateContactDetail.Import();
    var useExport = new OePconValidateContactDetail.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.UserAction.Command = local.CurrentUserAction.Command;
    useImport.Contact.Assign(export.Contact);
    useImport.ContactAddress.Assign(export.ContactAddress);
    import.Import1.CopyTo(useImport.Import1, MoveImport1);

    Call(OePconValidateContactDetail.Execute, useImport, useExport);

    export.Work.FormattedName = useExport.WorkSet.FormattedName;
    export.CsePerson.Number = useExport.CsePerson.Number;
    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    MoveContact1(useExport.Contact, export.Contact);
    export.ContactAddress.Assign(useExport.ContactAddress);
    useExport.Export1.CopyTo(export.Export1, MoveExport4);
    useExport.Errors.CopyTo(local.Errors, MoveErrors);
  }

  private void UseOePconValidateContactDetail2()
  {
    var useImport = new OePconValidateContactDetail.Import();
    var useExport = new OePconValidateContactDetail.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.UserAction.Command = local.CurrentUserAction.Command;
    useImport.Contact.Assign(export.Contact);

    Call(OePconValidateContactDetail.Execute, useImport, useExport);

    export.Work.FormattedName = useExport.WorkSet.FormattedName;
    export.CsePerson.Number = useExport.CsePerson.Number;
    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    MoveContact1(useExport.Contact, export.Contact);
    useExport.Errors.CopyTo(local.Errors, MoveErrors);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);
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

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

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

    useImport.WorkArea.Text50 = local.WorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.WorkArea.Text50 = useExport.WorkArea.Text50;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailContactDetail.
      /// </summary>
      [JsonPropertyName("detailContactDetail")]
      public ContactDetail DetailContactDetail
      {
        get => detailContactDetail ??= new();
        set => detailContactDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailCommon;
      private ContactDetail detailContactDetail;
    }

    /// <summary>
    /// A value of HrespForHealthIns.
    /// </summary>
    [JsonPropertyName("hrespForHealthIns")]
    public Common HrespForHealthIns
    {
      get => hrespForHealthIns ??= new();
      set => hrespForHealthIns = value;
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

    /// <summary>
    /// A value of StartingStateCode.
    /// </summary>
    [JsonPropertyName("startingStateCode")]
    public CodeValue StartingStateCode
    {
      get => startingStateCode ??= new();
      set => startingStateCode = value;
    }

    /// <summary>
    /// A value of ListPersonNo.
    /// </summary>
    [JsonPropertyName("listPersonNo")]
    public Standard ListPersonNo
    {
      get => listPersonNo ??= new();
      set => listPersonNo = value;
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
    /// A value of SelectedState.
    /// </summary>
    [JsonPropertyName("selectedState")]
    public CodeValue SelectedState
    {
      get => selectedState ??= new();
      set => selectedState = value;
    }

    /// <summary>
    /// A value of ListStateCodeCode.
    /// </summary>
    [JsonPropertyName("listStateCodeCode")]
    public Code ListStateCodeCode
    {
      get => listStateCodeCode ??= new();
      set => listStateCodeCode = value;
    }

    /// <summary>
    /// A value of ListStateCodeStandard.
    /// </summary>
    [JsonPropertyName("listStateCodeStandard")]
    public Standard ListStateCodeStandard
    {
      get => listStateCodeStandard ??= new();
      set => listStateCodeStandard = value;
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
    /// A value of LastUpdated.
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public Contact LastUpdated
    {
      get => lastUpdated ??= new();
      set => lastUpdated = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePersonsWorkSet Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPrevContact.
    /// </summary>
    [JsonPropertyName("hiddenPrevContact")]
    public Contact HiddenPrevContact
    {
      get => hiddenPrevContact ??= new();
      set => hiddenPrevContact = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ContactDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

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
    /// A value of XxxImportDelDataXferPrompt.
    /// </summary>
    [JsonPropertyName("xxxImportDelDataXferPrompt")]
    public Standard XxxImportDelDataXferPrompt
    {
      get => xxxImportDelDataXferPrompt ??= new();
      set => xxxImportDelDataXferPrompt = value;
    }

    private Common hrespForHealthIns;
    private AbendData abendData;
    private CodeValue startingStateCode;
    private Standard listPersonNo;
    private CsePersonsWorkSet selected;
    private CodeValue selectedState;
    private Code listStateCodeCode;
    private Standard listStateCodeStandard;
    private Case1 case1;
    private Common hiddenDisplayPerformed;
    private Contact lastUpdated;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet work;
    private Common hiddenPrevUserAction;
    private CsePerson hiddenPrevCsePerson;
    private Contact hiddenPrevContact;
    private ContactDetail starting;
    private CodeValue country;
    private ContactAddress contactAddress;
    private Contact contact;
    private CsePerson csePerson;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard xxxImportDelDataXferPrompt;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailContactDetail.
      /// </summary>
      [JsonPropertyName("detailContactDetail")]
      public ContactDetail DetailContactDetail
      {
        get => detailContactDetail ??= new();
        set => detailContactDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailCommon;
      private ContactDetail detailContactDetail;
    }

    /// <summary>
    /// A value of HrespForHealthIns.
    /// </summary>
    [JsonPropertyName("hrespForHealthIns")]
    public Common HrespForHealthIns
    {
      get => hrespForHealthIns ??= new();
      set => hrespForHealthIns = value;
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

    /// <summary>
    /// A value of ListPersonNo.
    /// </summary>
    [JsonPropertyName("listPersonNo")]
    public Standard ListPersonNo
    {
      get => listPersonNo ??= new();
      set => listPersonNo = value;
    }

    /// <summary>
    /// A value of StartingStateCode.
    /// </summary>
    [JsonPropertyName("startingStateCode")]
    public CodeValue StartingStateCode
    {
      get => startingStateCode ??= new();
      set => startingStateCode = value;
    }

    /// <summary>
    /// A value of ListStateCodeCode.
    /// </summary>
    [JsonPropertyName("listStateCodeCode")]
    public Code ListStateCodeCode
    {
      get => listStateCodeCode ??= new();
      set => listStateCodeCode = value;
    }

    /// <summary>
    /// A value of ListStateCodeStandard.
    /// </summary>
    [JsonPropertyName("listStateCodeStandard")]
    public Standard ListStateCodeStandard
    {
      get => listStateCodeStandard ??= new();
      set => listStateCodeStandard = value;
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
    /// A value of LastUpdated.
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public Contact LastUpdated
    {
      get => lastUpdated ??= new();
      set => lastUpdated = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePersonsWorkSet Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenPrevContact.
    /// </summary>
    [JsonPropertyName("hiddenPrevContact")]
    public Contact HiddenPrevContact
    {
      get => hiddenPrevContact ??= new();
      set => hiddenPrevContact = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ContactDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Common hrespForHealthIns;
    private AbendData abendData;
    private Standard listPersonNo;
    private CodeValue startingStateCode;
    private Code listStateCodeCode;
    private Standard listStateCodeStandard;
    private Case1 case1;
    private Common hiddenDisplayPerformed;
    private Contact lastUpdated;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet work;
    private Common hiddenPrevUserAction;
    private CsePerson hiddenPrevCsePerson;
    private Contact hiddenPrevContact;
    private ContactDetail starting;
    private CodeValue country;
    private ContactAddress contactAddress;
    private Contact contact;
    private CsePerson csePerson;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of DetailNoteEntry.
      /// </summary>
      [JsonPropertyName("detailNoteEntry")]
      public Common DetailNoteEntry
      {
        get => detailNoteEntry ??= new();
        set => detailNoteEntry = value;
      }

      /// <summary>
      /// A value of DetailError.
      /// </summary>
      [JsonPropertyName("detailError")]
      public Common DetailError
      {
        get => detailError ??= new();
        set => detailError = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailNoteEntry;
      private Common detailError;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of InitializedContactAddress.
    /// </summary>
    [JsonPropertyName("initializedContactAddress")]
    public ContactAddress InitializedContactAddress
    {
      get => initializedContactAddress ??= new();
      set => initializedContactAddress = value;
    }

    /// <summary>
    /// A value of InitializedContact.
    /// </summary>
    [JsonPropertyName("initializedContact")]
    public Contact InitializedContact
    {
      get => initializedContact ??= new();
      set => initializedContact = value;
    }

    /// <summary>
    /// A value of InitializedContactDetail.
    /// </summary>
    [JsonPropertyName("initializedContactDetail")]
    public ContactDetail InitializedContactDetail
    {
      get => initializedContactDetail ??= new();
      set => initializedContactDetail = value;
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
    /// A value of ReturnRetcdvl.
    /// </summary>
    [JsonPropertyName("returnRetcdvl")]
    public Common ReturnRetcdvl
    {
      get => returnRetcdvl ??= new();
      set => returnRetcdvl = value;
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
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of ContactDisplayed.
    /// </summary>
    [JsonPropertyName("contactDisplayed")]
    public Common ContactDisplayed
    {
      get => contactDisplayed ??= new();
      set => contactDisplayed = value;
    }

    /// <summary>
    /// A value of CurrentUserAction.
    /// </summary>
    [JsonPropertyName("currentUserAction")]
    public Common CurrentUserAction
    {
      get => currentUserAction ??= new();
      set => currentUserAction = value;
    }

    /// <summary>
    /// A value of HighlightValidationError.
    /// </summary>
    [JsonPropertyName("highlightValidationError")]
    public Common HighlightValidationError
    {
      get => highlightValidationError ??= new();
      set => highlightValidationError = value;
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
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
    }

    private NextTranInfo null1;
    private ContactAddress initializedContactAddress;
    private Contact initializedContact;
    private ContactDetail initializedContactDetail;
    private Common work;
    private Common returnRetcdvl;
    private SpDocLiteral spDocLiteral;
    private BatchConvertNumToText batchConvertNumToText;
    private Common position;
    private WorkArea workArea;
    private TextWorkArea textWorkArea;
    private Document document;
    private Common contactDisplayed;
    private Common currentUserAction;
    private Common highlightValidationError;
    private Common lastErrorEntryNo;
    private Array<ErrorsGroup> errors;
  }
#endregion
}

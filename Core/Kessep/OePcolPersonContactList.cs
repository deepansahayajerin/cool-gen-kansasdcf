// Program: OE_PCOL_PERSON_CONTACT_LIST, ID: 371890282, model: 746.
// Short name: SWEPCOLP
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
/// A program: OE_PCOL_PERSON_CONTACT_LIST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure lists CSE Person's Contacts.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OePcolPersonContactList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PCOL_PERSON_CONTACT_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePcolPersonContactList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePcolPersonContactList.
  /// </summary>
  public OePcolPersonContactList(IContext context, Import import, Export export):
    
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
    // This procedure step lists contact details
    // PROCESSING:
    // It is passed with CSE_PERSON NUMBER. It reads and lists all the Contacts 
    // associated with the CSE Person.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R - -
    // 	CONTACT_ADDRESS		- R - -
    // DATABASE FILES USED:
    // CREATED BY:	govindaraj
    // DATE CREATED:	01/24/95
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHGREQ	DESCRIPTION
    // Govind	01/26/95	Initial coding
    // Lofton  02/16/96        Retrofit changes.
    // Welborn 06/26/96        Left Pad EAB.
    // Marchman 11/13/96	Add new security and next tran
    // *********************************************
    // 04/05/00   W.Campbell      Disabled existing call to
    //                            Security Cab and added a
    //                            new call with view matching
    //                            changed to match the export
    //                            views of case and cse_person.
    //                            Work done on WR#000162
    //                            for PRWORA - Family Violence.
    // --------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    // ----------------------------------------------------------
    // Beginning Of Change
    // 4.16.100 TC # 24
    // ----------------------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      export.Case1.Number = import.Case1.Number;
      export.CsePerson.Number = import.CsePerson.Number;
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // ----------------------------------------------------------
    // End Of Change
    // 4.16.100 TC # 24
    // ----------------------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.ListPersonNo.PromptField = import.ListPersonNo.PromptField;
    export.Starting.Assign(import.Starting);

    if (!import.Group.IsEmpty)
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.DetailCommon.SelectChar =
          import.Group.Item.DetailCommon.SelectChar;
        export.Group.Update.DetailContact.
          Assign(import.Group.Item.DetailContact);
        MoveContactAddress(import.Group.Item.DetailContactAddress,
          export.Group.Update.DetailContactAddress);
        export.Group.Next();
      }
    }

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
    }

    // ----------------------------------------------
    // Beginning Of Change
    // 4.16.100 TC # 13
    // ---------------------------------------------
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME"))
    {
      export.ListPersonNo.PromptField = "";
    }

    // ----------------------------------------------
    // End Of Change
    // 4.16.100 TC # 13
    // ---------------------------------------------
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
    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
      }

      // ----------------------------------------------
      // Beginning Of Change
      // 4.16.100 TC # 13
      // ---------------------------------------------
      export.ListPersonNo.PromptField = "";

      // ----------------------------------------------
      // End Of Change
      // 4.16.100 TC # 13
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    // ----------------------------------------------------------
    // Beginning Of Change
    // 4.16.100 TC # 1
    // The following if statement is newly added
    // and whole Read statement is moved inside it.
    // ----------------------------------------------------------
    if (!IsEmpty(export.CsePerson.Number))
    {
      // ----------------------------------------------------------
      // End Of Change
      // 4.16.100 TC # 1
      // whole Read statement is moved inside it.
      // ----------------------------------------------------------
      if (ReadCsePerson())
      {
        export.CsePerson.Number = entities.ExistingCsePerson.Number;
        local.CsePersonsWorkSet.Number = entities.ExistingCsePerson.Number;
        UseSiReadCsePerson();
      }
      else
      {
        // ----------------------------------------------------------
        // Beginning Of Change
        // 4.16.100 TC # 9
        // ----------------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          export.Group.Update.DetailContact.CompanyName = "";
          export.Group.Update.DetailContact.ContactNumber = 0;
          export.Group.Update.DetailContact.MiddleInitial = "";
          export.Group.Update.DetailContact.NameFirst = "";
          export.Group.Update.DetailContact.NameLast = "";
          export.Group.Update.DetailContact.RelationshipToCsePerson = "";
          export.Group.Update.DetailContactAddress.City = "";
          export.Group.Update.DetailContactAddress.State = "";
          export.Group.Update.DetailCommon.SelectChar = "";
        }

        // ----------------------------------------------------------
        // End Of Change
        // 4.16.100 TC # 9
        // ----------------------------------------------------------
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    // ----------------------------------------------------------
    // Beginning Of Change
    // 4.16.100 TC # 22
    // ----------------------------------------------------------
    if (Equal(global.Command, "RETPCON"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        export.Group.Update.DetailCommon.SelectChar = "";
      }

      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
      global.Command = "DISPLAY";
    }

    // ----------------------------------------------------------
    // End Of Change
    // 4.16.100 TC # 22
    // ----------------------------------------------------------
    if (Equal(global.Command, "PCON"))
    {
    }
    else
    {
      // **** begin group C ****
      // to validate action level security
      // --------------------------------------------------------
      // 04/05/00 W.Campbell - Disabled existing call to
      // Security Cab and added a new call with view
      // matching changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
      UseScCabTestSecurity();

      // --------------------------------------------------------
      // 04/04/00 W.Campbell - End of change to
      // disable existing call to Security Cab and
      // added a new call with view matching
      // changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
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
      case "LIST":
        // -------------------------------------------------------------
        // Beginning Of Change
        // 4.16.100 TC # 13
        // ------------------------------------------------------------
        if (!IsEmpty(export.ListPersonNo.PromptField) && AsChar
          (export.ListPersonNo.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.ListPersonNo, "promptField");

          field1.Error = true;

          return;
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

        var field = GetField(export.ListPersonNo, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        // -------------------------------------------------------------
        // End Of Change
        // 4.16.100 TC # 13
        // ------------------------------------------------------------
        break;
      case "PCON":
        local.Selected.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Selected.Count;
            MoveContact(export.Group.Item.DetailContact, export.SelectedContact);
              
          }

          if (!IsEmpty(export.Group.Item.DetailCommon.SelectChar) && AsChar
            (export.Group.Item.DetailCommon.SelectChar) != 'S')
          {
            ++local.Error.Count;
          }
        }

        if (local.Error.Count > 0)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!IsEmpty(export.Group.Item.DetailCommon.SelectChar) && AsChar
              (export.Group.Item.DetailCommon.SelectChar) != 'S')
            {
              var field1 =
                GetField(export.Group.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }

        if (local.Selected.Count > 1)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
            {
              var field1 =
                GetField(export.Group.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
        }

        // ---------------------------------------------
        // Now read the CONTACT and CONTACT_ADDRESS again to move all the 
        // attributes to selected export view.
        // ---------------------------------------------
        if (local.Selected.Count == 1)
        {
          if (ReadContact1())
          {
            export.SelectedContact.Assign(entities.ExistingContact);

            foreach(var item in ReadContactAddress2())
            {
              export.SelectedContactAddress.Assign(
                entities.ExistingContactAddress);
            }
          }

          ExitState = "ECO_XFR_TO_PCON_PERSON_CONT";

          return;
        }

        if (local.Selected.Count == 0 && local.Error.Count == 0)
        {
          ExitState = "ECO_XFR_TO_PCON_PERSON_CONT";
        }

        break;
      case "DISPLAY":
        // ----------------------------------------------------------
        // Beginning Of Change
        // 4.16.100 TC # 1
        // ----------------------------------------------------------
        if (IsEmpty(export.CsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "CSE_PERSON_NO_REQUIRED";

          return;
        }

        UseOeCabCheckCaseMember();

        switch(AsChar(local.Error.Flag))
        {
          case 'C':
            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            ExitState = "CASE_NF";

            break;
          case 'P':
            var field2 = GetField(export.CsePerson, "number");

            field2.Error = true;

            export.CsePersonsWorkSet.FormattedName = "";
            ExitState = "CSE_PERSON_NF";

            break;
          case 'R':
            var field3 = GetField(export.Case1, "number");

            field3.Error = true;

            var field4 = GetField(export.CsePerson, "number");

            field4.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";

            break;
          default:
            break;
        }

        if (!IsEmpty(local.Error.Flag))
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            MoveContact(local.InitializedContact, export.Starting);
            MoveContactAddress(local.InitializedContactAddress,
              export.Group.Update.DetailContactAddress);
            export.Group.Update.DetailCommon.SelectChar = "";
          }

          return;
        }

        // ----------------------------------------------------------
        // End Of Change
        // 4.16.100 TC # 1
        // ----------------------------------------------------------
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadContact2())
        {
          MoveContact(entities.ExistingContact,
            export.Group.Update.DetailContact);
          export.Group.Update.DetailCommon.SelectChar = "";

          // Read the latest address.
          if (ReadContactAddress1())
          {
            MoveContactAddress(entities.ExistingContactAddress,
              export.Group.Update.DetailContactAddress);
          }

          export.Group.Next();
        }

        if (export.Group.IsFull)
        {
          ExitState = "OE0000_LIST_FULL_PARTIAL_DATA_RT";
        }
        else if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "RETURN":
        local.Selected.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.DetailCommon.SelectChar) && AsChar
            (export.Group.Item.DetailCommon.SelectChar) != 'S')
          {
            var field1 = GetField(export.Group.Item.DetailCommon, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Selected.Count;

            if (local.Selected.Count > 1)
            {
              var field1 =
                GetField(export.Group.Item.DetailCommon, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            }

            MoveContact(export.Group.Item.DetailContact, export.SelectedContact);
              
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // Now read the CONTACT and CONTACT_ADDRESS again to
        // move all the attributes to selected export view.
        // ---------------------------------------------
        if (local.Selected.Count == 1)
        {
          if (ReadContact1())
          {
            export.SelectedContact.Assign(entities.ExistingContact);

            foreach(var item in ReadContactAddress2())
            {
              export.SelectedContactAddress.Assign(
                entities.ExistingContactAddress);
            }
          }
        }

        // **** begin group E ****
        ExitState = "ACO_NE0000_RETURN";

        // **** end   group E ****
        break;
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveContact(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.CompanyName = source.CompanyName;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveContactAddress(ContactAddress source,
    ContactAddress target)
  {
    target.City = source.City;
    target.State = source.State;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Error.Flag = useExport.Work.Flag;
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
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadContact1()
  {
    entities.ExistingContact.Populated = false;

    return Read("ReadContact1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "contactNumber", export.SelectedContact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingContact.NameTitle = db.GetNullableString(reader, 3);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 4);
        entities.ExistingContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 6);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 7);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingContact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.ExistingContact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.ExistingContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingContact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ExistingContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingContact.Populated = true;
      });
  }

  private IEnumerable<bool> ReadContact2()
  {
    return ReadEach("ReadContact2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", export.Starting.ContactNumber);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingContact.NameTitle = db.GetNullableString(reader, 3);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 4);
        entities.ExistingContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 6);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 7);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingContact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.ExistingContact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.ExistingContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingContact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ExistingContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingContact.Populated = true;

        return true;
      });
  }

  private bool ReadContactAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingContactAddress.Populated = false;

    return Read("ReadContactAddress1",
      (db, command) =>
      {
        db.
          SetInt32(command, "conNumber", entities.ExistingContact.ContactNumber);
          
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContactAddress.ConNumber = db.GetInt32(reader, 0);
        entities.ExistingContactAddress.CspNumber = db.GetString(reader, 1);
        entities.ExistingContactAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingContactAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingContactAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingContactAddress.City = db.GetNullableString(reader, 5);
        entities.ExistingContactAddress.State = db.GetNullableString(reader, 6);
        entities.ExistingContactAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingContactAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingContactAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingContactAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingContactAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.ExistingContactAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingContactAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingContactAddress.CreatedBy = db.GetString(reader, 14);
        entities.ExistingContactAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingContactAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingContactAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadContactAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingContactAddress.Populated = false;

    return ReadEach("ReadContactAddress2",
      (db, command) =>
      {
        db.
          SetInt32(command, "conNumber", entities.ExistingContact.ContactNumber);
          
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContactAddress.ConNumber = db.GetInt32(reader, 0);
        entities.ExistingContactAddress.CspNumber = db.GetString(reader, 1);
        entities.ExistingContactAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingContactAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingContactAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingContactAddress.City = db.GetNullableString(reader, 5);
        entities.ExistingContactAddress.State = db.GetNullableString(reader, 6);
        entities.ExistingContactAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingContactAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingContactAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingContactAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingContactAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.ExistingContactAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingContactAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingContactAddress.CreatedBy = db.GetString(reader, 14);
        entities.ExistingContactAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingContactAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingContactAddress.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of DetailContact.
      /// </summary>
      [JsonPropertyName("detailContact")]
      public Contact DetailContact
      {
        get => detailContact ??= new();
        set => detailContact = value;
      }

      /// <summary>
      /// A value of DetailContactAddress.
      /// </summary>
      [JsonPropertyName("detailContactAddress")]
      public ContactAddress DetailContactAddress
      {
        get => detailContactAddress ??= new();
        set => detailContactAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common detailCommon;
      private Contact detailContact;
      private ContactAddress detailContactAddress;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
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
    /// A value of SelectedContactAddress.
    /// </summary>
    [JsonPropertyName("selectedContactAddress")]
    public ContactAddress SelectedContactAddress
    {
      get => selectedContactAddress ??= new();
      set => selectedContactAddress = value;
    }

    /// <summary>
    /// A value of SelectedContact.
    /// </summary>
    [JsonPropertyName("selectedContact")]
    public Contact SelectedContact
    {
      get => selectedContact ??= new();
      set => selectedContact = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Contact Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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

    private Standard listPersonNo;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Case1 case1;
    private ContactAddress selectedContactAddress;
    private Contact selectedContact;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Contact starting;
    private CsePerson csePerson;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of DetailContact.
      /// </summary>
      [JsonPropertyName("detailContact")]
      public Contact DetailContact
      {
        get => detailContact ??= new();
        set => detailContact = value;
      }

      /// <summary>
      /// A value of DetailContactAddress.
      /// </summary>
      [JsonPropertyName("detailContactAddress")]
      public ContactAddress DetailContactAddress
      {
        get => detailContactAddress ??= new();
        set => detailContactAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common detailCommon;
      private Contact detailContact;
      private ContactAddress detailContactAddress;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of SelectedContactAddress.
    /// </summary>
    [JsonPropertyName("selectedContactAddress")]
    public ContactAddress SelectedContactAddress
    {
      get => selectedContactAddress ??= new();
      set => selectedContactAddress = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Contact Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of SelectedContact.
    /// </summary>
    [JsonPropertyName("selectedContact")]
    public Contact SelectedContact
    {
      get => selectedContact ??= new();
      set => selectedContact = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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

    private Standard listPersonNo;
    private Case1 case1;
    private ContactAddress selectedContactAddress;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Contact starting;
    private Contact selectedContact;
    private CsePerson csePerson;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of InitializedContactAddress.
    /// </summary>
    [JsonPropertyName("initializedContactAddress")]
    public ContactAddress InitializedContactAddress
    {
      get => initializedContactAddress ??= new();
      set => initializedContactAddress = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SetExitStateToMaintain.
    /// </summary>
    [JsonPropertyName("setExitStateToMaintain")]
    public Common SetExitStateToMaintain
    {
      get => setExitStateToMaintain ??= new();
      set => setExitStateToMaintain = value;
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

    private Contact initializedContact;
    private ContactAddress initializedContactAddress;
    private Common error;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common setExitStateToMaintain;
    private Common selected;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingContactAddress.
    /// </summary>
    [JsonPropertyName("existingContactAddress")]
    public ContactAddress ExistingContactAddress
    {
      get => existingContactAddress ??= new();
      set => existingContactAddress = value;
    }

    /// <summary>
    /// A value of ExistingContact.
    /// </summary>
    [JsonPropertyName("existingContact")]
    public Contact ExistingContact
    {
      get => existingContact ??= new();
      set => existingContact = value;
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

    private ContactAddress existingContactAddress;
    private Contact existingContact;
    private CsePerson existingCsePerson;
  }
#endregion
}

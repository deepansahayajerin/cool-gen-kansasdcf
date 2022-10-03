// Program: OE_MARL_MARRIAGE_HISTORY_LIST, ID: 371888966, model: 746.
// Short name: SWEMARLP
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
/// A program: OE_MARL_MARRIAGE_HISTORY_LIST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure lists marriage history details of a CSE Person.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeMarlMarriageHistoryList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MARL_MARRIAGE_HISTORY_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMarlMarriageHistoryList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMarlMarriageHistoryList.
  /// </summary>
  public OeMarlMarriageHistoryList(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This procedure step lists marriage history for a CSE Person.
    // PROCESSING:
    // It reads MARRIAGE_HISTORY for the given CSE_PERSON in descending order of
    // marriage date and lists the name of the CSE_PERSON or CONTACT associated
    // with the MARRIAGE_HISTORY record. It also provides for a starting
    // marriage date to be entered if the list is too big to be accomodated by
    // implicit scrolling.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // MARRIAGE_HISTORY	- R - -
    // CSE_PERSON		- R - -
    // CONTACT			- R - -
    // DATABASE FILES USED:
    // CREATED BY:		Govindaraj
    // DATE CREATED:		12-27-1994
    // *********************************************
    // *********************************************
    // MAINTEANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // Govind	12/27/94			Initial coding.
    // Lofton	02/12/96			Retrofit changes.
    // Welborn 6/26/96
    //         Add call for EAB for Left Padd of 0's.
    // Marchman 1/8/96                        Add new security and next tran.
    // *********************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.FormattedName =
      import.CsePersonsWorkSet.FormattedName;
    export.Case1.Number = import.Case1.Number;
    export.ListCsePersons.PromptField = import.ListCsePersons.PromptField;

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

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (!Equal(global.Command, "LIST"))
    {
      export.ListCsePersons.PromptField = "";
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";
    }

    if (!import.Import1.IsEmpty && !Equal(global.Command, "DISPLAY"))
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

        export.Export1.Update.DetailSelectOpt.OneChar =
          import.Import1.Item.DetailSelectOpt.OneChar;
        export.Export1.Update.DetailSpouseMarriageHistory.Assign(
          import.Import1.Item.DetailSpouseMarriageHistory);
        export.Export1.Update.DetailSpouseCsePerson.Number =
          import.Import1.Item.DetailSpouseCsePerson.Number;
        export.Export1.Update.DetailContactInd.OneChar =
          import.Import1.Item.DetailContactInd.OneChar;
        export.Export1.Update.DetailSpouseCsePersonsWorkSet.Assign(
          import.Import1.Item.DetailSpouseCsePersonsWorkSet);
        export.Export1.Update.DetailSpouseContact.ContactNumber =
          import.Import1.Item.DetailSpouseContact.ContactNumber;
        export.Export1.Update.DetailPrmOrSpous.OneChar =
          import.Import1.Item.DetailPrmOrSpous.OneChar;
        export.Export1.Next();
      }
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
      export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    if (Equal(global.Command, "RETCOMP") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "PCOL") || Equal
      (global.Command, "MARH"))
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
      case "PCOL":
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailSelectOpt.OneChar))
          {
            ++local.Selected.Count;

            if (AsChar(export.Export1.Item.DetailSelectOpt.OneChar) == 'S')
            {
              MoveMarriageHistory(export.Export1.Item.
                DetailSpouseMarriageHistory, export.Selected);

              if (AsChar(export.Export1.Item.DetailContactInd.OneChar) == 'Y')
              {
                export.SelectedSpouseContact.ContactNumber =
                  export.Export1.Item.DetailSpouseContact.ContactNumber;
              }
              else if (AsChar(export.Export1.Item.DetailPrmOrSpous.OneChar) == 'S'
                )
              {
                export.SelectedSpouseCsePerson.Number =
                  export.Export1.Item.DetailSpouseCsePerson.Number;
              }
              else
              {
                export.SelectedSpouseCsePerson.Number = export.CsePerson.Number;
                export.CsePerson.Number =
                  export.Export1.Item.DetailSpouseCsePerson.Number;
              }
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailSelectOpt, "oneChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }
        }

        if (local.Selected.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.DetailSelectOpt.OneChar))
            {
              var field =
                GetField(export.Export1.Item.DetailSelectOpt, "oneChar");

              field.Error = true;
            }
          }

          ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ECO_LNK_TO_LIST_CONTACT";
        }

        break;
      case "LIST":
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
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListCsePersons, "promptField");

          field.Error = true;
        }

        break;
      case "DISPLAY":
        if (IsEmpty(export.CsePerson.Number))
        {
          export.CsePersonsWorkSet.FormattedName = "";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NO_REQUIRED";

          return;
        }

        if (!IsEmpty(export.Case1.Number))
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

          if (!IsEmpty(local.Work.Flag))
          {
            return;
          }
        }

        if (ReadCsePerson3())
        {
          export.CsePerson.Number = entities.ExistingSelected.Number;
          local.CsePersonsWorkSet.Number = export.CsePerson.Number;
          UseSiReadCsePerson1();
        }
        else
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          export.CsePersonsWorkSet.FormattedName = "";
          ExitState = "OE0026_INVALID_CSE_PERSON_NO";

          return;
        }

        local.Temp00010101.MarriageDate = new DateTime(1, 1, 1);

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadMarriageHistory2())
        {
          export.Export1.Update.DetailSelectOpt.OneChar = "";
          MoveMarriageHistory(entities.Existing,
            export.Export1.Update.DetailSpouseMarriageHistory);

          if (ReadCsePerson5())
          {
            if (Equal(entities.ExistingSpouseCsePerson.Number,
              entities.ExistingSelected.Number))
            {
              export.Export1.Update.DetailPrmOrSpous.OneChar = "P";

              if (ReadCsePerson2())
              {
                export.Export1.Update.DetailSpouseCsePerson.Number =
                  entities.ExistingPrime.Number;
                local.CsePersonsWorkSet.Number =
                  export.Export1.Item.DetailSpouseCsePerson.Number;
                UseSiReadCsePerson2();
              }
            }
            else
            {
              export.Export1.Update.DetailSpouseCsePerson.Number =
                entities.ExistingSpouseCsePerson.Number;
              local.CsePersonsWorkSet.Number =
                export.Export1.Item.DetailSpouseCsePerson.Number;
              UseSiReadCsePerson2();
              export.Export1.Update.DetailPrmOrSpous.OneChar = "S";
            }

            export.Export1.Update.DetailContactInd.OneChar = "";
          }

          if (ReadContact2())
          {
            export.Export1.Update.DetailSpouseContact.ContactNumber =
              entities.ExistingSpouseContact.ContactNumber;
            export.Export1.Update.DetailPrmOrSpous.OneChar = "S";
            export.Export1.Update.DetailContactInd.OneChar = "Y";
            export.Export1.Update.DetailSpouseCsePerson.Number = "";
            export.Export1.Update.DetailSpouseCsePersonsWorkSet.FirstName =
              entities.ExistingSpouseContact.NameFirst ?? Spaces(12);
            export.Export1.Update.DetailSpouseCsePersonsWorkSet.LastName =
              entities.ExistingSpouseContact.NameLast ?? Spaces(17);
            export.Export1.Update.DetailSpouseCsePersonsWorkSet.MiddleInitial =
              entities.ExistingSpouseContact.MiddleInitial ?? Spaces(1);
          }

          export.Export1.Next();
        }

        if (!export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "OE0001_NO_MARRIAGE_HISTORY_AVL";
        }

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "RETURN":
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailSelectOpt.OneChar))
          {
            ++local.Selected.Count;

            if (AsChar(export.Export1.Item.DetailSelectOpt.OneChar) == 'S')
            {
              MoveMarriageHistory(export.Export1.Item.
                DetailSpouseMarriageHistory, export.Selected);

              if (AsChar(export.Export1.Item.DetailContactInd.OneChar) == 'Y')
              {
                export.SelectedSpouseContact.ContactNumber =
                  export.Export1.Item.DetailSpouseContact.ContactNumber;
              }
              else if (AsChar(export.Export1.Item.DetailPrmOrSpous.OneChar) == 'S'
                )
              {
                export.SelectedSpouseCsePerson.Number =
                  export.Export1.Item.DetailSpouseCsePerson.Number;
              }
              else
              {
                export.SelectedSpouseCsePerson.Number = export.CsePerson.Number;
                export.CsePerson.Number =
                  export.Export1.Item.DetailSpouseCsePerson.Number;
              }
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailSelectOpt, "oneChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }
        }

        if (local.Selected.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.DetailSelectOpt.OneChar))
            {
              var field =
                GetField(export.Export1.Item.DetailSelectOpt, "oneChar");

              field.Error = true;
            }
          }

          ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.Selected.Count == 0)
        {
          // **** begin group E ****
          ExitState = "ACO_NE0000_RETURN";

          // **** end   group E ****
          return;
        }

        // ---------------------------------------------
        // Now read the database again to get all the attribute views for 
        // export. Group export detail view contains only minimum attributes
        // ---------------------------------------------
        if (ReadCsePerson1())
        {
          export.CsePerson.Number = entities.ExistingPrime.Number;
        }

        if (ReadMarriageHistory1())
        {
          export.Selected.Assign(entities.Existing);
        }

        if (!IsEmpty(entities.ExistingSpouseCsePerson.Number))
        {
          if (ReadCsePerson4())
          {
            export.SelectedSpouseCsePerson.Number =
              entities.ExistingSpouseCsePerson.Number;
          }
        }
        else if (ReadContact1())
        {
          export.SelectedSpouseContact.Assign(entities.ExistingSpouseContact);
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
      case "MARH":
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailSelectOpt.OneChar))
          {
            ++local.Selected.Count;

            if (AsChar(export.Export1.Item.DetailSelectOpt.OneChar) == 'S')
            {
              MoveMarriageHistory(export.Export1.Item.
                DetailSpouseMarriageHistory, export.Selected);

              if (AsChar(export.Export1.Item.DetailContactInd.OneChar) == 'Y')
              {
                export.SelectedSpouseContact.ContactNumber =
                  export.Export1.Item.DetailSpouseContact.ContactNumber;
              }
              else if (AsChar(export.Export1.Item.DetailPrmOrSpous.OneChar) == 'S'
                )
              {
                export.SelectedSpouseCsePerson.Number =
                  export.Export1.Item.DetailSpouseCsePerson.Number;
              }
              else
              {
                export.SelectedSpouseCsePerson.Number = export.CsePerson.Number;
                export.CsePerson.Number =
                  export.Export1.Item.DetailSpouseCsePerson.Number;
              }
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailSelectOpt, "oneChar");

              field.Error = true;
            }
          }
        }

        if (local.Selected.Count == 0 || local.Selected.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailSelectOpt, "oneChar");

            field.Error = true;
          }

          if (local.Selected.Count == 0)
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
          else
          {
            ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // Now read the database again to get all the attribute views for 
        // export. Group export detail view contains only minimum attributes
        // ---------------------------------------------
        if (ReadCsePerson1())
        {
          export.CsePerson.Number = entities.ExistingPrime.Number;
        }

        if (ReadMarriageHistory1())
        {
          export.Selected.Assign(entities.Existing);
        }

        if (!IsEmpty(entities.ExistingSpouseCsePerson.Number))
        {
          if (ReadCsePerson4())
          {
            export.SelectedSpouseCsePerson.Number =
              entities.ExistingSpouseCsePerson.Number;
          }
        }
        else if (ReadContact1())
        {
          export.SelectedSpouseContact.Assign(entities.ExistingSpouseContact);
        }

        // **** begin group E ****
        ExitState = "ECO_XFR_TO_MARH_MARRIAGE_HST";

        // **** end   group E ****
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveMarriageHistory(MarriageHistory source,
    MarriageHistory target)
  {
    target.Identifier = source.Identifier;
    target.DivorceDate = source.DivorceDate;
    target.MarriageDate = source.MarriageDate;
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

    local.Work.Flag = useExport.Work.Flag;
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

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Export1.Update.DetailSpouseCsePersonsWorkSet.Assign(
      useExport.CsePersonsWorkSet);
  }

  private bool ReadContact1()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingSpouseContact.Populated = false;

    return Read("ReadContact1",
      (db, command) =>
      {
        db.SetInt32(
          command, "contactNumber1",
          entities.Existing.ConINumber.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Existing.CspINumber ?? "");
        db.SetInt32(
          command, "contactNumber2",
          export.SelectedSpouseContact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingSpouseContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingSpouseContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingSpouseContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingSpouseContact.NameTitle =
          db.GetNullableString(reader, 3);
        entities.ExistingSpouseContact.CompanyName =
          db.GetNullableString(reader, 4);
        entities.ExistingSpouseContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingSpouseContact.NameLast =
          db.GetNullableString(reader, 6);
        entities.ExistingSpouseContact.NameFirst =
          db.GetNullableString(reader, 7);
        entities.ExistingSpouseContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingSpouseContact.HomePhone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingSpouseContact.WorkPhone =
          db.GetNullableInt32(reader, 10);
        entities.ExistingSpouseContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingSpouseContact.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingSpouseContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingSpouseContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingSpouseContact.Populated = true;
      });
  }

  private bool ReadContact2()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingSpouseContact.Populated = false;

    return Read("ReadContact2",
      (db, command) =>
      {
        db.SetInt32(
          command, "contactNumber",
          entities.Existing.ConINumber.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Existing.CspINumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSpouseContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingSpouseContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingSpouseContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingSpouseContact.NameTitle =
          db.GetNullableString(reader, 3);
        entities.ExistingSpouseContact.CompanyName =
          db.GetNullableString(reader, 4);
        entities.ExistingSpouseContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingSpouseContact.NameLast =
          db.GetNullableString(reader, 6);
        entities.ExistingSpouseContact.NameFirst =
          db.GetNullableString(reader, 7);
        entities.ExistingSpouseContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingSpouseContact.HomePhone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingSpouseContact.WorkPhone =
          db.GetNullableInt32(reader, 10);
        entities.ExistingSpouseContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingSpouseContact.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingSpouseContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingSpouseContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingSpouseContact.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPrime.Number = db.GetString(reader, 0);
        entities.ExistingPrime.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Existing.CspRNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrime.Number = db.GetString(reader, 0);
        entities.ExistingPrime.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.ExistingSelected.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingSelected.Number = db.GetString(reader, 0);
        entities.ExistingSelected.Populated = true;
      });
  }

  private bool ReadCsePerson4()
  {
    entities.ExistingSpouseCsePerson.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SelectedSpouseCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingSpouseCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingSpouseCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson5()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingSpouseCsePerson.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Existing.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSpouseCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingSpouseCsePerson.Populated = true;
      });
  }

  private bool ReadMarriageHistory1()
  {
    entities.Existing.Populated = false;

    return Read("ReadMarriageHistory1",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetInt32(command, "identifier", export.Selected.Identifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspRNumber = db.GetString(reader, 0);
        entities.Existing.MarriageDate = db.GetNullableDate(reader, 1);
        entities.Existing.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.Existing.DivorcePetitionDate = db.GetNullableDate(reader, 3);
        entities.Existing.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.Existing.MarriageCountry = db.GetNullableString(reader, 5);
        entities.Existing.DivorcePendingInd = db.GetNullableString(reader, 6);
        entities.Existing.DivorceCounty = db.GetNullableString(reader, 7);
        entities.Existing.DivorceState = db.GetNullableString(reader, 8);
        entities.Existing.DivorceCountry = db.GetNullableString(reader, 9);
        entities.Existing.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.Existing.DivorceDate = db.GetNullableDate(reader, 11);
        entities.Existing.SeparationDate = db.GetNullableDate(reader, 12);
        entities.Existing.CreatedBy = db.GetString(reader, 13);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.Existing.LastUpdatedBy = db.GetString(reader, 15);
        entities.Existing.LastUpdatedTimestamp = db.GetDateTime(reader, 16);
        entities.Existing.CspNumber = db.GetNullableString(reader, 17);
        entities.Existing.CspINumber = db.GetNullableString(reader, 18);
        entities.Existing.ConINumber = db.GetNullableInt32(reader, 19);
        entities.Existing.DivorceCity = db.GetNullableString(reader, 20);
        entities.Existing.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.Existing.Identifier = db.GetInt32(reader, 22);
        entities.Existing.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMarriageHistory2()
  {
    return ReadEach("ReadMarriageHistory2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingSelected.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Existing.CspRNumber = db.GetString(reader, 0);
        entities.Existing.MarriageDate = db.GetNullableDate(reader, 1);
        entities.Existing.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.Existing.DivorcePetitionDate = db.GetNullableDate(reader, 3);
        entities.Existing.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.Existing.MarriageCountry = db.GetNullableString(reader, 5);
        entities.Existing.DivorcePendingInd = db.GetNullableString(reader, 6);
        entities.Existing.DivorceCounty = db.GetNullableString(reader, 7);
        entities.Existing.DivorceState = db.GetNullableString(reader, 8);
        entities.Existing.DivorceCountry = db.GetNullableString(reader, 9);
        entities.Existing.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.Existing.DivorceDate = db.GetNullableDate(reader, 11);
        entities.Existing.SeparationDate = db.GetNullableDate(reader, 12);
        entities.Existing.CreatedBy = db.GetString(reader, 13);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.Existing.LastUpdatedBy = db.GetString(reader, 15);
        entities.Existing.LastUpdatedTimestamp = db.GetDateTime(reader, 16);
        entities.Existing.CspNumber = db.GetNullableString(reader, 17);
        entities.Existing.CspINumber = db.GetNullableString(reader, 18);
        entities.Existing.ConINumber = db.GetNullableInt32(reader, 19);
        entities.Existing.DivorceCity = db.GetNullableString(reader, 20);
        entities.Existing.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.Existing.Identifier = db.GetInt32(reader, 22);
        entities.Existing.Populated = true;

        return true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailSelectOpt.
      /// </summary>
      [JsonPropertyName("detailSelectOpt")]
      public Standard DetailSelectOpt
      {
        get => detailSelectOpt ??= new();
        set => detailSelectOpt = value;
      }

      /// <summary>
      /// A value of DetailSpouseMarriageHistory.
      /// </summary>
      [JsonPropertyName("detailSpouseMarriageHistory")]
      public MarriageHistory DetailSpouseMarriageHistory
      {
        get => detailSpouseMarriageHistory ??= new();
        set => detailSpouseMarriageHistory = value;
      }

      /// <summary>
      /// A value of DetailSpouseCsePerson.
      /// </summary>
      [JsonPropertyName("detailSpouseCsePerson")]
      public CsePerson DetailSpouseCsePerson
      {
        get => detailSpouseCsePerson ??= new();
        set => detailSpouseCsePerson = value;
      }

      /// <summary>
      /// A value of DetailSpouseContact.
      /// </summary>
      [JsonPropertyName("detailSpouseContact")]
      public Contact DetailSpouseContact
      {
        get => detailSpouseContact ??= new();
        set => detailSpouseContact = value;
      }

      /// <summary>
      /// A value of DetailSpouseCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailSpouseCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailSpouseCsePersonsWorkSet
      {
        get => detailSpouseCsePersonsWorkSet ??= new();
        set => detailSpouseCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailContactInd.
      /// </summary>
      [JsonPropertyName("detailContactInd")]
      public Standard DetailContactInd
      {
        get => detailContactInd ??= new();
        set => detailContactInd = value;
      }

      /// <summary>
      /// A value of DetailPrmOrSpous.
      /// </summary>
      [JsonPropertyName("detailPrmOrSpous")]
      public Standard DetailPrmOrSpous
      {
        get => detailPrmOrSpous ??= new();
        set => detailPrmOrSpous = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Standard detailSelectOpt;
      private MarriageHistory detailSpouseMarriageHistory;
      private CsePerson detailSpouseCsePerson;
      private Contact detailSpouseContact;
      private CsePersonsWorkSet detailSpouseCsePersonsWorkSet;
      private Standard detailContactInd;
      private Standard detailPrmOrSpous;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of SelectedSpouseContact.
    /// </summary>
    [JsonPropertyName("selectedSpouseContact")]
    public Contact SelectedSpouseContact
    {
      get => selectedSpouseContact ??= new();
      set => selectedSpouseContact = value;
    }

    /// <summary>
    /// A value of SelectedSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("selectedSpouseCsePerson")]
    public CsePerson SelectedSpouseCsePerson
    {
      get => selectedSpouseCsePerson ??= new();
      set => selectedSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedMarriageHistory.
    /// </summary>
    [JsonPropertyName("selectedMarriageHistory")]
    public MarriageHistory SelectedMarriageHistory
    {
      get => selectedMarriageHistory ??= new();
      set => selectedMarriageHistory = value;
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

    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Standard listCsePersons;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Contact selectedSpouseContact;
    private CsePerson selectedSpouseCsePerson;
    private MarriageHistory selectedMarriageHistory;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of DetailSelectOpt.
      /// </summary>
      [JsonPropertyName("detailSelectOpt")]
      public Standard DetailSelectOpt
      {
        get => detailSelectOpt ??= new();
        set => detailSelectOpt = value;
      }

      /// <summary>
      /// A value of DetailSpouseMarriageHistory.
      /// </summary>
      [JsonPropertyName("detailSpouseMarriageHistory")]
      public MarriageHistory DetailSpouseMarriageHistory
      {
        get => detailSpouseMarriageHistory ??= new();
        set => detailSpouseMarriageHistory = value;
      }

      /// <summary>
      /// A value of DetailSpouseCsePerson.
      /// </summary>
      [JsonPropertyName("detailSpouseCsePerson")]
      public CsePerson DetailSpouseCsePerson
      {
        get => detailSpouseCsePerson ??= new();
        set => detailSpouseCsePerson = value;
      }

      /// <summary>
      /// A value of DetailSpouseContact.
      /// </summary>
      [JsonPropertyName("detailSpouseContact")]
      public Contact DetailSpouseContact
      {
        get => detailSpouseContact ??= new();
        set => detailSpouseContact = value;
      }

      /// <summary>
      /// A value of DetailSpouseCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailSpouseCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailSpouseCsePersonsWorkSet
      {
        get => detailSpouseCsePersonsWorkSet ??= new();
        set => detailSpouseCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailContactInd.
      /// </summary>
      [JsonPropertyName("detailContactInd")]
      public Standard DetailContactInd
      {
        get => detailContactInd ??= new();
        set => detailContactInd = value;
      }

      /// <summary>
      /// A value of DetailPrmOrSpous.
      /// </summary>
      [JsonPropertyName("detailPrmOrSpous")]
      public Standard DetailPrmOrSpous
      {
        get => detailPrmOrSpous ??= new();
        set => detailPrmOrSpous = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Standard detailSelectOpt;
      private MarriageHistory detailSpouseMarriageHistory;
      private CsePerson detailSpouseCsePerson;
      private Contact detailSpouseContact;
      private CsePersonsWorkSet detailSpouseCsePersonsWorkSet;
      private Standard detailContactInd;
      private Standard detailPrmOrSpous;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of SelectedSpouseContact.
    /// </summary>
    [JsonPropertyName("selectedSpouseContact")]
    public Contact SelectedSpouseContact
    {
      get => selectedSpouseContact ??= new();
      set => selectedSpouseContact = value;
    }

    /// <summary>
    /// A value of SelectedSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("selectedSpouseCsePerson")]
    public CsePerson SelectedSpouseCsePerson
    {
      get => selectedSpouseCsePerson ??= new();
      set => selectedSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public MarriageHistory Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    private Standard listCsePersons;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Contact selectedSpouseContact;
    private CsePerson selectedSpouseCsePerson;
    private MarriageHistory selected;
    private CsePerson csePerson;
    private Array<ExportGroup> export1;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public CsePerson Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of NoOfNonzeroChars.
    /// </summary>
    [JsonPropertyName("noOfNonzeroChars")]
    public Common NoOfNonzeroChars
    {
      get => noOfNonzeroChars ??= new();
      set => noOfNonzeroChars = value;
    }

    /// <summary>
    /// A value of FirstNonzeroPosition.
    /// </summary>
    [JsonPropertyName("firstNonzeroPosition")]
    public Common FirstNonzeroPosition
    {
      get => firstNonzeroPosition ??= new();
      set => firstNonzeroPosition = value;
    }

    /// <summary>
    /// A value of StringLength.
    /// </summary>
    [JsonPropertyName("stringLength")]
    public Common StringLength
    {
      get => stringLength ??= new();
      set => stringLength = value;
    }

    /// <summary>
    /// A value of Temp00010101.
    /// </summary>
    [JsonPropertyName("temp00010101")]
    public MarriageHistory Temp00010101
    {
      get => temp00010101 ??= new();
      set => temp00010101 = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Common work;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common setExitStateToMaintain;
    private CsePerson temp;
    private Common noOfNonzeroChars;
    private Common firstNonzeroPosition;
    private Common stringLength;
    private MarriageHistory temp00010101;
    private Common selected;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPrime.
    /// </summary>
    [JsonPropertyName("existingPrime")]
    public CsePerson ExistingPrime
    {
      get => existingPrime ??= new();
      set => existingPrime = value;
    }

    /// <summary>
    /// A value of ExistingSelected.
    /// </summary>
    [JsonPropertyName("existingSelected")]
    public CsePerson ExistingSelected
    {
      get => existingSelected ??= new();
      set => existingSelected = value;
    }

    /// <summary>
    /// A value of ExistingSpouseContact.
    /// </summary>
    [JsonPropertyName("existingSpouseContact")]
    public Contact ExistingSpouseContact
    {
      get => existingSpouseContact ??= new();
      set => existingSpouseContact = value;
    }

    /// <summary>
    /// A value of ExistingSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("existingSpouseCsePerson")]
    public CsePerson ExistingSpouseCsePerson
    {
      get => existingSpouseCsePerson ??= new();
      set => existingSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public MarriageHistory Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CsePerson existingPrime;
    private CsePerson existingSelected;
    private Contact existingSpouseContact;
    private CsePerson existingSpouseCsePerson;
    private MarriageHistory existing;
  }
#endregion
}

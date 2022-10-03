// Program: FN_NPER_MTN_NON_CASE_CLIENT, ID: 372254430, model: 746.
// Short name: SWENPERP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_NPER_MTN_NON_CASE_CLIENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnNperMtnNonCaseClient: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_NPER_MTN_NON_CASE_CLIENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnNperMtnNonCaseClient(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnNperMtnNonCaseClient.
  /// </summary>
  public FnNperMtnNonCaseClient(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#      Description
    // 01/04/97 R. Marchman			Add new security/next tran.
    // 01/17/97 H. Kennedy			Mutliple screen flow and
    // 					exitstate fixes.
    // 04/15/97 H. Kennedy-MTW                 Clear Select value on PF4
    //                                         
    // Added logic to validate
    //                                         
    // Required fields.
    //                                         
    // Added logic to populate the
    //                                         
    // CSE Person upon return from
    //                                         
    // Name list screen.
    //                                         
    // Set a case for the command
    //                                         
    // of spaces to avoid invalid
    //                                         
    // command message upon
    //                                         
    // return from another screen.
    // 10/
    // 14/98 M. Brown:    Make SSN 3 fields on
    // screen, and add edits.   Add edits for
    // phone number.  Check to see if person
    // number entered is an organization.  If
    // so, do not allow display.  Protect
    // fields if the person is case-related.
    // 08/05/2002   K.Doshi   PR149011    Fix screen help Id.
    // ----------------------------------------------------------------
    // : Dec 8, 1999, M Brown, PR#80435 - Allows conversion of the adabas system
    // flag from 'Y' (known to CSE) to 'N' (non case).  This situation exists
    // for persons who were on a closed case, and were not converted, yet the
    // system flag remained 'Y' on adabas.  As a result, the person does not
    // exist on our system, yet is considered known to cse by adabas.
    // -------------------------------------------------------------
    // 11/04/02 M. Lachowicz - Validate first, last and middle name.
    //                         Work made on PR160147.
    // -------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.BeenToName.Flag = import.BeenToName.Flag;
    export.PrevCommon.Command = import.PrevCommon.Command;
    MoveCsePerson1(import.CsePerson, export.CsePerson);
    export.PrevCsePerson.Number = import.PrevCsePerson.Number;
    export.Client.Assign(import.Client);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.PrevCsePersonsWorkSet.Assign(import.PrevCsePersonsWorkSet);
    export.Dob.Text10 = import.Dob.Text10;
    export.Protect.Flag = import.Protect.Flag;
    export.SsnWorkArea.Assign(import.SsnWorkArea);
    export.Converted.Flag = import.Converted.Flag;
    export.PrevCommon.Command = global.Command;

    // *** If a flow to name has occurred, and nothing has been passed back, re-
    // display what was there before.
    if (Equal(global.Command, "RETCSENO"))
    {
      if (IsEmpty(import.CsePersonsWorkSet.Number))
      {
        // *** Re-display what was there before.
        MoveCsePersonsWorkSet1(import.BeforeFlowCsePersonsWorkSet,
          export.CsePersonsWorkSet);
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
        MoveSsnWorkArea3(import.BeforeFlowSsnWorkArea, export.SsnWorkArea);
      }
      else
      {
        export.CsePerson.Number = import.CsePersonsWorkSet.Number;
        export.Prompt.SelectChar = "";
        global.Command = "DISPLAY";
      }
    }

    // : IF prompt = "S" but pf4 not pressed, reset prompt field,
    //   and person number.
    MoveStandard(import.Standard, export.Standard);

    if (AsChar(import.Standard.PromptField) == 'S' && !
      Equal(global.Command, "LIST"))
    {
      export.Standard.PromptField = "+";
      export.CsePerson.Number = export.CsePersonsWorkSet.Number;
    }

    // : Validate NEXT tran.
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is coming into this procedure on a next tran action.
      // ****
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "NADR") || Equal(global.Command, "NADS") || Equal
      (global.Command, "RTFRMLNK") || Equal(global.Command, "RETCSENO"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "ADD"))
    {
      if (AsChar(export.BeenToName.Flag) != 'Y')
      {
        ExitState = "NAME_SEARCH_REQUIRED";

        var field = GetField(export.Standard, "promptField");

        field.Color = "red";
        field.Highlighting = Highlighting.Normal;
        field.Protected = false;
        field.Focused = true;

        return;
      }

      // *** IF something was just added or updated, and the person number
      // *** is not spaces, make them clear the screen before proceeding
      // *** with an add.
      if (!IsEmpty(export.CsePerson.Number))
      {
        ExitState = "FN0000_CLEAR_BEFORE_ADD_PERSON";

        return;
      }
    }

    if (Equal(global.Command, "UPDATE"))
    {
      if (!Equal(export.CsePerson.Number, export.PrevCsePerson.Number))
      {
        ExitState = "FN0000_DISPLAY_BEFORE_UPDATE";

        goto Test1;
      }

      if (AsChar(export.Protect.Flag) == 'Y')
      {
        ExitState = "FN0000_PERS_CASE_RELATED";
      }
    }

Test1:

    // *** Perform edits common to update and add functions.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD")) && IsExitState
      ("ACO_NN0000_ALL_OK"))
    {
      // *****
      //  Validate mandatory fields
      // *****
      if (IsEmpty(export.CsePersonsWorkSet.FirstName))
      {
        var field = GetField(export.CsePersonsWorkSet, "firstName");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.CsePersonsWorkSet.LastName))
      {
        var field = GetField(export.CsePersonsWorkSet, "lastName");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      // 11/04/2002 M.Lachowicz Start
      UseSiCheckName();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.CsePersonsWorkSet, "firstName");

        field1.Error = true;

        var field2 = GetField(export.CsePersonsWorkSet, "lastName");

        field2.Error = true;

        var field3 = GetField(export.CsePersonsWorkSet, "middleInitial");

        field3.Error = true;

        ExitState = "SI0001_INVALID_NAME";
      }

      // 11/04/2002 M.Lachowicz End.
      // : If the area code is entered, phone number must also be present.
      if (export.Client.HomePhoneAreaCode.GetValueOrDefault() != 0)
      {
        if (export.Client.HomePhone.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Client, "homePhone");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (export.Client.WorkPhoneAreaCode.GetValueOrDefault() != 0)
      {
        if (export.Client.WorkPhone.GetValueOrDefault() == 0)
        {
          var field = GetField(export.Client, "workPhone");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
      }

      if (IsEmpty(export.CsePersonsWorkSet.Sex))
      {
        var field = GetField(export.CsePersonsWorkSet, "sex");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test2;
      }

      if (AsChar(export.CsePersonsWorkSet.Sex) == 'M' || AsChar
        (export.CsePersonsWorkSet.Sex) == 'F')
      {
      }
      else
      {
        var field = GetField(export.CsePersonsWorkSet, "sex");

        field.Error = true;

        ExitState = "INVALID_SEX";

        goto Test2;
      }

      if (export.SsnWorkArea.SsnNumPart1 == 0 && export
        .SsnWorkArea.SsnNumPart2 == 0 && export.SsnWorkArea.SsnNumPart3 == 0)
      {
        export.CsePersonsWorkSet.Ssn = "000000000";
      }
      else
      {
        // : Convert SSN to one text field, and check to make sure
        //   it's 9 characters.
        local.SsnWorkArea.ConvertOption = "2";
        MoveSsnWorkArea2(export.SsnWorkArea, local.SsnWorkArea);
        UseCabSsnConvertNumToText();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field1.Error = true;

          var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field2.Error = true;

          var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field3.Error = true;

          goto Test2;
        }

        export.CsePersonsWorkSet.Ssn = local.SsnWorkArea.SsnText9;
      }

      if (Lt(Now().Date, export.CsePersonsWorkSet.Dob))
      {
        var field = GetField(export.CsePersonsWorkSet, "dob");

        field.Error = true;

        ExitState = "INVALID_DATE_OF_BIRTH";
      }
    }

Test2:

    switch(TrimEnd(global.Command))
    {
      case "RETCSENO":
        break;
      case "RTFRMLNK":
        break;
      case "LIST":
        if (AsChar(export.Standard.PromptField) == 'S')
        {
          export.BeenToName.Flag = "Y";

          // *** Save the info currently displayed - it will be re-displayed if 
          // nothing is selected on the NAME screen.
          export.BeforeFlowSsnWorkArea.Assign(export.SsnWorkArea);
          export.BeforeFlowCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

          return;
        }
        else
        {
          var field1 = GetField(export.Standard, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "NADS":
        ExitState = "ECO_LNK_TO_NADS";

        return;
      case "NADR":
        ExitState = "ECO_LNK_TO_NADR";

        return;
      case "UPDATE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Go to bottom for field protection logic.
          break;
        }

        // : Update not allowed for case related persons.
        if (AsChar(export.Protect.Flag) == 'Y')
        {
          break;
        }

        export.Client.Number = export.CsePersonsWorkSet.Number;

        if (AsChar(export.Converted.Flag) == 'Y')
        {
          // : Dec 8, 1999, M Brown, PR#80435 - Update on converted means that
          //  the person does not yet exist on the cse person table, yet they
          //  are on adabas as known to cse.
          UseCabCreateCseClient();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("CLIENT_AE"))
            {
              ExitState = "CSE_PERSON_AE";

              var field1 = GetField(export.CsePerson, "number");

              field1.Error = true;
            }

            UseEabRollbackCics();

            return;
          }
        }
        else
        {
          UseCabUpdateCseClient();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        UseFnUpdateNonCaseAdabasPerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("ADABAS_INVALID_SSN_W_RB") || IsExitState
            ("ADABAS_DUPLICATE_SSN_W_RB"))
          {
            var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

            field1.Error = true;

            var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

            field2.Error = true;

            var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

            field3.Error = true;
          }

          UseEabRollbackCics();

          return;
        }

        // : Dec 8, 1999, M Brown, PR#80435 - Reset the 'converted' flag, which 
        // is used to tell adabas to change the system flag from 'Y' (known to
        // cse) to 'N' (non case).  This flag was set on the initial read of the
        // person.
        export.Converted.Flag = "";

        if (Equal(export.CsePersonsWorkSet.Ssn, "000000000"))
        {
          export.CsePersonsWorkSet.Ssn = "";
        }

        export.PrevCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DISPLAY":
        if (IsEmpty(export.CsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseFnCabNperReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Error in the read.  Set the phone numbers on the screen to
          //   zero, so if there was something displaying previously, it
          //   is no longer shown.
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          MoveCsePerson4(local.InitializeClient, export.Client);

          return;
        }

        // : Do not proceed if Person Number entered is an organization
        //   ("O" will be the last character of the number if true).
        if (AsChar(export.CsePerson.Type1) == 'O')
        {
          ExitState = "FN0000_PERS_IS_ORGZ";

          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          MoveCsePerson4(local.InitializeClient, export.Client);

          return;
        }

        export.Client.HomePhone =
          export.CsePerson.HomePhone.GetValueOrDefault();
        export.Client.HomePhoneAreaCode =
          export.CsePerson.HomePhoneAreaCode.GetValueOrDefault();
        export.Client.WorkPhone =
          export.CsePerson.WorkPhone.GetValueOrDefault();
        export.Client.WorkPhoneAreaCode =
          export.CsePerson.WorkPhoneAreaCode.GetValueOrDefault();

        // : Set the SSN fields for screen display.  The CAB will return
        //   the SSN in three numeric fields.
        local.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.PrevCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
        export.PrevCsePerson.Number = export.CsePerson.Number;

        if (AsChar(export.Converted.Flag) == 'Y')
        {
          ExitState = "FN0000_PF6_TO_CONVRT_TO_NON_CASE";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "ADD":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Go to bottom for field protection logic.
          break;
        }

        // : Reset the "been to name" flag, to enforce flow to NAME screen
        //   on any subsequent adds.
        export.BeenToName.Flag = "N";
        UseFnCreateNonCaseAdabasPerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("ADABAS_INVALID_SSN_W_RB") || IsExitState
            ("ADABAS_DUPLICATE_SSN_W_RB"))
          {
            var field1 = GetField(export.SsnWorkArea, "ssnNumPart1");

            field1.Error = true;

            var field2 = GetField(export.SsnWorkArea, "ssnNumPart2");

            field2.Error = true;

            var field3 = GetField(export.SsnWorkArea, "ssnNumPart3");

            field3.Error = true;
          }

          UseEabRollbackCics();

          return;
        }

        export.Client.Number = export.CsePersonsWorkSet.Number;
        UseCabCreateCseClient();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("CLIENT_AE"))
          {
            ExitState = "CSE_PERSON_AE";

            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;
          }

          UseEabRollbackCics();

          return;
        }

        // : Reset the flag to indicate that the new person is not case related.
        export.Protect.Flag = "N";
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
        MoveCsePerson3(local.Client, export.Client);
        export.PrevCsePerson.Number = export.CsePerson.Number;
        export.PrevCsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        var field = GetField(export.ZdelExportNonCaseMessage, "text30");

        field.Color = "yellow";
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = false;

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // *** Protect all fields except person# if the person is case related.
    if (AsChar(export.Protect.Flag) == 'Y')
    {
      var field1 = GetField(export.Client, "homePhone");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Client, "homePhoneAreaCode");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Client, "workPhone");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.Client, "workPhoneAreaCode");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.CsePersonsWorkSet, "firstName");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.CsePersonsWorkSet, "lastName");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.CsePersonsWorkSet, "middleInitial");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.CsePersonsWorkSet, "dob");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.CsePersonsWorkSet, "sex");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.SsnWorkArea, "ssnNumPart1");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.SsnWorkArea, "ssnNumPart2");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 = GetField(export.SsnWorkArea, "ssnNumPart3");

      field12.Color = "cyan";
      field12.Protected = true;

      export.NonCaseMessage.Text40 = "";
    }
    else if (!IsEmpty(export.CsePerson.Number) && Equal
      (export.CsePerson.Number, export.PrevCsePerson.Number) && !
      IsExitState("FN0000_PF6_TO_CONVRT_TO_NON_CASE"))
    {
      export.NonCaseMessage.Text40 = "This is a non-case related person.";

      var field = GetField(export.NonCaseMessage, "text40");

      field.Color = "yellow";
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = false;
    }
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
  }

  private static void MoveCsePerson3(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
  }

  private static void MoveCsePerson4(CsePerson source, CsePerson target)
  {
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LastTran = source.LastTran;
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
    target.MiscNum1 = source.MiscNum1;
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

  private static void MoveSsnWorkArea3(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
  }

  private void UseCabCreateCseClient()
  {
    var useImport = new CabCreateCseClient.Import();
    var useExport = new CabCreateCseClient.Export();

    MoveCsePerson3(export.Client, useImport.Client);

    Call(CabCreateCseClient.Execute, useImport, useExport);

    local.Client.Assign(useExport.Client);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(local.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, local.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.SsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabUpdateCseClient()
  {
    var useImport = new CabUpdateCseClient.Import();
    var useExport = new CabUpdateCseClient.Export();

    MoveCsePerson3(export.Client, useImport.Client);

    Call(CabUpdateCseClient.Execute, useImport, useExport);

    MoveCsePerson3(useExport.Client, export.Client);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCabNperReadCsePerson()
  {
    var useImport = new FnCabNperReadCsePerson.Import();
    var useExport = new FnCabNperReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(FnCabNperReadCsePerson.Execute, useImport, useExport);

    export.Protect.Flag = useExport.Protect.Flag;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson2(useExport.CsePerson, export.CsePerson);
    export.Converted.Flag = useExport.Converted.Flag;
  }

  private void UseFnCreateNonCaseAdabasPerson()
  {
    var useImport = new FnCreateNonCaseAdabasPerson.Import();
    var useExport = new FnCreateNonCaseAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(FnCreateNonCaseAdabasPerson.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseFnUpdateNonCaseAdabasPerson()
  {
    var useImport = new FnUpdateNonCaseAdabasPerson.Import();
    var useExport = new FnUpdateNonCaseAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    useImport.Converted.Flag = export.Converted.Flag;

    Call(FnUpdateNonCaseAdabasPerson.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
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
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCheckName()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    MoveCsePersonsWorkSet2(export.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(SiCheckName.Execute, useImport, useExport);
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
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
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
    /// A value of PrevCsePerson.
    /// </summary>
    [JsonPropertyName("prevCsePerson")]
    public CsePerson PrevCsePerson
    {
      get => prevCsePerson ??= new();
      set => prevCsePerson = value;
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
    /// A value of PrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("prevCsePersonsWorkSet")]
    public CsePersonsWorkSet PrevCsePersonsWorkSet
    {
      get => prevCsePersonsWorkSet ??= new();
      set => prevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NonCaseMessage.
    /// </summary>
    [JsonPropertyName("nonCaseMessage")]
    public WorkArea NonCaseMessage
    {
      get => nonCaseMessage ??= new();
      set => nonCaseMessage = value;
    }

    /// <summary>
    /// A value of ZdelimportNonCaseMessage.
    /// </summary>
    [JsonPropertyName("zdelimportNonCaseMessage")]
    public TextWorkArea ZdelimportNonCaseMessage
    {
      get => zdelimportNonCaseMessage ??= new();
      set => zdelimportNonCaseMessage = value;
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
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of BeforeFlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("beforeFlowCsePersonsWorkSet")]
    public CsePersonsWorkSet BeforeFlowCsePersonsWorkSet
    {
      get => beforeFlowCsePersonsWorkSet ??= new();
      set => beforeFlowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of BeforeFlowSsnWorkArea.
    /// </summary>
    [JsonPropertyName("beforeFlowSsnWorkArea")]
    public SsnWorkArea BeforeFlowSsnWorkArea
    {
      get => beforeFlowSsnWorkArea ??= new();
      set => beforeFlowSsnWorkArea = value;
    }

    /// <summary>
    /// A value of Dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public TextWorkArea Dob
    {
      get => dob ??= new();
      set => dob = value;
    }

    /// <summary>
    /// A value of Protect.
    /// </summary>
    [JsonPropertyName("protect")]
    public Common Protect
    {
      get => protect ??= new();
      set => protect = value;
    }

    /// <summary>
    /// A value of BeenToName.
    /// </summary>
    [JsonPropertyName("beenToName")]
    public Common BeenToName
    {
      get => beenToName ??= new();
      set => beenToName = value;
    }

    /// <summary>
    /// A value of PrevCommon.
    /// </summary>
    [JsonPropertyName("prevCommon")]
    public Common PrevCommon
    {
      get => prevCommon ??= new();
      set => prevCommon = value;
    }

    /// <summary>
    /// A value of Converted.
    /// </summary>
    [JsonPropertyName("converted")]
    public Common Converted
    {
      get => converted ??= new();
      set => converted = value;
    }

    private CsePerson client;
    private CsePerson csePerson;
    private CsePerson prevCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet prevCsePersonsWorkSet;
    private WorkArea nonCaseMessage;
    private TextWorkArea zdelimportNonCaseMessage;
    private AbendData abendData;
    private Common ae;
    private NextTranInfo hidden;
    private Standard standard;
    private SsnWorkArea ssnWorkArea;
    private CsePersonsWorkSet beforeFlowCsePersonsWorkSet;
    private SsnWorkArea beforeFlowSsnWorkArea;
    private TextWorkArea dob;
    private Common protect;
    private Common beenToName;
    private Common prevCommon;
    private Common converted;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
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
    /// A value of PrevCsePerson.
    /// </summary>
    [JsonPropertyName("prevCsePerson")]
    public CsePerson PrevCsePerson
    {
      get => prevCsePerson ??= new();
      set => prevCsePerson = value;
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
    /// A value of NonCaseMessage.
    /// </summary>
    [JsonPropertyName("nonCaseMessage")]
    public WorkArea NonCaseMessage
    {
      get => nonCaseMessage ??= new();
      set => nonCaseMessage = value;
    }

    /// <summary>
    /// A value of ZdelExportNonCaseMessage.
    /// </summary>
    [JsonPropertyName("zdelExportNonCaseMessage")]
    public TextWorkArea ZdelExportNonCaseMessage
    {
      get => zdelExportNonCaseMessage ??= new();
      set => zdelExportNonCaseMessage = value;
    }

    /// <summary>
    /// A value of PrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("prevCsePersonsWorkSet")]
    public CsePersonsWorkSet PrevCsePersonsWorkSet
    {
      get => prevCsePersonsWorkSet ??= new();
      set => prevCsePersonsWorkSet = value;
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
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of Protect.
    /// </summary>
    [JsonPropertyName("protect")]
    public Common Protect
    {
      get => protect ??= new();
      set => protect = value;
    }

    /// <summary>
    /// A value of YearDob.
    /// </summary>
    [JsonPropertyName("yearDob")]
    public TextWorkArea YearDob
    {
      get => yearDob ??= new();
      set => yearDob = value;
    }

    /// <summary>
    /// A value of Dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public TextWorkArea Dob
    {
      get => dob ??= new();
      set => dob = value;
    }

    /// <summary>
    /// A value of BeforeFlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("beforeFlowCsePersonsWorkSet")]
    public CsePersonsWorkSet BeforeFlowCsePersonsWorkSet
    {
      get => beforeFlowCsePersonsWorkSet ??= new();
      set => beforeFlowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of BeforeFlowSsnWorkArea.
    /// </summary>
    [JsonPropertyName("beforeFlowSsnWorkArea")]
    public SsnWorkArea BeforeFlowSsnWorkArea
    {
      get => beforeFlowSsnWorkArea ??= new();
      set => beforeFlowSsnWorkArea = value;
    }

    /// <summary>
    /// A value of BeenToName.
    /// </summary>
    [JsonPropertyName("beenToName")]
    public Common BeenToName
    {
      get => beenToName ??= new();
      set => beenToName = value;
    }

    /// <summary>
    /// A value of PrevCommon.
    /// </summary>
    [JsonPropertyName("prevCommon")]
    public Common PrevCommon
    {
      get => prevCommon ??= new();
      set => prevCommon = value;
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
    /// A value of Converted.
    /// </summary>
    [JsonPropertyName("converted")]
    public Common Converted
    {
      get => converted ??= new();
      set => converted = value;
    }

    private CsePerson client;
    private CsePerson csePerson;
    private CsePerson prevCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea nonCaseMessage;
    private TextWorkArea zdelExportNonCaseMessage;
    private CsePersonsWorkSet prevCsePersonsWorkSet;
    private AbendData abendData;
    private Common ae;
    private NextTranInfo hidden;
    private Standard standard;
    private SsnWorkArea ssnWorkArea;
    private Common protect;
    private TextWorkArea yearDob;
    private TextWorkArea dob;
    private CsePersonsWorkSet beforeFlowCsePersonsWorkSet;
    private SsnWorkArea beforeFlowSsnWorkArea;
    private Common beenToName;
    private Common prevCommon;
    private Common prompt;
    private Common converted;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Alias.
    /// </summary>
    [JsonPropertyName("alias")]
    public CsePersonsWorkSet Alias
    {
      get => alias ??= new();
      set => alias = value;
    }

    /// <summary>
    /// A value of NumericCheck.
    /// </summary>
    [JsonPropertyName("numericCheck")]
    public Common NumericCheck
    {
      get => numericCheck ??= new();
      set => numericCheck = value;
    }

    /// <summary>
    /// A value of PersonNumber.
    /// </summary>
    [JsonPropertyName("personNumber")]
    public WorkArea PersonNumber
    {
      get => personNumber ??= new();
      set => personNumber = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of LeapYear.
    /// </summary>
    [JsonPropertyName("leapYear")]
    public Common LeapYear
    {
      get => leapYear ??= new();
      set => leapYear = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of DobYear.
    /// </summary>
    [JsonPropertyName("dobYear")]
    public TextWorkArea DobYear
    {
      get => dobYear ??= new();
      set => dobYear = value;
    }

    /// <summary>
    /// A value of Dob.
    /// </summary>
    [JsonPropertyName("dob")]
    public TextWorkArea Dob
    {
      get => dob ??= new();
      set => dob = value;
    }

    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public DateWorkArea Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public CsePersonsWorkSet NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of InitializeClient.
    /// </summary>
    [JsonPropertyName("initializeClient")]
    public CsePerson InitializeClient
    {
      get => initializeClient ??= new();
      set => initializeClient = value;
    }

    /// <summary>
    /// A value of InitializeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("initializeCsePersonsWorkSet")]
    public CsePersonsWorkSet InitializeCsePersonsWorkSet
    {
      get => initializeCsePersonsWorkSet ??= new();
      set => initializeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ZdelPersonInvolvdInCseCase.
    /// </summary>
    [JsonPropertyName("zdelPersonInvolvdInCseCase")]
    public Common ZdelPersonInvolvdInCseCase
    {
      get => zdelPersonInvolvdInCseCase ??= new();
      set => zdelPersonInvolvdInCseCase = value;
    }

    private CsePersonsWorkSet alias;
    private Common numericCheck;
    private WorkArea personNumber;
    private CsePerson client;
    private Common leapYear;
    private TextWorkArea month;
    private TextWorkArea dobYear;
    private TextWorkArea dob;
    private DateWorkArea init1;
    private CsePersonsWorkSet nullDate;
    private TextWorkArea textWorkArea;
    private CsePerson csePerson;
    private SsnWorkArea ssnWorkArea;
    private CsePerson initializeClient;
    private CsePersonsWorkSet initializeCsePersonsWorkSet;
    private Common zdelPersonInvolvdInCseCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }
#endregion
}

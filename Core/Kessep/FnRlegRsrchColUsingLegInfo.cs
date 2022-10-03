// Program: FN_RLEG_RSRCH_COL_USING_LEG_INFO, ID: 371774949, model: 746.
// Short name: SWERLEGP
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
/// A program: FN_RLEG_RSRCH_COL_USING_LEG_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRlegRsrchColUsingLegInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RLEG_RSRCH_COL_USING_LEG_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRlegRsrchColUsingLegInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRlegRsrchColUsingLegInfo.
  /// </summary>
  public FnRlegRsrchColUsingLegInfo(IContext context, Import import,
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
    // *********************************************************************
    // ??/??/??        R.Delgado       Initial Development
    // 01/03/97	R. Marchman	Add new security/next tran.
    // 02/13/97        R.B.Mohapatra   Rewrote the DISPLAY Logic.
    //                                 
    // The previous logic was
    // completely irrelevant
    //                                 
    // to the context.
    // 04/28/97	Sheraz Malik	Change Current Date
    // 3/23/98		Siraj Konkader			ZDEL cleanup
    // 11/23/98	Sunya Sharp	Make changes per screen assessment.
    // **********************************************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.Case1.Number = import.Case1.Number;
    export.CaseRole.Type1 = import.CaseRole.Type1;
    export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;

    // *** Need to add logic to move import to export for cse_person_workset.  
    // Sunya Sharp 11/23/1998 ***
    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.LegalAction.StandardNumber =
        import.Group.Item.LegalAction.StandardNumber;
      export.Group.Update.Ap.Number = import.Group.Item.Ap.Number;
      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      export.Group.Update.LegalActionPerson.AccountType =
        import.Group.Item.LegalActionPerson.AccountType;
      export.Group.Update.CsePersonsWorkSet.Assign(
        import.Group.Item.CsePersonsWorkSet);

      switch(AsChar(export.Group.Item.Common.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.SelectCount.Count;
          local.SelectedChar.SelectChar = import.Group.Item.Common.SelectChar;
          export.CsePerson.Number = import.Group.Item.Ap.Number;
          export.SelectedLegalAction.StandardNumber =
            import.Group.Item.LegalAction.StandardNumber ?? "";
          export.SelectedCase.Number = import.Group.Item.Case1.Number;
          export.SelectedLegalActionPerson.AccountType =
            export.Group.Item.LegalActionPerson.AccountType;

          if (AsChar(export.Group.Item.LegalActionPerson.AccountType) == 'R')
          {
            export.SelectedAp.Number = export.Group.Item.Ap.Number;
          }
          else
          {
            export.SelectedAr.Number = export.Group.Item.Ap.Number;
          }

          break;
        default:
          ++local.SelectCount.Count;

          break;
      }

      export.Group.Next();
    }

    // *** Returns from OCTO and COMP ***
    if (Equal(global.Command, "RTFRMLNK"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        export.Group.Update.Common.SelectChar = "";
      }

      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    // *** Add logic to set export hidden next tran fields.  Sunya Sharp 11/23/
    // 1998 ***
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CaseNumber = export.SelectedCase.Number;
      export.Hidden.StandardCrtOrdNumber =
        export.SelectedLegalAction.StandardNumber ?? "";

      if (!IsEmpty(export.SelectedAp.Number))
      {
        export.Hidden.CsePersonNumber = export.SelectedAp.Number;
        export.Hidden.CsePersonNumberObligor = export.SelectedAp.Number;
        export.Hidden.CsePersonNumberAp = export.SelectedAp.Number;
      }

      if (!IsEmpty(export.SelectedAr.Number))
      {
        export.Hidden.CsePersonNumber = export.SelectedAr.Number;
        export.Hidden.CsePersonNumberObligee = export.SelectedAr.Number;
      }

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

    // *** Next tran into this screen is not allowed at this time.  Sunya Sharp 
    // 11/23/1998 ***
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

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

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "OCTO") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "COMP"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // **********************************************************************************
        // 02/13/97     R.B.Mohapatra
        //              When we flow from RCOL to this screen, three scenarios 
        // may arise :
        //              1. Only Case Number is Passed from Rcol
        //              2. Only the Legal_Action Standard Number is Passed
        //              3. Both the Case Number and Standard Number are Passed
        //              The procedure logic will treat each scenario separately 
        // and compile
        //              all the information that this screen displays for the 
        // applicable scenario.
        // **********************************************************************************
        if (IsEmpty(export.Case1.Number) && IsEmpty
          (export.LegalAction.StandardNumber))
        {
          ExitState = "RLEG_NEEDS_INPUT";

          return;
        }

        // <<<* ONLY CASE NUMBER IS PASSED *>>>
        if (!IsEmpty(export.Case1.Number) && IsEmpty
          (export.LegalAction.StandardNumber))
        {
          local.Group.Index = 0;
          local.Group.Clear();

          foreach(var item in ReadLegalActionLegalActionDetailLegalActionPerson3())
            
          {
            local.Group.Update.GrCase.Number = entities.ExistingCase.Number;
            local.Group.Update.GrCsePerson.Number =
              entities.ExistingCsePerson.Number;
            local.Group.Update.GrLegalAction.StandardNumber =
              entities.ExistingLegalAction.StandardNumber;
            local.Group.Update.GrLegalActionPerson.AccountType =
              entities.LegalActionPerson.AccountType;
            local.CsePersonsWorkSet.Number =
              local.Group.Item.GrCsePerson.Number;
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
                local.Group.Update.GrCsePersonsWorkSet);
            }

            local.Group.Next();
          }
        }

        // <<<* ONLY THE LEGAL-ACTION STANDARD_NUMBER IS PASSED *>>>
        if (IsEmpty(export.Case1.Number) && !
          IsEmpty(export.LegalAction.StandardNumber))
        {
          local.Group.Index = 0;
          local.Group.Clear();

          foreach(var item in ReadLegalActionLegalActionDetailLegalActionPerson2())
            
          {
            local.Group.Update.GrCase.Number = entities.ExistingCase.Number;
            local.Group.Update.GrCsePerson.Number =
              entities.ExistingCsePerson.Number;
            local.Group.Update.GrLegalAction.StandardNumber =
              entities.ExistingLegalAction.StandardNumber;
            local.Group.Update.GrLegalActionPerson.AccountType =
              entities.LegalActionPerson.AccountType;
            local.CsePersonsWorkSet.Number =
              local.Group.Item.GrCsePerson.Number;
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
                local.Group.Update.GrCsePersonsWorkSet);
            }

            local.Group.Next();
          }
        }

        // <<<* BOTH CASE NUMBER AND LEGAL_ACTION STANDARD_NUMBER ARE PASSED *
        // >>>
        if (!IsEmpty(export.Case1.Number) && !
          IsEmpty(export.LegalAction.StandardNumber))
        {
          local.Group.Index = 0;
          local.Group.Clear();

          foreach(var item in ReadLegalActionLegalActionDetailLegalActionPerson1())
            
          {
            local.Group.Update.GrCase.Number = entities.ExistingCase.Number;
            local.Group.Update.GrCsePerson.Number =
              entities.ExistingCsePerson.Number;
            local.Group.Update.GrLegalAction.StandardNumber =
              entities.ExistingLegalAction.StandardNumber;
            local.Group.Update.GrLegalActionPerson.AccountType =
              entities.LegalActionPerson.AccountType;
            local.CsePersonsWorkSet.Number =
              local.Group.Item.GrCsePerson.Number;
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
                local.Group.Update.GrCsePersonsWorkSet);
            }

            local.Group.Next();
          }
        }

        // ---- Filter out duplicates
        export.Group.Index = 0;
        export.Group.Clear();

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          if (Equal(local.Group.Item.GrCsePerson.Number,
            local.PreviousCsePerson.Number) && Equal
            (local.Group.Item.GrCase.Number, local.PreviousCase.Number) && Equal
            (local.Group.Item.GrLegalAction.StandardNumber,
            local.PreviousLegalAction.StandardNumber))
          {
            export.Group.Next();

            continue;
          }
          else
          {
            local.PreviousCase.Number = local.Group.Item.GrCase.Number;
            local.PreviousCsePerson.Number =
              local.Group.Item.GrCsePerson.Number;
            local.PreviousLegalAction.StandardNumber =
              local.Group.Item.GrLegalAction.StandardNumber;
            export.Group.Update.Case1.Number = local.Group.Item.GrCase.Number;
            export.Group.Update.Ap.Number = local.Group.Item.GrCsePerson.Number;
            export.Group.Update.CsePersonsWorkSet.Assign(
              local.Group.Item.GrCsePersonsWorkSet);
            export.Group.Update.LegalAction.StandardNumber =
              local.Group.Item.GrLegalAction.StandardNumber;
            export.Group.Update.LegalActionPerson.AccountType =
              local.Group.Item.GrLegalActionPerson.AccountType;
          }

          export.Group.Next();
        }

        // *** Add logic to send message when successful display.  Sunya Sharp 
        // 11/23/1998 ***
        if (export.Group.IsEmpty)
        {
          ExitState = "FN0000_NO_INFO_FOUND";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }

        break;
      case "NEXT":
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RETURN":
        // *** Add logic to make fields error when invalid code is entered or 
        // multiple rows are selected.  Sunya Sharp 11/23/1998 ***
        switch(local.SelectCount.Count)
        {
          case 0:
            break;
          case 1:
            if (AsChar(local.SelectedChar.SelectChar) == 'S')
            {
              export.RlegSelection.Flag = "L";
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!IsEmpty(export.Group.Item.Common.SelectChar))
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;
                }
              }

              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";

              return;
            }

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!IsEmpty(export.Group.Item.Common.SelectChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "OCTO":
        // *** Add logic to evaluate the selection character before flowing to 
        // OCTO.  Sunya Sharp 11/25/1998 ***
        switch(local.SelectCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            if (AsChar(local.SelectedChar.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_LST_OBLIG_BY_CRT_ORDR";
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!IsEmpty(export.Group.Item.Common.SelectChar))
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;
                }
              }

              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";
            }

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!IsEmpty(export.Group.Item.Common.SelectChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "COMP":
        // *** Add logic to flow to COMP.  This will allow the user to determine
        // if the obligor and obligee on a court order are the AR and AP on the
        // case.  Sunya Sharp 11/23/1998 ***
        switch(local.SelectCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_SELECTION_REQUIRED";

            break;
          case 1:
            if (AsChar(local.SelectedChar.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!IsEmpty(export.Group.Item.Common.SelectChar))
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;
                }
              }

              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";
            }

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!IsEmpty(export.Group.Item.Common.SelectChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        // *** Changed exit state to standard and set PF keys correctly on the 
        // screen to ensure the correct message is returned to the screen.
        // Sunya Sharp 11/23/1998 ***
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

    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePersonsWorkSet.Number =
      import.Group.Item.CsePersonsWorkSet.Number;
    useImport.CsePerson.Number = import.Group.Item.Ap.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadLegalActionLegalActionDetailLegalActionPerson1()
  {
    return ReadEach("ReadLegalActionLegalActionDetailLegalActionPerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "filedDt", local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.Type1 = db.GetString(reader, 2);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 5);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingLegalActionDetail.Number = db.GetInt32(reader, 6);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.ExistingCsePerson.Number = db.GetString(reader, 8);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 10);
        entities.ExistingCase.Number = db.GetString(reader, 11);
        entities.ExistingCase.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingLegalActionDetail.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionDetailLegalActionPerson2()
  {
    return ReadEach("ReadLegalActionLegalActionDetailLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "filedDt", local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.Type1 = db.GetString(reader, 2);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 5);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingLegalActionDetail.Number = db.GetInt32(reader, 6);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.ExistingCsePerson.Number = db.GetString(reader, 8);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 10);
        entities.ExistingCase.Number = db.GetString(reader, 11);
        entities.ExistingCase.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingLegalActionDetail.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionDetailLegalActionPerson3()
  {
    return ReadEach("ReadLegalActionLegalActionDetailLegalActionPerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "filedDt", local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.Type1 = db.GetString(reader, 2);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 5);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingLegalActionDetail.Number = db.GetInt32(reader, 6);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.ExistingCsePerson.Number = db.GetString(reader, 8);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 10);
        entities.ExistingCase.Number = db.GetString(reader, 11);
        entities.ExistingCase.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingLegalActionDetail.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePerson Ap
      {
        get => ap ??= new();
        set => ap = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalActionPerson legalActionPerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Case1 case1;
      private LegalAction legalAction;
      private CsePerson ap;
      private Common common;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private CaseRole caseRole;
    private LegalAction legalAction;
    private Case1 case1;
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
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
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
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePerson Ap
      {
        get => ap ??= new();
        set => ap = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalActionPerson legalActionPerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Case1 case1;
      private LegalAction legalAction;
      private CsePerson ap;
      private Common common;
    }

    /// <summary>
    /// A value of SelectedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("selectedLegalActionPerson")]
    public LegalActionPerson SelectedLegalActionPerson
    {
      get => selectedLegalActionPerson ??= new();
      set => selectedLegalActionPerson = value;
    }

    /// <summary>
    /// A value of SelectedAr.
    /// </summary>
    [JsonPropertyName("selectedAr")]
    public CsePerson SelectedAr
    {
      get => selectedAr ??= new();
      set => selectedAr = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePerson SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
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
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
    }

    /// <summary>
    /// A value of RlegSelection.
    /// </summary>
    [JsonPropertyName("rlegSelection")]
    public Common RlegSelection
    {
      get => rlegSelection ??= new();
      set => rlegSelection = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private LegalActionPerson selectedLegalActionPerson;
    private CsePerson selectedAr;
    private CsePerson selectedAp;
    private Case1 selectedCase;
    private LegalAction selectedLegalAction;
    private Common rlegSelection;
    private CaseRole caseRole;
    private LegalAction legalAction;
    private Case1 case1;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GrLegalActionPerson.
      /// </summary>
      [JsonPropertyName("grLegalActionPerson")]
      public LegalActionPerson GrLegalActionPerson
      {
        get => grLegalActionPerson ??= new();
        set => grLegalActionPerson = value;
      }

      /// <summary>
      /// A value of GrCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grCsePersonsWorkSet")]
      public CsePersonsWorkSet GrCsePersonsWorkSet
      {
        get => grCsePersonsWorkSet ??= new();
        set => grCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GrCase.
      /// </summary>
      [JsonPropertyName("grCase")]
      public Case1 GrCase
      {
        get => grCase ??= new();
        set => grCase = value;
      }

      /// <summary>
      /// A value of GrLegalAction.
      /// </summary>
      [JsonPropertyName("grLegalAction")]
      public LegalAction GrLegalAction
      {
        get => grLegalAction ??= new();
        set => grLegalAction = value;
      }

      /// <summary>
      /// A value of GrCsePerson.
      /// </summary>
      [JsonPropertyName("grCsePerson")]
      public CsePerson GrCsePerson
      {
        get => grCsePerson ??= new();
        set => grCsePerson = value;
      }

      /// <summary>
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalActionPerson grLegalActionPerson;
      private CsePersonsWorkSet grCsePersonsWorkSet;
      private Case1 grCase;
      private LegalAction grLegalAction;
      private CsePerson grCsePerson;
      private Common grCommon;
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
    /// A value of PreviousCase.
    /// </summary>
    [JsonPropertyName("previousCase")]
    public Case1 PreviousCase
    {
      get => previousCase ??= new();
      set => previousCase = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of PreviousCsePerson.
    /// </summary>
    [JsonPropertyName("previousCsePerson")]
    public CsePerson PreviousCsePerson
    {
      get => previousCsePerson ??= new();
      set => previousCsePerson = value;
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
    /// A value of SelectedChar.
    /// </summary>
    [JsonPropertyName("selectedChar")]
    public Common SelectedChar
    {
      get => selectedChar ??= new();
      set => selectedChar = value;
    }

    /// <summary>
    /// A value of SelectCount.
    /// </summary>
    [JsonPropertyName("selectCount")]
    public Common SelectCount
    {
      get => selectCount ??= new();
      set => selectCount = value;
    }

    private LegalAction previousLegalAction;
    private Case1 previousCase;
    private DateWorkArea current;
    private DateWorkArea initialized;
    private CsePerson previousCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<GroupGroup> group;
    private Common selectedChar;
    private Common selectCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionDetail.
    /// </summary>
    [JsonPropertyName("existingLegalActionDetail")]
    public LegalActionDetail ExistingLegalActionDetail
    {
      get => existingLegalActionDetail ??= new();
      set => existingLegalActionDetail = value;
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
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonAccount.
    /// </summary>
    [JsonPropertyName("existingCsePersonAccount")]
    public CsePersonAccount ExistingCsePersonAccount
    {
      get => existingCsePersonAccount ??= new();
      set => existingCsePersonAccount = value;
    }

    private Case1 existingCase;
    private CaseRole existingCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction existingLegalAction;
    private LegalActionDetail existingLegalActionDetail;
    private LegalActionPerson legalActionPerson;
    private CsePerson existingCsePerson;
    private CsePersonAccount existingCsePersonAccount;
  }
#endregion
}

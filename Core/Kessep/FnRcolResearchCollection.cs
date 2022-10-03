// Program: FN_RCOL_RESEARCH_COLLECTION, ID: 371774920, model: 746.
// Short name: SWERCOLP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RCOL_RESEARCH_COLLECTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRcolResearchCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RCOL_RESEARCH_COLLECTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRcolResearchCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRcolResearchCollection.
  /// </summary>
  public FnRcolResearchCollection(IContext context, Import import, Export export)
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
    // ********************************************************************
    // 01/03/97	R. Marchman	Add new security/next tran.
    // 02/12/97        R.B.Mohapatra   Modified existing code and incorporated 
    // new logic to 				communicate proper data across RARS,RAPS, EMPL, PEMP
    // ( RLEG will be fixed later)
    // 10/20/97	JF.Cailouet	Add PFkey flows for OCTO,LAPS,LACS,LACN, and NAME
    // 11/17/98	Sunya Sharp	Make changes per the screen assessment.
    // 05/17/99	Sunya Sharp	Change logic to use new action block to get cse 
    // person address.
    // *******************************************************************
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
    MoveCaseRole(import.CaseRole, export.CaseRole);
    local.CaseNo.Text10 = export.Case1.Number;
    UseEabPadLeftWithZeros();
    export.Case1.Number = local.CaseNo.Text10;
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.ApCsePerson.Assign(import.ApCsePerson);
    export.ApCsePersonAddress.Assign(import.ApCsePersonAddress);
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.ArCsePerson.Assign(import.ArCsePerson);
    export.ArCsePersonAddress.Assign(import.ArCsePersonAddress);
    export.ArCsePersonsWorkSet.Assign(import.ArCsePersonsWorkSet);
    export.Employer.Assign(import.Employer);
    export.EmployerPhone.Text10 = import.EmployerPhone.Text10;
    export.ApFromRleg.Assign(import.ApFromRleg);
    export.ArFromRleg.Assign(import.ArFromRleg);
    MoveFips(import.Fips, export.Fips);
    export.Short1.Assign(import.Short1);
    export.EmpPrompt.PromptField = import.EmpPrompt.PromptField;

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // *** Add logic to set fields.  Sunya Sharp 11/23/1998 ***
      export.Hidden.CsePersonNumber = export.ApCsePerson.Number;
      export.Hidden.CsePersonNumberAp = export.ApCsePerson.Number;
      export.Hidden.CsePersonNumberObligor = export.ApCsePerson.Number;
      export.Hidden.CsePersonNumberObligee = export.ArCsePerson.Number;
      export.Hidden.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
      export.Hidden.CaseNumber = export.Case1.Number;
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

      // *** Add logic to set fields.  Sunya Sharp 11/23/1998 ***
      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.ApCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberAp))
      {
        export.ApCsePerson.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberObligor))
      {
        export.ApCsePerson.Number = export.Hidden.CsePersonNumberObligor ?? Spaces
          (10);
      }

      export.ArCsePerson.Number = export.Hidden.CsePersonNumberObligee ?? Spaces
        (10);
      export.LegalAction.StandardNumber =
        export.Hidden.StandardCrtOrdNumber ?? "";
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      global.Command = "RTFRMLNK";
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
    // *** This Flag will be passed  only if the AP, AR, Legal or Employer info 
    // has been
    // seleted after doing the Research in the respective research screens ***
    // Returning Proc    Value of Flag
    //     RARS	      E
    //     RAPS	      R
    //     RLEG	      L
    //     EMPL	      M (no longer valid - Sunya Sharp 11/17/1998)
    //     REMP	      N
    // ----------------------------------------------
    // *** Add logic to supply data to the phone number.  Sunya Sharp 11/17/1998
    // ***
    switch(AsChar(import.PassedSelection.Flag))
    {
      case 'E':
        // *** Seleted AR info from RARS ***
        if (!IsEmpty(import.PassFromRarsRapsName.FormattedName))
        {
          export.ArCsePerson.Assign(import.PassedFromRarsAndRapsCsePerson);
          export.ArCsePersonAddress.Assign(
            import.PassedFromRarsAndRapsCsePersonAddress);
          MoveCsePersonsWorkSet(import.PassFromRarsRapsName,
            export.ArCsePersonsWorkSet);
          export.ArCsePersonsWorkSet.Char10 =
            NumberToString(export.ArCsePerson.HomePhoneAreaCode.
              GetValueOrDefault(), 13, 3) + NumberToString
            (export.ArCsePerson.HomePhone.GetValueOrDefault(), 9, 7);

          if (Verify(export.ArCsePersonsWorkSet.Char10, " 0") == 0)
          {
            export.ArCsePersonsWorkSet.Char10 = "";
          }
        }

        break;
      case 'R':
        // *** Seleted AP info from RAPS ***
        if (!IsEmpty(import.PassFromRarsRapsName.FormattedName))
        {
          export.ApCsePerson.Assign(import.PassedFromRarsAndRapsCsePerson);
          export.ApCsePersonAddress.Assign(
            import.PassedFromRarsAndRapsCsePersonAddress);
          MoveCsePersonsWorkSet(import.PassFromRarsRapsName,
            export.ApCsePersonsWorkSet);
          export.ApCsePersonsWorkSet.Char10 =
            NumberToString(export.ApCsePerson.HomePhoneAreaCode.
              GetValueOrDefault(), 13, 3) + NumberToString
            (export.ApCsePerson.HomePhone.GetValueOrDefault(), 9, 7);

          if (Verify(export.ApCsePersonsWorkSet.Char10, " 0") == 0)
          {
            export.ApCsePersonsWorkSet.Char10 = "";
          }
        }

        break;
      case 'L':
        // *** Seleted Case# and Court-order# from RLEG ***
        export.LegalAction.StandardNumber =
          import.PassedFromRlegLegalAction.StandardNumber;
        export.Case1.Number = import.PassedFromRlegCase.Number;
        MoveCaseRole(import.PassedFromRlegCaseRole, export.CaseRole);

        if (!IsEmpty(import.ApFromRleg.Number))
        {
          export.ApCsePerson.Assign(export.ApFromRleg);
        }

        if (!IsEmpty(export.ArFromRleg.Number))
        {
          export.ArCsePerson.Assign(export.ArFromRleg);
        }

        break;
      case 'N':
        // *** Seleted AP# from REMP for the employer selected from EMPL ***
        export.ApCsePerson.Assign(import.PassedFromRarsAndRapsCsePerson);
        export.Employer.Assign(import.EmployerFromRemp);
        export.EmployerPhone.Text10 =
          NumberToString(export.Employer.AreaCode.GetValueOrDefault(), 13, 3) +
          (export.Employer.PhoneNo ?? "");

        if (Verify(export.EmployerPhone.Text10, " 0") == 0)
        {
          export.EmployerPhone.Text10 = "";
        }

        break;
      default:
        // *** Nothing is seleted from RARS or RAPS; So display the existing 
        // Info ***
        break;
    }

    // *** Need to ensure that if there was a court order in the field before 
    // flowing to the other screens that it is not lost upon return is nothing
    // was selected from the other screen.  Sunya Sharp 11/17/1998 ***
    if (Equal(global.Command, "RETLACS") || Equal
      (global.Command, "RETLAPS") || Equal(global.Command, "RETLACN"))
    {
      if (!IsEmpty(export.Short1.StandardNumber))
      {
        MoveLegalAction(export.Short1, export.LegalAction);
      }
    }

    if (Equal(global.Command, "RESEARCH"))
    {
      export.ApCsePerson.Number = import.FromCrrc.ObligorPersonNumber ?? Spaces
        (10);
    }

    // *** Add logic for the return from NAME.  Sunya Sharp 11/20/1998 ***
    if (Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.PassFromRarsRapsName.Number))
      {
        export.ApCsePerson.Number = import.PassFromRarsRapsName.Number;
      }
    }

    if (Equal(global.Command, "RESEARCH") || Equal
      (global.Command, "RETRLEG") || Equal(global.Command, "RTFRMLNK") || Equal
      (global.Command, "RETNAME"))
    {
      // *** Add logic to supply data to the phone number.  Sunya Sharp 11/17/
      // 1998 ***
      if (!IsEmpty(export.ApCsePerson.Number))
      {
        local.CsePersonsWorkSet.Number = export.ApCsePerson.Number;
        UseSiReadCsePerson2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          export.ApCsePersonsWorkSet.Char10 =
            NumberToString(local.Ap.HomePhoneAreaCode.GetValueOrDefault(), 13, 3)
            + NumberToString(local.Ap.HomePhone.GetValueOrDefault(), 9, 7);

          if (Verify(export.ApCsePersonsWorkSet.Char10, " 0") == 0)
          {
            export.ApCsePersonsWorkSet.Char10 = "";
          }
        }

        // *** Add logic to use the action block 
        // FN_GET_ACTIVE_CSE_PERSON_ADDRESS to get the correct address.  Sunya
        // Sharp 11/23/1998. ***
        // *** It has been requested that we now use the action block 
        // SI_GET_CSE_PERSON_MAILING_ADDR.  Sunya Sharp 5/17/1999 ***
        UseSiGetCsePersonMailingAddr2();
      }

      if (!IsEmpty(export.ArCsePerson.Number))
      {
        local.CsePersonsWorkSet.Number = export.ArCsePerson.Number;
        UseSiReadCsePerson1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
            export.ArCsePersonsWorkSet);
          export.ArCsePersonsWorkSet.Char10 =
            NumberToString(local.Ar.HomePhoneAreaCode.GetValueOrDefault(), 13, 3)
            + NumberToString(local.Ar.HomePhone.GetValueOrDefault(), 9, 7);

          if (Verify(export.ArCsePersonsWorkSet.Char10, " 0") == 0)
          {
            export.ArCsePersonsWorkSet.Char10 = "";
          }
        }

        // *** Add logic to use the action block 
        // FN_GET_ACTIVE_CSE_PERSON_ADDRESS to get the correct address.  Sunya
        // Sharp 11/23/1998. ***
        // *** It has been requested that we now use the action block 
        // SI_GET_CSE_PERSON_MAILING_ADDR.  Sunya Sharp 5/17/1999 ***
        UseSiGetCsePersonMailingAddr1();
      }

      // *** User would like to see successful display if information is 
      // present.  And a note to display information if nothing is entered.
      // Sunya Sharp 11/23/1998 ***
      if (!IsEmpty(export.ApCsePersonsWorkSet.FormattedName) || !
        IsEmpty(export.ArCsePersonsWorkSet.FormattedName) || !
        IsEmpty(export.Case1.Number) || !
        IsEmpty(export.LegalAction.StandardNumber))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else
      {
        ExitState = "FN0000_ENTER_RESEARCH_INFO";
      }
    }

    // *** If Returns from RARS, RAPS or RLEG, then ESCAPE to display the screen
    if (Equal(global.Command, "RTFRMLNK") || Equal
      (global.Command, "RETOCTO") || Equal(global.Command, "RETLACN") || Equal
      (global.Command, "RETLACS") || Equal(global.Command, "RETLAPS") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "RETRLEG"))
    {
      return;
    }

    // *** If FLOWing from CRRC, bring all AP and CASE Info.
    //     The Command passed thru flow is "RESEARCH"
    //     Change suggested by      Tom Redmond    02-12-1997
    //            Incorporated by   R.B.Mohapatra  02-12-1997
    // --------------------------------------------------------
    // *** Add logic to pass AR information from CRRC.  Sunya Sharp 11/23/1998 *
    // **
    if (Equal(global.Command, "RESEARCH"))
    {
      export.Case1.Number = import.FromCrrc.CaseNumber ?? Spaces(10);
      export.LegalAction.StandardNumber = import.FromCrrc.CourtOrderNumber ?? ""
        ;

      if (IsEmpty(import.FromCrrc.ObligorPersonNumber))
      {
        export.ApCsePerson.Number = import.FromCrrc.ObligorPersonNumber ?? Spaces
          (10);
        export.ApCsePersonsWorkSet.FirstName =
          import.FromCrrc.ObligorFirstName ?? Spaces(12);
        export.ApCsePersonsWorkSet.MiddleInitial =
          import.FromCrrc.ObligorMiddleName ?? Spaces(1);
        export.ApCsePersonsWorkSet.LastName =
          import.FromCrrc.ObligorLastName ?? Spaces(17);
        export.ApCsePersonsWorkSet.Ssn =
          import.FromCrrc.ObligorSocialSecurityNumber ?? Spaces(9);
      }

      export.ArCsePersonsWorkSet.FirstName = import.FromCrrc.PayeeFirstName ?? Spaces
        (12);
      export.ArCsePersonsWorkSet.MiddleInitial =
        import.FromCrrc.PayeeMiddleName ?? Spaces(1);
      export.ArCsePersonsWorkSet.LastName = import.FromCrrc.PayeeLastName ?? Spaces
        (17);

      return;
    }

    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "RAPS") || Equal(global.Command, "RARS") || Equal
      (global.Command, "CRRC") || Equal(global.Command, "RLEG") || Equal
      (global.Command, "OCTO") || Equal(global.Command, "LAPS") || Equal
      (global.Command, "LACS") || Equal(global.Command, "LACN") || Equal
      (global.Command, "NAME") || Equal(global.Command, "RETRLEG") || Equal
      (global.Command, "RESEARCH") || Equal(global.Command, "RTFRMLNK"))
    {
    }
    else
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

    // **** end   group C ****
    // *** Removed extra logic for RETRLEG.  The logic was duplicated from 
    // above.  Sunya Sharp 11/23/1998 ***
    switch(TrimEnd(global.Command))
    {
      case "OCTO":
        ExitState = "ECO_LNK_TO_OCTO";

        break;
      case "LAPS":
        // *** Add logic to pass the SSN to LAPS.  Sunya Sharp 11/20/1998 ***
        if (!IsEmpty(export.ApCsePersonsWorkSet.Ssn))
        {
          export.Pass3PartSsn.SsnNumPart1 =
            (int)StringToNumber(Substring(
              export.ApCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
            1, 3));
          export.Pass3PartSsn.SsnNumPart2 =
            (int)StringToNumber(Substring(
              export.ApCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
            4, 2));
          export.Pass3PartSsn.SsnNumPart3 =
            (int)StringToNumber(Substring(
              export.ApCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
            6, 4));
        }

        export.ApCsePersonsWorkSet.Number = export.ApCsePerson.Number;
        ExitState = "ECO_LNK_TO_LAPS";

        break;
      case "LACS":
        ExitState = "ECO_LNK_TO_LACS";

        break;
      case "LACN":
        export.Short1.Classification = "J";
        ExitState = "ECO_LNK_TO_LACN";

        break;
      case "NAME":
        export.PhoneticParms.Percentage = 35;
        export.PhoneticParms.Flag = "Y";
        ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";

        break;
      case "LIST":
        // *** Add logic to make field error when there is not an "S" in the 
        // prompt field and when an invalid code is entered.  Sunya Sharp 11/23/
        // 1998 ***
        if (AsChar(export.EmpPrompt.PromptField) == 'S')
        {
          export.EmpPrompt.PromptField = "";
          ExitState = "ECO_LNK_LST_RESEARCH_EMPLOYER";
        }
        else if (IsEmpty(export.EmpPrompt.PromptField))
        {
          var field = GetField(export.EmpPrompt, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else
        {
          var field = GetField(export.EmpPrompt, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "RLEG":
        ExitState = "ECO_LNK_LST_RESEARCH_LEGAL";

        break;
      case "RAPS":
        export.PhoneticParms.Percentage = 35;
        export.PhoneticParms.Flag = "Y";
        export.ApArIndicator.Type1 = "AP";
        export.ToRars.Assign(export.ApCsePersonsWorkSet);
        ExitState = "ECO_LNK_LST_RESEARCH_PAYEE";

        break;
      case "RARS":
        export.PhoneticParms.Percentage = 35;
        export.PhoneticParms.Flag = "Y";
        export.ApArIndicator.Type1 = "AR";
        export.ToRars.Assign(export.ArCsePersonsWorkSet);
        ExitState = "ECO_LNK_LST_RESEARCH_PAYEE";

        break;
      case "CRRC":
        ExitState = "ECO_LNK_TO_REC_COLLECTION_SEC";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        // *** Set the Researched AP and LEGAL info so that they can be passed 
        // back to CRRC ***
        // Change incorporated : R.B.Mohapatra  02-12-1997
        // --------------------------------------------------------------------------------------
        export.ToCrrcCashReceiptDetail.CaseNumber = export.Case1.Number;
        export.ToCrrcCashReceiptDetail.CourtOrderNumber =
          export.LegalAction.StandardNumber ?? "";
        export.ToCrrcCashReceiptDetail.ObligorFirstName =
          export.ApCsePersonsWorkSet.FirstName;
        export.ToCrrcCashReceiptDetail.ObligorLastName =
          export.ApCsePersonsWorkSet.LastName;
        export.ToCrrcCashReceiptDetail.ObligorMiddleName =
          export.ApCsePersonsWorkSet.MiddleInitial;
        export.ToCrrcCashReceiptDetail.ObligorPersonNumber =
          export.ApCsePerson.Number;
        export.ToCrrcCashReceiptDetail.ObligorSocialSecurityNumber =
          export.ApCsePersonsWorkSet.Ssn;
        export.ToCrrcCashReceiptDetail.ObligorPhoneNumber =
          export.ApCsePersonsWorkSet.Char10;
        export.ToCrrcCashReceiptDetail.PayeeFirstName =
          export.ArCsePersonsWorkSet.FirstName;
        export.ToCrrcCashReceiptDetail.PayeeLastName =
          export.ArCsePersonsWorkSet.LastName;
        export.ToCrrcCashReceiptDetail.PayeeMiddleName =
          export.ArCsePersonsWorkSet.MiddleInitial;
        export.ToCrrcCashReceiptDetailAddress.City =
          export.ApCsePersonAddress.City ?? Spaces(30);
        export.ToCrrcCashReceiptDetailAddress.State =
          export.ApCsePersonAddress.State ?? Spaces(2);
        export.ToCrrcCashReceiptDetailAddress.Street1 =
          export.ApCsePersonAddress.Street1 ?? Spaces(25);
        export.ToCrrcCashReceiptDetailAddress.Street2 =
          export.ApCsePersonAddress.Street2 ?? "";
        export.ToCrrcCashReceiptDetailAddress.ZipCode5 =
          export.ApCsePersonAddress.ZipCode ?? Spaces(5);
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
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

    useImport.TextWorkArea.Text10 = local.CaseNo.Text10;
    useExport.TextWorkArea.Text10 = local.CaseNo.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.CaseNo.Text10 = useExport.TextWorkArea.Text10;
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

    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePersonsWorkSet.Number = import.ApCsePersonsWorkSet.Number;
    useImport.CsePerson.Number = import.ApCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiGetCsePersonMailingAddr1()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = export.ArCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, export.ArCsePersonAddress);
  }

  private void UseSiGetCsePersonMailingAddr2()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = export.ApCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, export.ApCsePersonAddress);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.Ar);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.Ap);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
    /// A value of EmployerFromRemp.
    /// </summary>
    [JsonPropertyName("employerFromRemp")]
    public Employer EmployerFromRemp
    {
      get => employerFromRemp ??= new();
      set => employerFromRemp = value;
    }

    /// <summary>
    /// A value of EmployerPhone.
    /// </summary>
    [JsonPropertyName("employerPhone")]
    public TextWorkArea EmployerPhone
    {
      get => employerPhone ??= new();
      set => employerPhone = value;
    }

    /// <summary>
    /// A value of Short1.
    /// </summary>
    [JsonPropertyName("short1")]
    public LegalAction Short1
    {
      get => short1 ??= new();
      set => short1 = value;
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
    /// A value of ArFromRleg.
    /// </summary>
    [JsonPropertyName("arFromRleg")]
    public CsePerson ArFromRleg
    {
      get => arFromRleg ??= new();
      set => arFromRleg = value;
    }

    /// <summary>
    /// A value of ApFromRleg.
    /// </summary>
    [JsonPropertyName("apFromRleg")]
    public CsePerson ApFromRleg
    {
      get => apFromRleg ??= new();
      set => apFromRleg = value;
    }

    /// <summary>
    /// A value of PassedSelection.
    /// </summary>
    [JsonPropertyName("passedSelection")]
    public Common PassedSelection
    {
      get => passedSelection ??= new();
      set => passedSelection = value;
    }

    /// <summary>
    /// A value of FromCrrc.
    /// </summary>
    [JsonPropertyName("fromCrrc")]
    public CashReceiptDetail FromCrrc
    {
      get => fromCrrc ??= new();
      set => fromCrrc = value;
    }

    /// <summary>
    /// A value of PassFromRarsRapsName.
    /// </summary>
    [JsonPropertyName("passFromRarsRapsName")]
    public CsePersonsWorkSet PassFromRarsRapsName
    {
      get => passFromRarsRapsName ??= new();
      set => passFromRarsRapsName = value;
    }

    /// <summary>
    /// A value of PassedFromRarsAndRapsCsePersonAddress.
    /// </summary>
    [JsonPropertyName("passedFromRarsAndRapsCsePersonAddress")]
    public CsePersonAddress PassedFromRarsAndRapsCsePersonAddress
    {
      get => passedFromRarsAndRapsCsePersonAddress ??= new();
      set => passedFromRarsAndRapsCsePersonAddress = value;
    }

    /// <summary>
    /// A value of PassedFromRarsAndRapsCsePerson.
    /// </summary>
    [JsonPropertyName("passedFromRarsAndRapsCsePerson")]
    public CsePerson PassedFromRarsAndRapsCsePerson
    {
      get => passedFromRarsAndRapsCsePerson ??= new();
      set => passedFromRarsAndRapsCsePerson = value;
    }

    /// <summary>
    /// A value of PassedFromRlegLegalAction.
    /// </summary>
    [JsonPropertyName("passedFromRlegLegalAction")]
    public LegalAction PassedFromRlegLegalAction
    {
      get => passedFromRlegLegalAction ??= new();
      set => passedFromRlegLegalAction = value;
    }

    /// <summary>
    /// A value of PassedFromRlegCase.
    /// </summary>
    [JsonPropertyName("passedFromRlegCase")]
    public Case1 PassedFromRlegCase
    {
      get => passedFromRlegCase ??= new();
      set => passedFromRlegCase = value;
    }

    /// <summary>
    /// A value of PassedFromRlegCaseRole.
    /// </summary>
    [JsonPropertyName("passedFromRlegCaseRole")]
    public CaseRole PassedFromRlegCaseRole
    {
      get => passedFromRlegCaseRole ??= new();
      set => passedFromRlegCaseRole = value;
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
    /// A value of EmpPrompt.
    /// </summary>
    [JsonPropertyName("empPrompt")]
    public Standard EmpPrompt
    {
      get => empPrompt ??= new();
      set => empPrompt = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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

    private Employer employerFromRemp;
    private TextWorkArea employerPhone;
    private LegalAction short1;
    private Fips fips;
    private CsePerson arFromRleg;
    private CsePerson apFromRleg;
    private Common passedSelection;
    private CashReceiptDetail fromCrrc;
    private CsePersonsWorkSet passFromRarsRapsName;
    private CsePersonAddress passedFromRarsAndRapsCsePersonAddress;
    private CsePerson passedFromRarsAndRapsCsePerson;
    private LegalAction passedFromRlegLegalAction;
    private Case1 passedFromRlegCase;
    private CaseRole passedFromRlegCaseRole;
    private LegalAction legalAction;
    private Standard empPrompt;
    private Employer employer;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonAddress arCsePersonAddress;
    private CsePerson arCsePerson;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonAddress apCsePersonAddress;
    private CsePerson apCsePerson;
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
    /// A value of Pass3PartSsn.
    /// </summary>
    [JsonPropertyName("pass3PartSsn")]
    public SsnWorkArea Pass3PartSsn
    {
      get => pass3PartSsn ??= new();
      set => pass3PartSsn = value;
    }

    /// <summary>
    /// A value of EmployerPhone.
    /// </summary>
    [JsonPropertyName("employerPhone")]
    public TextWorkArea EmployerPhone
    {
      get => employerPhone ??= new();
      set => employerPhone = value;
    }

    /// <summary>
    /// A value of Short1.
    /// </summary>
    [JsonPropertyName("short1")]
    public LegalAction Short1
    {
      get => short1 ??= new();
      set => short1 = value;
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
    /// A value of ApFromRleg.
    /// </summary>
    [JsonPropertyName("apFromRleg")]
    public CsePerson ApFromRleg
    {
      get => apFromRleg ??= new();
      set => apFromRleg = value;
    }

    /// <summary>
    /// A value of ArFromRleg.
    /// </summary>
    [JsonPropertyName("arFromRleg")]
    public CsePerson ArFromRleg
    {
      get => arFromRleg ??= new();
      set => arFromRleg = value;
    }

    /// <summary>
    /// A value of ApArIndicator.
    /// </summary>
    [JsonPropertyName("apArIndicator")]
    public CaseRole ApArIndicator
    {
      get => apArIndicator ??= new();
      set => apArIndicator = value;
    }

    /// <summary>
    /// A value of ToRars.
    /// </summary>
    [JsonPropertyName("toRars")]
    public CsePersonsWorkSet ToRars
    {
      get => toRars ??= new();
      set => toRars = value;
    }

    /// <summary>
    /// A value of ToCrrcCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("toCrrcCashReceiptDetailAddress")]
    public CashReceiptDetailAddress ToCrrcCashReceiptDetailAddress
    {
      get => toCrrcCashReceiptDetailAddress ??= new();
      set => toCrrcCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of ToCrrcCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("toCrrcCashReceiptDetail")]
    public CashReceiptDetail ToCrrcCashReceiptDetail
    {
      get => toCrrcCashReceiptDetail ??= new();
      set => toCrrcCashReceiptDetail = value;
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
    /// A value of EmpPrompt.
    /// </summary>
    [JsonPropertyName("empPrompt")]
    public Standard EmpPrompt
    {
      get => empPrompt ??= new();
      set => empPrompt = value;
    }

    /// <summary>
    /// A value of PhoneticParms.
    /// </summary>
    [JsonPropertyName("phoneticParms")]
    public Common PhoneticParms
    {
      get => phoneticParms ??= new();
      set => phoneticParms = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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

    private SsnWorkArea pass3PartSsn;
    private TextWorkArea employerPhone;
    private LegalAction short1;
    private Fips fips;
    private CsePerson apFromRleg;
    private CsePerson arFromRleg;
    private CaseRole apArIndicator;
    private CsePersonsWorkSet toRars;
    private CashReceiptDetailAddress toCrrcCashReceiptDetailAddress;
    private CashReceiptDetail toCrrcCashReceiptDetail;
    private LegalAction legalAction;
    private Standard empPrompt;
    private Common phoneticParms;
    private Employer employer;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonAddress arCsePersonAddress;
    private CsePerson arCsePerson;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePersonAddress apCsePersonAddress;
    private CsePerson apCsePerson;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of CaseNo.
    /// </summary>
    [JsonPropertyName("caseNo")]
    public TextWorkArea CaseNo
    {
      get => caseNo ??= new();
      set => caseNo = value;
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
    /// A value of ApArSelected.
    /// </summary>
    [JsonPropertyName("apArSelected")]
    public Common ApArSelected
    {
      get => apArSelected ??= new();
      set => apArSelected = value;
    }

    private CsePerson ar;
    private CsePerson ap;
    private TextWorkArea caseNo;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common apArSelected;
  }
#endregion
}
